using UnityEngine;

namespace XrPrototypeKit.SixDofControllers
{
	public class AverageVector2OverTime : CircularBuffer<AverageVector2OverTime.Entry>
	{
		private const int TARGET_FRAME_RATE = 60;

		public struct Entry
		{
			public Vector2 Value;
			public float Timestamp;

			public Entry (Vector2 v, float t)
			{
				Value = v;
				Timestamp = t;
			}
		}

		private float _timeWindow;

		public AverageVector2OverTime(float duration) : base ((int)(duration * TARGET_FRAME_RATE))
		{
			_timeWindow = duration;
		}

		public void AddValue (Vector2 value)
		{
			base.Add(new Entry(value, Time.timeSinceLevelLoad));
		}

		public Vector2 GetAverage ()
		{
			Vector2 average = Vector2.zero;
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
				return average;
			}

//			Debug.Log(Time.frameCount + " " + count + " entries for a total of " + average.ToString("F4") + " and average of " + (average / count).ToString("F4"));

			return average / count;
		}
	}
}