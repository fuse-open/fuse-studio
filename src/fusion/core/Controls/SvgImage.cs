using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using Outracks.Diagnostics;
using Svg;

namespace Outracks.Fusion
{
	public class SvgImage : CachedImageBase
	{
		readonly Func<Stream> _svgStreamFactory;

		public SvgImage(Func<Stream> svgStreamFactory)
		{
			_svgStreamFactory = svgStreamFactory;
		}

		protected override Stream OnCreatePngStream(Ratio<Pixels, Points> scaleFactor, Optional<IColorMap> colorMap, out Ratio<Pixels, Points> imageScaleFactor)
		{
			imageScaleFactor = scaleFactor;
			return ConvertToPng(scaleFactor, colorMap);
		}


		static readonly Regex _hexColorMatchRegex = new Regex(
			"#(?<color>[0-9a-fA-F]{6}|[0-9a-fA-F]{3})\\b",
			RegexOptions.Compiled);

		// NOTE: This expects colors in SVGs to be specified with hex triplets to be recognized
		static void ApplyColorMap(XmlDocument xmlDocument, IColorMap colorMap)
		{
			foreach (var attr in xmlDocument
				.SelectNodes("//*")
				.OfType<XmlElement>()
				.SelectMany(x => x.Attributes.Cast<XmlAttribute>()))
			{
				var attrValue = attr.Value;
				foreach (var match in _hexColorMatchRegex.Matches(attrValue).Cast<Match>().Reverse())
				{
					var matchGroup = match.Groups["color"];
					var origHexColor = matchGroup.Value;
					if (origHexColor.Length == 3)
					{
						origHexColor = string.Format("{0}{0}{1}{1}{2}{2}", origHexColor[0], origHexColor[1], origHexColor[2]);
					}

					var origColor = Color.FromRgb(Convert.ToInt32(origHexColor, 16));
					var newColor = colorMap.Map(origColor);
					if (origColor != newColor)
					{
						var newHexColor = string.Format(
							"{0:x2}{1:x2}{2:x2}",
							(int) Math.Round(newColor.R * 255.0f),
							(int) Math.Round(newColor.G * 255.0f),
							(int) Math.Round(newColor.B * 255.0f));
						attrValue = attrValue.Remove(matchGroup.Index, matchGroup.Length).Insert(matchGroup.Index, newHexColor);
					}
				}

				attr.Value = attrValue;
			}
		}


		Stream ConvertToPng(Ratio<Pixels, Points> scaleFactor, Optional<IColorMap> colorMap)
		{
			using (var s = _svgStreamFactory())
			{
				// Started out with 16x16 oversampling here, but that's just ridiculous and too expensive.
				// Setting to 4x4 as I'm not able to notice a difference going above that.
				var oversampling = 4;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(s);
				colorMap.Do(cm => ApplyColorMap(xmlDocument, cm));
				var svgDoc = SvgDocument.Open(xmlDocument);
				using (var bitmap = RenderSvg(svgDoc, scaleFactor.Value, oversampling))
				using (var scaledBitmap = DownscaleByFactor(bitmap, oversampling))
				{
					var memoryStream = new MemoryStream();
					scaledBitmap.Save(memoryStream, ImageFormat.Png);
					memoryStream.Seek(0, SeekOrigin.Begin);
					return memoryStream;
				}
			}
		}

		static Bitmap RenderSvg(SvgDocument svgDoc, double scaleFactor, int oversampling)
		{
			var dimensions = svgDoc.GetDimensions();
			var finalWidth = (int)Math.Round(dimensions.Width * scaleFactor);
			var finalHeight = (int)Math.Round(dimensions.Height * scaleFactor);

			var renderWidth = finalWidth * oversampling;
			var renderHeight = finalHeight * oversampling;

			var bitmap = new Bitmap(
				renderWidth,
				renderHeight,
				PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.TextRenderingHint = TextRenderingHint.AntiAlias;
				g.PixelOffsetMode = PixelOffsetMode.Half;
				g.TextContrast = 1;
				g.SmoothingMode = SmoothingMode.None;
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.InterpolationMode = InterpolationMode.Default;
				using (var renderer = new SvgRendererWithNoSmoothControl(SvgRenderer.FromImage(bitmap)))
				{
					renderer.SetBoundable(
						new GenericBoundable(0.0f, 0.0f, bitmap.Width, bitmap.Height));
					renderer.ScaleTransform(
						bitmap.Width / dimensions.Width,
						bitmap.Height / dimensions.Height,
						MatrixOrder.Append);
					svgDoc.Overflow = SvgOverflow.Auto;
					svgDoc.RenderElement(renderer);
				}
			}

			return bitmap;
		}

