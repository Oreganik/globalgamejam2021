using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.SixDofControllers
{
	public static class Haptics
	{
		public enum Intensity
		{
			Low = 0,
			Medium = 1,
			High = 2
		}
		public enum VibrationTarget
		{
			Touchpad = 0,
			Body = 1
		}
		public enum VibrationPattern
		{
			Click = 0,
			Bump = 1,
			DoubleClick = 2,
			Buzz = 3,
			Tick = 4,
			ForceDown = 5,
			ForceUp = 6,
			ForceDwell = 7,
			SecondForceDown = 8
		}
	}
}
