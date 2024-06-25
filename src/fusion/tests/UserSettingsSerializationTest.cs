using Newtonsoft.Json;
using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	class UserSettingsSerializationTest
	{
		[Test]
		public void ReadCurrentPointsFormat()
		{
			var s = JsonConvert.DeserializeObject<Points>("1337", FusionJsonSerializer.Settings);
			Assert.AreEqual(new Points(1337), s);
		}

		[Test]
		public void ConvertPoints()
		{
			var s = new Points(2);
			var json = JsonConvert.SerializeObject(s, Formatting.Indented, FusionJsonSerializer.Settings);
			var s2 = JsonConvert.DeserializeObject<Points>(json, FusionJsonSerializer.Settings);
			Assert.AreEqual(s, s2);
		}

		[Test]
		public void ConvertPointOfPoints()
		{
			var p = new Point<Points>(1, 2);

			var json = JsonConvert.SerializeObject(p, Formatting.Indented, FusionJsonSerializer.Settings);
			var p2 = JsonConvert.DeserializeObject<Point<Points>>(json, FusionJsonSerializer.Settings);
			Assert.AreEqual(p, p2);
		}

		[Test]
		public void ReadCurrentPointOfPointsFormat()
		{
			var p = JsonConvert.DeserializeObject<Point<Points>>("[2,3]", FusionJsonSerializer.Settings);
			Assert.AreEqual(new Point<Points>(2, 3), p);
		}

		[Test]
		public void ConvertSizeOfPoints()
		{
			var s = new Size<Points>(2, 3);
			var json = JsonConvert.SerializeObject(s, Formatting.Indented, FusionJsonSerializer.Settings);
			var s2 = JsonConvert.DeserializeObject<Size<Points>>(json, FusionJsonSerializer.Settings);
			Assert.AreEqual(s, s2);
		}

		[Test]
		public void ReadCurrentSizeOfPointsFormat()
		{
			var s = JsonConvert.DeserializeObject<Size<Points>>("[2,  3]", FusionJsonSerializer.Settings);
			Assert.AreEqual(new Size<Points>(2, 3), s);
		}
	}
}
