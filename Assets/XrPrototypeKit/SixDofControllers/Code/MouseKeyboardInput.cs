using UnityEngine;

namespace XrPrototypeKit.SixDofControllers
{
	public class MouseKeyboardInput : SixDofController
	{
		private const KeyCode TOUCHPAD_HARD_PRESS = KeyCode.Space;

		private const float KEEP_SCROLL_ALIVE_DURATION = 0.1f;
		private const float MOUSE_SCROLL_WHEEL_MULTIPLIER = 5f;
		private const float KEYBOARD_SCROLL_MULTIPLIER = 10;
		private const float SCROLL_WHEEL_VELOCITY_CLAMP = 0.3f;
		protected const float SCROLL_VELOCITY_SMOOTHING_DURATION = 0.2f;

		public bool HasFocus
		{
			get { return _hasFocus; }
		}

#if UNITY_EDITOR
		public override bool IsAvailable
		{
			get { return true; }
		}
#else
		public override bool IsAvailable
		{
			get { return false; }
		}
#endif

		public override ControllerType DeviceType
		{
			get { return ControllerType.MouseAndKeyboard; }
		}

		public override int DegreesOfFreedom
		{
			get { return 0; }
		}

		private AverageFloatOverTime _scrollWheelBuffer;
		private bool _hasFocus;
		private bool[] _isUsingKeyboardButton;
		private float _keepScrollAliveTimer;
		private float _prevScrollWheelInput;
		private KeyCode _primaryButtonKey = KeyCode.Space;
		private KeyCode _secondaryButtonKey = KeyCode.Return;

		public override float GetTrigger()
		{
			if (_hasFocus == false) return 0;
			return GetButtonDown(InputButton.Trigger) ? 1 : 0;
		}

		public override void ProcessButtonInput ()
		{
			if (_hasFocus)
			{
				ProcessButton(InputButton.Trigger, 0, _primaryButtonKey);
				ProcessButton(InputButton.Bumper, 1, _secondaryButtonKey);

				// original
				// UpdateButtonState(InputButton.Trigger, Input.GetMouseButton(0));
				// UpdateButtonState(InputButton.Bumper, Input.GetMouseButton(1));

				// This simulates the ML Control SDK, which only reports "button up" events for the Home button.
				// See MLControllerInput for more details.
				HomeTapThisFrame = Input.GetMouseButtonUp(2) || Input.GetKeyUp(KeyCode.H);
			}
			// don't reset button states if focus is lost: sometimes we want to maintain them across app focus events
		}

		private void ProcessButton (InputButton button, int mouseId, KeyCode keyCode)
		{
			// If the button is up, then either the keycode or mouse button can change its state
			if (GetButtonState(button) == InputButtonState.Up)
			{
				if (Input.GetMouseButton(mouseId))
				{
					UpdateButtonState(button, true);
					_isUsingKeyboardButton[mouseId] = false;
				}
				else if (Input.GetKey(keyCode))
				{
					UpdateButtonState(button, true);
					_isUsingKeyboardButton[mouseId] = true;
				}
			}
			else
			{
				if (_isUsingKeyboardButton[mouseId])
				{
					UpdateButtonState(button, Input.GetKey(keyCode));
				}
				else
				{
					UpdateButtonState(button, Input.GetMouseButton(mouseId));
				}
			}
		}

