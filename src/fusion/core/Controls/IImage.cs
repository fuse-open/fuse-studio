namespace Outracks.Fusion
{
	public class ImageVersion<T>
	{
		public ImageVersion(Ratio<Pixels, Points> scaleFactor, T image)
		{
			ScaleFactor = scaleFactor;
			Image = image;
		}

		public Ratio<Pixels, Points> ScaleFactor { get; private set; }
		public T Image { get; private set; }
	}

	public interface IImage
	{
		/// <summary>
		/// Loads an appropriate version of the image using scaleFactor and colorMap
		/// </summary>
		ImageVersion<T> Load<T>(
			Ratio<Pixels, Points> optimalScaleFactor = default(Ratio<Pixels, Points>),
			Optional<IColorMap> colorMap = default(Optional<IColorMap>),
			bool cache = true);
	}
}