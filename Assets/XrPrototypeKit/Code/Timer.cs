using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// Manages simple timer functions
	/// </summary>
	[System.Serializable]
	public class Timer
	{
		public float Duration
		{
			get
			{
				return _duration;
			}
		}

		public bool IsComplete
		{
			get
			{ 
				if (_duration < 0)
				{
					return false;
				}
				return _elapsedTime >= _duration; 
			}
		}

		public float t
		{
			get 
			{
				if (_duration <= 0)
				{
					return 0;
				}
				return Mathf.Clamp01(_elapsedTime / _duration);
			}
		}

		private float _duration;
		private float _elapsedTime;

		public Timer ()
		{
		}

		/// <summary>
		/// Setting a duration of less than zero (e.g. -1) ensures the timer will never complete.
		/// </summary>
		public Timer (float duration)
		{
			_duration = duration;
		}

		public void FinishNow ()
		{
			_elapsedTime = _duration;
		}

		public void Reset ()
		{
			_elapsedTime = 0;
		}

		public void StartNewTimer (float duration)
		{
			_duration = duration;
			_elapsedTime = 0;
		}

		public void Update (float deltaTime)
		{
			_elapsedTime = Mathf.Clamp(_elapsedTime + deltaTime, 0, _duration);
		}
	}
}