		static Bitmap DownscaleByFactor(Bitmap bitmap, int downscaleFactor)
		{
			int[] sourcePixels = GetIntPixelArray(bitmap);

			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
				throw new ArgumentException("Only works 32bpp ARGB format");
			if (bitmap.Height % downscaleFactor != 0 || bitmap.Width % downscaleFactor != 0)
				throw new ArgumentException("Width and height of bitmap must be dividable by downscaleFactor", "downscaleFactor");

			var dw = bitmap.Width / downscaleFactor;
			var dh = bitmap.Height / downscaleFactor;

			var destPixels = new int[dw * dh];
			var gamma = 2.2;
			var toLinearLut = Enumerable.Range(0, 256).Select(x => Math.Pow(x / 255.0, gamma)).ToArray();

			for (int dy = 0; dy < dh; dy++)
			{
				// Scale down every AA block
				for (int dx = 0; dx < dw; dx++)
				{
					double a = 0, r = 0, g = 0, b = 0, pixelCount = 0;
					for (int suby = 0; suby < downscaleFactor; suby++)
					{
						var soffset = ((dy * downscaleFactor + suby) * (dw * downscaleFactor)) + dx * downscaleFactor;
						for (int subx = 0; subx < downscaleFactor; subx++)
						{
							var sourcePixel = sourcePixels[soffset + subx];
							var pa = ((sourcePixel >> 24) & 0xff) / 255.0;
							if (pa > 0)
							{
								pixelCount += 1;
								a += pa;

								var pr = (sourcePixel >> 16) & 0xff;
								var pg = (sourcePixel >> 8) & 0xff;
								var pb = (sourcePixel & 0xff);

								if (Platform.IsMac && pa < 1.0)
								{
									// Workaround for annoying problem with mono libgdiplus
									// Our pixels are premultiplied, but not treated as such :/
									pr = (int) Math.Min(255, pr / pa);
									pg = (int) Math.Min(255, pg / pa);
									pb = (int) Math.Min(255, pb / pa);
								}

								r += toLinearLut[pr];
								g += toLinearLut[pg];
								b += toLinearLut[pb];
							}
						}
					}

					int destPixel = 0;
					if (a > 0)
					{
						var da = (int) Math.Round(a / (downscaleFactor * downscaleFactor) * 255.0);
						Func<double, int> fromLinear = x => (int) Math.Round(Math.Pow(x / pixelCount, 1 / gamma) * 255.0);
						var dr = fromLinear(r);
						var dg = fromLinear(g);
						var db = fromLinear(b);
						destPixel = (da << 24) | (dr << 16) | (dg << 8) | db;
					}

					destPixels[dy * dw + dx] = destPixel;
				}
			}

			var scaledBitmap = new Bitmap(dw, dh, PixelFormat.Format32bppArgb);
			var rect = new System.Drawing.Rectangle(0, 0, dw, dh);
			BitmapData bdata = scaledBitmap.LockBits(rect, ImageLockMode.ReadWrite, scaledBitmap.PixelFormat);
			IntPtr ptr = bdata.Scan0;
			Marshal.Copy(destPixels, 0, ptr, destPixels.Length);
			scaledBitmap.UnlockBits(bdata);
			return scaledBitmap;
		}

		static int[] GetIntPixelArray(Bitmap bitmap)
		{
			var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
			BitmapData bdata = null;
			try
			{
				bdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
				if (bdata.Stride != bitmap.Width * 4)
				{
					throw new NotImplementedException("Assumes stride equal to width and 4 bytes per pixel");
				}
				IntPtr ptr = bdata.Scan0;
				var pixels = new int[Math.Abs(bdata.Stride) / sizeof(int) * bitmap.Height];
				Marshal.Copy(ptr, pixels, 0, pixels.Length);
				return pixels;
			}
			finally
			{
				if (bdata != null)
					bitmap.UnlockBits(bdata);
			}
		}

		#region Svg renderer for rendering with AA disabled

		class GenericBoundable : ISvgBoundable
		{
			public GenericBoundable(RectangleF rect)
			{
				Bounds = rect;
			}

			public GenericBoundable(float x, float y, float width, float height)
			{
				Bounds = new RectangleF(x, y, width, height);
			}

			public PointF Location
			{
				get { return Bounds.Location; }
			}

			public SizeF Size
			{
				get { return Bounds.Size; }
			}

			public RectangleF Bounds { get; set; }
		}

		class SvgRendererWithNoSmoothControl : ISvgRenderer
		{
			readonly ISvgRenderer _svgRendererImplementation;

			public SvgRendererWithNoSmoothControl(ISvgRenderer svgRendererImplementation)
			{
				_svgRendererImplementation = svgRendererImplementation;
			}

			public void Dispose()
			{
				_svgRendererImplementation.Dispose();
			}

			public void DrawImage(System.Drawing.Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit)
			{
				_svgRendererImplementation.DrawImage(image, destRect, srcRect, graphicsUnit);
			}

			public void DrawImageUnscaled(System.Drawing.Image image, System.Drawing.Point location)
			{
				_svgRendererImplementation.DrawImageUnscaled(image, location);
			}

			public void DrawPath(Pen pen, GraphicsPath path)
			{
				_svgRendererImplementation.DrawPath(pen, path);
			}

			public void FillPath(System.Drawing.Brush brush, GraphicsPath path)
			{
				_svgRendererImplementation.FillPath(brush, path);
			}

			public ISvgBoundable GetBoundable()
			{
				return _svgRendererImplementation.GetBoundable();
			}

			public Region GetClip()
			{
				return _svgRendererImplementation.GetClip();
			}

			public ISvgBoundable PopBoundable()
			{
				return _svgRendererImplementation.PopBoundable();
			}

			public void RotateTransform(float fAngle, MatrixOrder order = MatrixOrder.Append)
			{
				_svgRendererImplementation.RotateTransform(fAngle, order);
			}

			public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Append)
			{
				_svgRendererImplementation.ScaleTransform(sx, sy, order);
			}

			public void SetBoundable(ISvgBoundable boundable)
			{
				_svgRendererImplementation.SetBoundable(boundable);
			}

			public void SetClip(Region region, CombineMode combineMode = CombineMode.Replace)
			{
				_svgRendererImplementation.SetClip(region, combineMode);
			}

			public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Append)
			{
				_svgRendererImplementation.TranslateTransform(dx, dy, order);
			}

			public float DpiY
			{
				get { return _svgRendererImplementation.DpiY; }
			}

			// Make smoothing mode do nothing
			public SmoothingMode SmoothingMode { get; set; }

			public System.Drawing.Drawing2D.Matrix Transform
			{
				get { return _svgRendererImplementation.Transform; }
				set { _svgRendererImplementation.Transform = value; }
			}
		}
		#endregion

	}
}