using UnityEngine;

namespace XrPrototypeKit.SixDofControllers
{
	public class AverageFloatOverTime : CircularBuffer<AverageFloatOverTime.Entry>
	{
		private const int TARGET_FRAME_RATE = 60;

		public struct Entry
		{
			public float Value;
			public float Timestamp;

			public Entry (float v, float t)
			{
				Value = v;
				Timestamp = t;
			}
		}

		private float _timeWindow;

		public AverageFloatOverTime (float duration) : base ((int)(duration * TARGET_FRAME_RATE))
		{
			_timeWindow = duration;
		}

		public void AddValue (float value)
		{
			base.Add(new Entry(value, Time.timeSinceLevelLoad));
		}

		public float GetAverage ()
		{
			float average = 0;
			float count = 0;

			foreach (Entry entry in _data)
			{
				if (Time.timeSinceLevelLoad - entry.Timestamp < _timeWindow)
				{
					average += entry.Value;
					count++;
				}
			}

			if (count == 0)
			{
				return 0;
			}

			return average / count;
		}

		public float GetHighWaterMark ()
		{
			float HWM = 0.0f;

			foreach (Entry entry in _data)
			{
				if (Time.timeSinceLevelLoad - entry.Timestamp < _timeWindow)
				{
					if(HWM < entry.Value)
					{
						HWM = entry.Value;
					}
				}
			}

			return HWM;
		}

	}
}
