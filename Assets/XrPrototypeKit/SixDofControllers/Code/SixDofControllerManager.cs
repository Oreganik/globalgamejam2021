using System;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.SixDofControllers
{
	/// <summary>
	/// Manages all controller input.
	/// </summary>
	[DefaultExecutionOrder(-100)]
	public class SixDofControllerManager : MonoBehaviour
	{
		public static Action<InputEvent> OnInputEvent;

		#region Events
		public delegate void ButtonEvent();

		public event ButtonEvent OnTriggerPressed;
		public event ButtonEvent OnTriggerReleased;
		public event ButtonEvent OnBumperPressed;
		public event ButtonEvent OnBumperReleased;
		public static Action OnHomeTap;

		public delegate void TouchpadEvent(TouchPadData data);

		public event TouchpadEvent OnTouchpadPressed;
		public event TouchpadEvent OnTouchpadReleased;
		public event TouchpadEvent OnTouchpadButtonPressed;
		public event TouchpadEvent OnTouchpadMoved;
		public event TouchpadEvent OnTouchpadButtonReleased;

		public delegate void ControllerEvent ();
		public event ControllerEvent OnControllerConnected;
		public event ControllerEvent OnControllerDisconnected;

		#endregion

		#region Public Static
		public static SixDofControllerManager Instance;
		#endregion

#if UNITY_EDITOR

		#pragma warning disable 0649 // Disable unassigned warning (fields assigned from Unity Editor)

		//displays a controller for debug purposes
		[SerializeField] private bool _enableVirtualController = true;
		//offsets the controller from the default editor controller position to simulate being held in a hand
		[SerializeField] private Vector3 _virtualControllerOffset;

		#pragma warning restore 0649

		public bool EnableVirtualController
		{
			get {return _enableVirtualController; }
		}
#endif

		#region Private Variables
		private SixDofController _activeInputDevice;
		private bool _processedThisFrame;
		private Dictionary<ControllerType, SixDofController> _registeredDevices;
		private int _degreesOfFreedom;
		private List<SixDofController> _connectedDevices;
		private Quaternion _startOrientation;
		private TouchPadData _touchpadData;
		private TouchPadData _previousTouchPadData;
		private Vector3 _startPosition;
		#endregion

		#region Accessors
		public SixDofController ActiveDevice
		{
			get
			{
				if (_activeInputDevice == null)
				{
					_activeInputDevice = GetBestActiveDevice();
				}
				return _activeInputDevice;
			}
		}

		public ControllerType DeviceType
		{
			get
			{
				if (_activeInputDevice)
				{
					return _activeInputDevice.DeviceType;
				}
				return ControllerType.None;
			}
		}

		public Vector3 ControllerPosition
		{
			get
			{
				if (_activeInputDevice)
				{
					return _activeInputDevice.transform.position;
				}
				return Vector3.zero;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns the most relevant of connected devices
		/// </summary>
		public SixDofController GetBestActiveDevice ()
		{
			SixDofController inputDevice = null;
			if (_registeredDevices.TryGetValue(ControllerType.MLControl, out inputDevice)) return inputDevice;
			if (_registeredDevices.TryGetValue(ControllerType.ViveController, out inputDevice)) return inputDevice;
			if (_registeredDevices.TryGetValue(ControllerType.XBoxController, out inputDevice)) return inputDevice;
			if (_registeredDevices.TryGetValue(ControllerType.MouseAndKeyboard, out inputDevice)) return inputDevice;
			return null;
		}

		public bool GetButtonDown(InputButton button)
		{
			if (_activeInputDevice == null) return false;
			GatherInput();
			return _activeInputDevice.GetButtonDown(button);
		}

		public bool GetButtonUp(InputButton button)
		{
			if (_activeInputDevice == null) return false;
			GatherInput();
			return _activeInputDevice.GetButtonUp(button);
		}

		public bool GetButtonPressedThisFrame(InputButton button)
		{
			if (_activeInputDevice == null) return false;
			GatherInput();
			return _activeInputDevice.GetPressedThisFrame(button);
		}

		public bool GetButtonReleasedThisFrame(InputButton button)
		{
			if (_activeInputDevice == null) return false;
			GatherInput();
			return _activeInputDevice.GetReleasedThisFrame(button);
		}

		public bool GetHomeTapThisFrame ()
		{
			if (_activeInputDevice == null) return false;
			GatherInput();
			return _activeInputDevice.HomeTapThisFrame;
		}

		public float GetTrigger()
		{
			if (_activeInputDevice == null) return 0;
			GatherInput();
			return Mathf.Abs(_activeInputDevice.GetTrigger());
		}

		public void GetSixDOF(out Vector3 position, out Quaternion rotation, out Vector3 forwardVector)
		{
			if (_activeInputDevice == null)
			{
				position = Vector3.zero;
				rotation = Quaternion.identity;
				forwardVector = Vector3.forward;
				return;
			}

			Transform controllerTransform;

			controllerTransform = _activeInputDevice.transform;

			position = controllerTransform.position;
			rotation = controllerTransform.rotation;
			forwardVector = controllerTransform.forward;

#if UNITY_EDITOR
			if (_enableVirtualController)
			{
				position += controllerTransform.TransformDirection(_virtualControllerOffset);
			}
#endif
		}

		public void RegisterDevice(SixDofController device)
		{
			if (_registeredDevices.ContainsKey(device.DeviceType) == false)
			{
				_registeredDevices.Add(device.DeviceType, device);
				_activeInputDevice = GetBestActiveDevice();

#if UNITY_EDITOR
				if (device.DeviceType == ControllerType.MLControl || device.DeviceType == ControllerType.ViveController)
				{
					_enableVirtualController = false;
				}
#endif

#if DEBUG && UNITY_EDITOR == false
				Debug.Log("Register DeviceType." + device.DeviceType.ToString() + ". Active input device is DeviceType." + _activeInputDevice.DeviceType.ToString());
#endif

				if (OnControllerConnected != null)
				{
					OnControllerConnected();
				}
			}
#if DEBUG
			else if (_registeredDevices[device.DeviceType] != device)
			{
				// allow double registrations, but warn on multiple device of same-type failures
				Debug.LogError("Register DeviceType." + device.DeviceType.ToString() + ". FAILED: Device of same type has already been added!");
			}
#endif

#if DEBUG
			if (_activeInputDevice == null)
			{
				Debug.LogWarning("Register Device: WARNING: Active Input Device is NULL!");
			}
#endif
		}

		public void SubscribeButton(InputButton inputButton, InputButtonEvent inputEvent, ButtonEvent handler)
		{
			if (inputButton == InputButton.Trigger)
			{
				if (inputEvent == InputButtonEvent.Pressed) OnTriggerPressed += handler;
				else OnTriggerReleased += handler;
			}
			else if (inputButton == InputButton.Bumper)
			{
				if (inputEvent == InputButtonEvent.Pressed) OnBumperPressed += handler;
				else OnBumperReleased += handler;
			}
		}

		public void TriggerHaptics(Haptics.VibrationPattern vibrationPattern, Haptics.Intensity intensity)
		{
			if (_activeInputDevice)
			{
				_activeInputDevice.TriggerHaptics(vibrationPattern, intensity);
			}
		}

		public void UnregisterDevice(SixDofController device, string message = "")
		{
			if (_registeredDevices.ContainsKey(device.DeviceType))
			{
				_registeredDevices.Remove(device.DeviceType);
				_activeInputDevice = GetBestActiveDevice();
#if DEBUG && UNITY_EDITOR == false
			Debug.Log("Unregister DeviceType." + device.DeviceType.ToString() + ". Reason: " + message);
#endif
				if (OnControllerDisconnected != null)
				{
					OnControllerDisconnected();
				}
			}
#if DEBUG && UNITY_EDITOR == false
			else
			{
			Debug.Log("Unregister DeviceType." + device.DeviceType.ToString() + ". Reason: " + message + ". WARNING: Was not previously registered!");
			}
#endif
		}

		public void UnsubscribeButton(InputButton inputButton, InputButtonEvent inputEvent, ButtonEvent handler)
		{
			if (inputButton == InputButton.Trigger)
			{
				if (inputEvent == InputButtonEvent.Pressed) OnTriggerPressed -= handler;
				else OnTriggerReleased -= handler;
			}
			else if (inputButton == InputButton.Bumper)
			{
				if (inputEvent == InputButtonEvent.Pressed) OnBumperPressed -= handler;
				else OnBumperReleased -= handler;
			}
		}
		#endregion

		#region Private Methods
		private void GatherInput ()
		{
			if (_processedThisFrame || _activeInputDevice == null)
			{
				return;
			}

			// Process Trigger, Bumper, and Home button inputs
			_activeInputDevice.ProcessButtonInput();

			// In this version of CoreInputManager, only the first finger on the touchpad is supported,
			// even though the Magic Leap Control can support two.  \_(o_o)_/  OH WELL
			_previousTouchPadData = _touchpadData;
			_touchpadData = _activeInputDevice.ProcessTouchpadInput(0);

			_processedThisFrame = true;
		}

		private void NotifyCallbacks()
		{
			// Trigger 
			if (OnTriggerPressed != null && GetButtonPressedThisFrame(InputButton.Trigger))
			{
				OnTriggerPressed();
			}
			else if (OnTriggerReleased != null && GetButtonReleasedThisFrame(InputButton.Trigger))
			{
				OnTriggerReleased();
			}

			// Bumper
			if (OnBumperPressed != null && GetButtonPressedThisFrame(InputButton.Bumper))
			{
				OnBumperPressed();
			}
			else if (OnBumperReleased != null && GetButtonReleasedThisFrame(InputButton.Bumper))
			{
				OnBumperReleased();
			}

			// HomeTap
			if (GetHomeTapThisFrame() && OnHomeTap != null)
			{
				OnHomeTap();
			}

			// Touchpad Touch
			if (_touchpadData.isTouched)
			{
				if (_previousTouchPadData.isTouched == false && OnTouchpadPressed != null)
				{
					OnTouchpadPressed(_touchpadData);
				}
			}
			else if (_previousTouchPadData.isTouched)
			{
				if (OnTouchpadReleased != null)
				{
					OnTouchpadReleased(_touchpadData);
				}
			}

			// Touchpad Button (aka Hard Press)
			if (_touchpadData.isButtonPressed)
			{
				if (_previousTouchPadData.isButtonPressed == false && OnTouchpadButtonPressed != null)
				{
					OnTouchpadButtonPressed(_touchpadData);
				}
			}
			else if (_previousTouchPadData.isButtonPressed)
			{
				if (OnTouchpadButtonReleased != null)
				{
					OnTouchpadButtonReleased(_touchpadData);
				}
			}

			// Touchpad Move
			if (_previousTouchPadData.isTouched == true && _touchpadData.isTouched)
			{
				if (OnTouchpadMoved != null)
				{
					OnTouchpadMoved(_touchpadData);
				}
			}
		}

		private void NotifyInputEvents()
		{
			if (OnInputEvent == null)
			{
				return;
			}

			// Trigger
			if (GetButtonPressedThisFrame(InputButton.Trigger))
			{
				OnInputEvent(InputEvent.TriggerPressed);
			}
			else if (GetButtonReleasedThisFrame(InputButton.Trigger))
			{
				OnInputEvent(InputEvent.TriggerReleased);
			}

			// Bumper
			if (GetButtonPressedThisFrame(InputButton.Bumper))
			{
				OnInputEvent(InputEvent.BumperPressed);
			}
			else if (GetButtonReleasedThisFrame(InputButton.Bumper))
			{
				OnInputEvent(InputEvent.BumperReleased);
			}

			// Home
			if (GetHomeTapThisFrame())
			{
				OnInputEvent(InputEvent.HomeTap);
			}

			// Touchpad Button (aka Hard Press)
			if (_touchpadData.isButtonPressed && _previousTouchPadData.isButtonPressed == false)
			{
				OnInputEvent(InputEvent.TouchpadButtonPressed);
			}
			else if (_touchpadData.isButtonPressed == false && _previousTouchPadData.isButtonPressed)
			{
				OnInputEvent(InputEvent.TouchpadButtonReleased);
			}
		}
		#endregion

		#region Monobehaviour Events
		private void Awake()
		{
			Instance = this;
			_registeredDevices = new Dictionary<ControllerType, SixDofController>();
			_connectedDevices = new List<SixDofController>();
		}

		private void LateUpdate()
		{
			_processedThisFrame = false;

			foreach (KeyValuePair<ControllerType, SixDofController> pair in _registeredDevices)
			{
				if (pair.Value)
				{
					pair.Value.ClearHomeTapEvent();
				}
			}
		}

		private void Update()
		{
			// PreProcess all devices. This may result in one or more disconnecting, so we work from a list.
			_connectedDevices.Clear();

			foreach (KeyValuePair<ControllerType, SixDofController> pair in _registeredDevices)
			{
				_connectedDevices.Add(pair.Value);
			}

			foreach (SixDofController inputDevice in _connectedDevices)
			{
				inputDevice.UpdateDeviceState();
			}

			GatherInput();
			NotifyCallbacks();
			NotifyInputEvents();
		}
		#endregion
	}
}
