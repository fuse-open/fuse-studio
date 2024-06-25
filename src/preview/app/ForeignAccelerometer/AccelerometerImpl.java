package com.samples;

import android.hardware.*;
import android.content.Context;
import com.foreign.ForeignAccelerometer.AccelerometerUpdatedInternal_float_float_float;
import com.fuse.Activity;

public class AccelerometerImpl
{
	SensorEventListener _listener;
	SensorManager _manager;
	Sensor _sensor;

	public AccelerometerImpl(final AccelerometerUpdatedInternal_float_float_float handler)
	{
		_listener = new SensorEventListener() {
			@Override
			public void onSensorChanged(SensorEvent event) {
				float[] values = event.values;
				handler.run(values[0], values[1], values[2]);
			}

			@Override
			public void onAccuracyChanged(android.hardware.Sensor s, int a) {}
		};

		Context context = (Context)Activity.getRootActivity();
		_manager = (SensorManager) context.getSystemService(Context.SENSOR_SERVICE);
		_sensor = _manager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
	}

	public void start() {
		_manager.registerListener(_listener, _sensor, SensorManager.SENSOR_DELAY_GAME);
	}

	public void stop() {
		_manager.unregisterListener(_listener);
	}
}