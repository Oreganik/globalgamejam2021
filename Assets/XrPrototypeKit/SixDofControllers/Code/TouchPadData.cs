using UnityEngine;

namespace XrPrototypeKit.SixDofControllers
{
	public struct TouchPadData
	{
		public bool isButtonPressed;
		public bool isTouched;
		public float force;
		public ControllerType inputDeviceType;
		public int fingerId;
		public Vector2 position;
		public Vector2 velocity;

		public TouchPadData(int fingerId, ControllerType inputDeviceType)
		{
			isButtonPressed = false;
			isTouched = false;
			force = 0;
			this.inputDeviceType = inputDeviceType;
			this.fingerId = fingerId;
			position = Vector2.zero;
			velocity = Vector2.zero;
		}
	}
}
