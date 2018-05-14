using Uno;
using Uno.UX;
using Fuse.Scripting;
using Fuse;

namespace ForeignAccelerometer
{
	[UXGlobalModule]
	public class AccelerometerModule : NativeEventEmitterModule
	{
		static readonly AccelerometerModule _instance;

		readonly Accelerometer _accelerometer;

		public AccelerometerModule() : base(false, "update")
		{
			if(_instance != null) return;
			Resource.SetGlobalKey(_instance = this, "Accelerometer");
			_accelerometer = new Accelerometer();

			AddMember(new NativeFunction("start", (NativeCallback)_start));
			AddMember(new NativeFunction("stop", (NativeCallback)_stop));
		}

		void OnUpdated(object sender, AccelerometerUpdatedArgs args)
		{
			var v = args.Value;
			Emit("update", Time.FrameTime, v.X, v.Y, v.Z);
		}

		object _start(Context c, object[] args)
		{
			_accelerometer.Updated += OnUpdated;
			_accelerometer.Start();
			return null;
		}

		object _stop(Context c, object[] args)
		{
			_accelerometer.Stop();
			_accelerometer.Updated -= OnUpdated;
			return null;
		}
	}
}

