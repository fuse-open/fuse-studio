//using System;
//using System.IO;
//using System.Reactive.Linq;
//using System.Runtime.CompilerServices;
//using NUnit.Framework;


//namespace Outracks.Fusion.Tests
//{
//	[TestFixture]
//	class UserSettingsTest
//	{
//		class SettingsStub
//		{
//			public bool BoolSetting { get; set; }
//			public Size<Points> SizeSetting { get; set; }
//		}

//		static Action<SettingsStub> DummyWriter()
//		{
//			return (s) => { };
//		}


//		//[Test]
//		//public void AskingForNonExistingPropertyReturnsOptionalNone()
//		//{
//		//	var settings = PersistantSettings<SettingsStub>.Create(new object(), (_) => { });
//		//	var notPersistantSetting = settings.Setting<bool>("Foo");

//		//	var optional = notPersistantSetting.Value.Select(v => v.Equals(Optional.None<bool>()));

//		//	Console.WriteLine("foobar");
//		//	//Assert.That(() => { }, Throws.TypeOf<PropertyNotFoundException>()); // No setting called "Foo" causes runtime exception
//		//}


//		[Test]
//		public void RequestingSettingsOfWrongTypeThrows()
//		{
//			var settings = PersistantSettings<SettingsStub>.Create(new SettingsStub(), DummyWriter());
//			Assert.That(() => { settings.Setting<int>("BoolSetting"); }, Throws.TypeOf<InvalidCastException>());
//		}

//		[Test]
//		public void BehaviorSubjectFromUserSettingsHasExpectedDefaultValue()
//		{
//			var settingsdata = new SettingsStub
//			{
//				BoolSetting = true
//			};
//			var settings = PersistantSettings<SettingsStub>.Create(settingsdata, DummyWriter());
//			var boolsettingsub = settings.Setting<bool>("BoolSetting");

//			Assert.NotNull(boolsettingsub);
//			//Assert.True(boolsettingsub.Value.Value);
//		}


//		[Test]
//		public void ChangingBehaviorSubjectUpdatesSettings()
//		{
//			//dynamic settingsdata = new ExpandoObject(); // Fails to extract properties by name
//			//settingsdata.BoolSetting = true;
//			// not anonymus object has readonly properties
//			var settingsdata = new SettingsStub{ BoolSetting = true };
			
//			var settings = PersistantSettings<SettingsStub>.Create(settingsdata, DummyWriter());
//			var boolsettingsub = settings.Setting<bool>("BoolSetting");
//			var initialValue = boolsettingsub.Value;

//			/*boolsettingsub.OnNext(!initialValue.Value);

//			Assert.That(boolsettingsub.Value == false);
//			Assert.False(settingsdata.BoolSetting);*/
//		}


//		[Test]
//		public void SizeSettingHasExpecteDefaultValue()
//		{
//			var settingsdata = new SettingsStub
//			{
//				SizeSetting = new Size<Points>(123, 456)
//			};
		
//			var settings = PersistantSettings<SettingsStub>.Create(settingsdata, DummyWriter());
//			var sizeSetting = settings.Setting<Size<Points>>("SizeSetting");

//			//Assert.AreEqual(new Points(123), sizeSetting.Value.Value.Width);
//			//Assert.AreEqual(new Points(456), sizeSetting.Value.Value.Height);
//		}


//		[Test]
//		public void UpdatingSizeBehaviorSubjectUpdatesSettings()
//		{
//			var settingsdata = new SettingsStub
//			{
//				SizeSetting = new Size<Points>(123, 456)
//			};

//			var settings = PersistantSettings<SettingsStub>.Create(settingsdata, DummyWriter());
//			var sizeSetting = settings.Setting<Size<Points>>("SizeSetting");
//			var editedSize = new Size<Points>(100, 100);
//			//sizeSetting.Value.OnNext(editedSize);

//			Assert.AreEqual(editedSize, settingsdata.SizeSetting);
//		}

//		[Test]
//		public void WriteTofile()
//		{
//			var settingsdata = new SettingsStub
//			{
//				SizeSetting = new Size<Points>(123, 456)
//			};

//			var tmpfilename = Path.GetTempFileName();
//			Console.WriteLine(tmpfilename);

//			var settings = PersistantSettings<SettingsStub>.Create(settingsdata,
//				(s) =>
//				{
//					using (var f = File.Open(tmpfilename, FileMode.OpenOrCreate, FileAccess.Write))
//					using (var sw = new StreamWriter(f))
//					{
//						sw.WriteLine(s);
//					}
//				}); 
//			var sizeSetting = settings.Setting<Size<Points>>("SizeSetting");

//		}

//		// TODO Should this test be here?
//		[Test]
//		public void PropertyOfOptionalValue()
//		{
//			var optional = Optional.Some<bool>(true);
//			Assert.That(optional.HasValue);
//			var none = Optional.None<bool>();
//			Assert.False(none.HasValue);

//			var prop = Property.Create<Optional<int>>(32);
//			var f = prop.Value.Select(p => p.HasValue ? p.Value : -2);
			
//			var fv = f.Select(v => v);
//			Assert.AreEqual(32, fv);

//			var nonoptionalprop = prop.Convert(optional1 => optional1.Or(-3), Optional.Some);

//			var g = nonoptionalprop.Value.Select(p => p);
//			Assert.AreEqual(32, g);

//		}


//	}	
//}
