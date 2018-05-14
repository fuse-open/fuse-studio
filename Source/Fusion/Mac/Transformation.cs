using System;
using System.Drawing;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace Outracks.Fusion.OSX
{
	class TransformerView : NSView
	{
		public TransformerView(IntPtr handle)
			: base(handle)
		{
		}

		CGAffineTransform _currentTransform = CGAffineTransform.MakeIdentity();
		
		public override bool WantsUpdateLayer
		{
			get { return true; }
		}

		public TransformerView()
		{
			base.WantsLayer = true;		
		}

		public void ApplyTransformation(Matrix matrix)
		{
			matrix = Matrix.Multiply(
				Matrix.Translate(Frame.Width / 2, Frame.Height / 2),
				matrix,
				Matrix.Translate(-Frame.Width / 2, -Frame.Height / 2));

			var xAxis = matrix.GetRow(1);
			var yAxis = matrix.GetRow(2);

			_currentTransform = new CGAffineTransform(
				(float)xAxis.Item1,
				(float)yAxis.Item1,
				(float)xAxis.Item2,
				(float)yAxis.Item2,
				(float)xAxis.Item4,
				(float)yAxis.Item4);			
			NeedsDisplay = true;
		}

		public override void UpdateLayer()
		{
			Layer.AffineTransform = _currentTransform;
		}
	}

	class Transformation
	{
		public static void Initialize()
		{
			Fusion.Transformation.Initialize(FixedPositionFactory);
			Fusion.Transformation.Initialize(Transform);
		}

		static IControl Transform(IControl ctrl, IObservable<Matrix> matrix)
		{
			return Control.Create(location =>
			{
				ctrl.Mount(
					new MountLocation.Mutable
					{
						AvailableSize = location.AvailableSize,
						IsRooted = location.IsRooted,
						NativeFrame = ObservableMath.RectangleWithSize(ctrl.DesiredSize),
					});

				var nativeHandle = ctrl.NativeHandle as NSView;
				var containerView = new TransformerView();
				containerView.AddSubview(nativeHandle);

				location.BindNativeDefaults(containerView, Fusion.Application.MainThread);
				location.BindNativeProperty(Fusion.Application.MainThread, "transformation", matrix,
					mat =>
					{
						containerView.ApplyTransformation(mat);
					});

				return containerView;
			})
			.WithSize(
				ObservableMath
					.RectangleWithSize(ctrl.DesiredSize)
					.Transpose()
					.Transform(matrix)
					.Transpose()
					.Size);
		}

		static IControl FixedPositionFactory(IControl ctrl, Rectangle<IObservable<Points>> frame)
		{
			return Control.Create(
					mount =>
					{
						ctrl.Mount(new MountLocation.Mutable()
						{
							IsRooted = mount.IsRooted,
							AvailableSize = mount.AvailableSize,
							NativeFrame = Rectangle.FromPositionSize(
								frame.Position.X,
								mount.NativeFrame.Height
									.CombineLatest(frame.Position.Y, frame.Height)
									.Select(t => t.Item1 - t.Item2 - t.Item3), // Flip Y position
								frame.Width, 
								frame.Height)
						});

						return ctrl.NativeHandle;
					});
		}
	}
}