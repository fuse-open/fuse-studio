using Uno;
using Uno.Collections;
using Uno.Compiler.ExportTargetInterop;


namespace ForeignAccelerometer
{
	extern(Android)
	internal class Accelerometer_Android	
	{
		extern(Android) Java.Object _impl;

		public Accelerometer_Android(AccelerometerUpdatedInternal updateDelegate)
		{
			Init(updateDelegate);
		}

		[Foreign(Language.Java)]
		protected extern(Android) void Init(AccelerometerUpdatedInternal updateDelegate)
		@{
			com.samples.AccelerometerImpl impl = new com.samples.AccelerometerImpl(updateDelegate);
			@{Accelerometer_Android:Of(_this)._impl:Set(impl)};
		@}

		[Foreign(Language.Java)]
		public extern(Android) void Start()
		@{
			com.samples.AccelerometerImpl impl = (com.samples.AccelerometerImpl) @{Accelerometer_Android:Of(_this)._impl:Get()};
			impl.start();
		@}

		[Foreign(Language.Java)]
		public extern(Android) void Stop()
		@{
			com.samples.AccelerometerImpl impl = (com.samples.AccelerometerImpl) @{Accelerometer_Android:Of(_this)._impl:Get()};
			impl.stop();
		@}
	}
}