		public override TouchPadData ProcessTouchpadInput (int touchId)
		{
			_previousTouchpadData = _touchpadData;

			TouchPadData touchData = new TouchPadData(touchId, ControllerType.MouseAndKeyboard);

			if (_hasFocus == false || touchId >= MaxTouchCount)
			{
				return touchData;
			}

			bool touchpadButtonPressed = Input.GetKey(TOUCHPAD_HARD_PRESS);
			touchData.isButtonPressed = touchpadButtonPressed;

			touchData.position += Input.GetKey(KeyCode.I) ? Vector2.up : Vector2.zero;
			touchData.position += Input.GetKey(KeyCode.K) ? Vector2.down : Vector2.zero;
			touchData.position += Input.GetKey(KeyCode.J) ? Vector2.left : Vector2.zero;
			touchData.position += Input.GetKey(KeyCode.L) ? Vector2.right : Vector2.zero;

			// Early exit for keyboard input
			if (touchData.position != Vector2.zero)
			{
				touchData.position *= Time.deltaTime * KEYBOARD_SCROLL_MULTIPLIER;
				
				touchData.isTouched = true;
				touchData.fingerId = 0;
				touchData.force = touchpadButtonPressed ? 1 : 0.5f;
				return touchData;
			}

			// The axis "Mouse ScrollWheel" typically returns velocity values between -.3 and .3.
			// But the input is technically unbound, so we clamp it, then normalize it to a -1 to 1 range.
			float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
			scrollWheelInput = Mathf.Clamp(scrollWheelInput, -SCROLL_WHEEL_VELOCITY_CLAMP, SCROLL_WHEEL_VELOCITY_CLAMP);
			scrollWheelInput /= SCROLL_WHEEL_VELOCITY_CLAMP;

			if (Mathf.Abs(scrollWheelInput) > 0.01f)
			{
				_keepScrollAliveTimer = 0;
			}

			if (_keepScrollAliveTimer < KEEP_SCROLL_ALIVE_DURATION)
			{
				_keepScrollAliveTimer += Time.deltaTime;

				// With poor quality mice, input can return a zero even in the middle of a scroll.
				// So if the input is zero but we haven't timed out, use the previous value.
				if (Mathf.Abs(scrollWheelInput) < 0.01f)
				{
					scrollWheelInput = _prevScrollWheelInput;
				}
				// otherwise, store this input as the previous value
				else
				{
					_prevScrollWheelInput = scrollWheelInput;
				}

				_scrollWheelBuffer.AddValue(scrollWheelInput);
				float averageScrollVelocity = _scrollWheelBuffer.GetAverage();

				bool useHorizontalAxis = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
				Vector2 scrollWheelVelocity = useHorizontalAxis ? Vector2.right * averageScrollVelocity : Vector2.up * averageScrollVelocity;
				touchData.velocity = scrollWheelVelocity;
				touchData.force = touchpadButtonPressed ? 1 : 0.5f;
				touchData.isTouched = true;

				// The scroll wheel has nothing that describes position.
				// But since we are clamping values from -1 to 1 (like a real touchpad), it will register as valid.
				touchData.position = scrollWheelVelocity;
			}
			else
			{
				_scrollWheelBuffer.Clear();
			}

			return touchData;
		}

		#region Monobehaviour Events
		protected override void Awake()
		{
			base.Awake();
			_hasFocus = true;
			_scrollWheelBuffer = new AverageFloatOverTime(SCROLL_VELOCITY_SMOOTHING_DURATION);
			_isUsingKeyboardButton = new bool[2];
		}

		private void OnApplicationFocus(bool focusState)
		{
			_hasFocus = focusState;
		}

		protected override void Update ()
		{
			base.Update();

			// Toggle in/out with escape regardless of actual focus
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				_hasFocus = !_hasFocus;

				// Toggle locking cursor
				UnityEngine.Cursor.lockState = (_hasFocus) ? CursorLockMode.Locked : CursorLockMode.None;
			}
            if (Input.GetKey(KeyCode.Comma))
            {
                transform.Rotate(Vector3.forward * 180 * Time.deltaTime, Space.Self);
            }

            if (Input.GetKey(KeyCode.Period))
            {
                transform.Rotate(Vector3.forward * -180 * Time.deltaTime, Space.Self);
            }
			// Mouse click restores focus
			if (_hasFocus == false && Input.GetMouseButtonDown(0))
			{
				_hasFocus = true;
			}
		}
		#endregion
	}
}
