namespace XrPrototypeKit.SixDofControllers
{
	public enum InputEvent
	{
		BumperPressed,
		BumperReleased,
		HomeTap,
		TouchpadButtonPressed,  // This is a "hard press" over a specified force amount
		TouchpadButtonReleased,
		TouchpadMoved,
		TouchpadPressed,		// This is a "soft touch" under a specified force amount
		TouchpadReleased,
		TriggerPressed,
		TriggerReleased
	}
}