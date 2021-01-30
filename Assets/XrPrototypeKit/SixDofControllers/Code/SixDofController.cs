using UnityEngine;

namespace XrPrototypeKit.SixDofControllers
{
	public abstract class SixDofController : MonoBehaviour
	{
		// If true, "pressed" will return true if "down" is requested, and "released" will return true if "up" is requested.
		protected const bool INCLUDE_BUTTON_TRANSITIONAL_STATES = true;

		public abstract bool IsAvailable { get; }

		public bool HomeTapThisFrame
		{
			get; protected set;
		}

		public abstract ControllerType DeviceType { get; }
		public abstract int DegreesOfFreedom { get; }

		public virtual int MaxTouchCount
		{
			get { return 1; }
		}

		public RaycastCursor RaycastCursor
		{
			get 
			{
				// if (EyeController.Instance && EyeController.Instance.IsActive)
				// {
				// 	return EyeController.Instance.RaycastCursor;
				// } 
				return _raycastCursor; 
			}
		}

		protected static readonly int s_ButtonCount = System.Enum.GetNames(typeof(InputButton)).Length;

		protected RaycastCursor _raycastCursor;
		protected InputButtonState[] _buttonStates;
		protected TouchPadData _touchpadData;
		protected TouchPadData _previousTouchpadData;

		public void ClearHomeTapEvent ()
		{
			HomeTapThisFrame = false;
		}

		public virtual bool GetButtonDown(InputButton button)
		{
			if (_buttonStates[(int)button] == InputButtonState.Down) return true;
			if (INCLUDE_BUTTON_TRANSITIONAL_STATES && _buttonStates[(int)button] == InputButtonState.Pressed) return true;
			return false;
		}

		public virtual bool GetButtonUp(InputButton button)
		{
			if (_buttonStates[(int)button] == InputButtonState.Up) return true;
			if (INCLUDE_BUTTON_TRANSITIONAL_STATES && _buttonStates[(int)button] == InputButtonState.Released) return true;
			return false;
		}
		
		public virtual bool GetPressedThisFrame(InputButton button)
		{
			return _buttonStates[(int)button] == InputButtonState.Pressed;
		}

		public virtual bool GetReleasedThisFrame(InputButton button)
		{
			return _buttonStates[(int)button] == InputButtonState.Released;
		}

		public abstract float GetTrigger();
		public abstract void ProcessButtonInput();
		public abstract TouchPadData ProcessTouchpadInput(int touchId);

		public virtual void UpdateDeviceState ()
		{
		}

		public virtual void TriggerHaptics(Haptics.VibrationPattern vibrationPattern, Haptics.Intensity intensity)
		{
		}

		protected void ClearAllButtonStates ()
		{
			for (int i = 0; i < s_ButtonCount; i++)
			{
				_buttonStates[i] = InputButtonState.Up;
			}
		}

		protected InputButtonState GetButtonState(InputButton button)
		{
			return _buttonStates[(int)button];
		}

		protected void UpdateButtonState(InputButton button, bool isPressed)
		{
			InputButtonState currentState = _buttonStates[(int)button];
			InputButtonState nextState = InputButtonState.Up;

			if (isPressed)
			{
				switch (currentState)
				{
					case InputButtonState.Up: nextState = InputButtonState.Pressed; break;
					case InputButtonState.Pressed: nextState = InputButtonState.Down; break;
					case InputButtonState.Down: nextState = InputButtonState.Down; break;
					case InputButtonState.Released: nextState = InputButtonState.Pressed; break;
				}
			}
			else
			{
				switch (currentState)
				{
					case InputButtonState.Up: nextState = InputButtonState.Up; break;
					case InputButtonState.Pressed: nextState = InputButtonState.Released; break;
					case InputButtonState.Down: nextState = InputButtonState.Released; break;
					case InputButtonState.Released: nextState = InputButtonState.Up; break;
				}
			}

			_buttonStates[(int)button] = nextState;
		}

		#region Monobehaviour Events
		protected virtual void Awake()
		{
			_buttonStates = new InputButtonState[s_ButtonCount];
			_touchpadData = new TouchPadData(0, DeviceType);
			_previousTouchpadData = new TouchPadData(0, DeviceType);
			_raycastCursor = GetComponent<RaycastCursor>();

			ContentTools contentTools = GetComponentInChildren<ContentTools>();
			if (contentTools)
			{
				contentTools.SetCursor(_raycastCursor);
			}
		}

		protected virtual void OnEnable()
		{
			if (SixDofControllerManager.Instance)
			{
				SixDofControllerManager.Instance.RegisterDevice(this);
			}
		}

		protected virtual void OnDisable()
		{
			if (SixDofControllerManager.Instance)
			{
				SixDofControllerManager.Instance.UnregisterDevice(this);
			}
		}

		protected virtual void Update ()
		{
			_raycastCursor.Scan(transform.position, transform.forward);
		}
		#endregion
	}
}
