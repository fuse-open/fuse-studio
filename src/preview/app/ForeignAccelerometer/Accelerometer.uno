using Uno;
using Uno.Compiler.ExportTargetInterop;

namespace ForeignAccelerometer
{
	public class AccelerometerUpdatedArgs : EventArgs
	{
		public float3 Value { get; private set; }

		public AccelerometerUpdatedArgs(float3 value)
		{
			Value = value;
		}
	}

	public delegate void AccelerometerUpdated(object sender, AccelerometerUpdatedArgs args);
	
	public delegate void AccelerometerUpdatedInternal(float x, float y, float z);

	public class Accelerometer
	{
		public event AccelerometerUpdated Updated;

		extern(Android) Accelerometer_Android _androidImpl;
		extern(iOS) Accelerometer_iOS _iosImpl;

		public Accelerometer()
		{
			if defined(Android)
			{
				_androidImpl = new Accelerometer_Android(OnUpdate);
			}
			else if defined(iOS)
			{
				_iosImpl = new Accelerometer_iOS(OnUpdate);
			}
		}

		void OnUpdate(float x, float y, float z)
		{
			if(Updated != null)
			{
				var args = new AccelerometerUpdatedArgs(float3(x, y, z));
				Updated(this, args);
			}
		}

		extern(Android)
		public void Start() { _androidImpl.Start(); }
		extern(Android)
		public void Stop() { _androidImpl.Stop(); }

		extern(iOS)
		public void Start() { _iosImpl.Start(); }
		extern(iOS)
		public void Stop() { _iosImpl.Stop(); }

		extern(!MOBILE)
		public void Start() {}
		extern(!MOBILE)
		public void Stop() {}
	}
}
