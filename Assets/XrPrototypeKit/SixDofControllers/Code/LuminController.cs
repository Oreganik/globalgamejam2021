using UnityEngine;

#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace XrPrototypeKit.SixDofControllers
{
    public class LuminController : SixDofController
    {
        private const float HARD_PRESS_FORCE_BEGIN = 0.7f;
        private const float HARD_PRESS_FORCE_END = 0.6f;

        // Velocity on the touchpad is averaged over this amount of time.
        protected const float SCROLL_VELOCITY_SMOOTHING_DURATION = 0.2f;

        // A lower value means a shorter move distance is required to reach max velocity.
        // Note that the clamped velocity is "softened" by the smoothing duration for the final output.
        // Achieving max velocity for one frame out of 16 (for example) won't do much.
        protected const float SCROLL_VELOCITY_CLAMP = 0.15f;

		[Tooltip("Position/rotation smoothing. smoothingLevel = 0 means no smoothing at all,"
			+ " always use latest sample."
			+ " ('weight' will be 1-smoothingLevel)")]
		[SerializeField] private float _smoothingLevel = 0.0f;

        public override bool IsAvailable
        {
            get
            {
#if PLATFORM_LUMIN
                if (_activeController == null) return false;
                return _activeController.Connected;
#else
				return false;
#endif
            }
        }

        public override ControllerType DeviceType
        {
            get { return ControllerType.MLControl; }
        }

        public override int DegreesOfFreedom
        {
            get { return _degreesOfFreedom; }
        }

        public override int MaxTouchCount
        {
            get { return 2; }
        }

        private AverageVector2OverTime _averageVector2OverTime;
        private bool _touchActive;
        private int _degreesOfFreedom = 0;
        private Quaternion _previousRotation;
        private Vector3 _previousPosition;

#if !PLATFORM_LUMIN

		// Implement empty methods required by BaseInputDevice.
		public override float GetTrigger () { return 0; }
        public override void ProcessButtonInput() { }
		public override TouchPadData ProcessTouchpadInput (int touchId) { return new TouchPadData(); }

#else

        private MLInput.Controller _activeController;
		private int _activeControllerID = -1;

        #region Public Methods
        public override float GetTrigger()
        {
            if (IsAvailable)
            {
                return _activeController.TriggerValue;
            }
            return 0;
        }

        public override void UpdateDeviceState()
        {
            // This should never happen. But if it does, immediately unregister this input device.
            if (_activeController == null)
            {
                SixDofControllerManager.Instance.UnregisterDevice(this, "Controller reference is null");
                return;
            }

            // This should have already been detected by HandleControllerDisconnected.
            // But just in case, if we've lost connection, unregister this device.
            // TODO: Support having two controllers connected, and fall back to a connected controller. (tbrown)
            if (_activeController.Connected == false)
            {
                SixDofControllerManager.Instance.UnregisterDevice(this, "Controller is not connected");
                return;
            }
            if (_activeController.Dof == MLInput.Controller.ControlDof.Dof6)
            {
                _degreesOfFreedom = 6;

				if(_smoothingLevel <= 0.0f)
				{
					// smoothing level 0 or lower means don't smooth at all.
					transform.position = _activeController.Position;
					transform.rotation = _activeController.Orientation;
				}
				else
				{
					UpdateTransformSmooth(_activeController.Position, _activeController.Orientation, _smoothingLevel);
				}
			}
            else if (_activeController.Dof == MLInput.Controller.ControlDof.Dof3)
            {
                _degreesOfFreedom = 3;
                transform.position = Vector3.zero;
                transform.rotation = _activeController.Orientation;
            }
            else // _activeController.Dof == MLInputControllerDof.None
            {
                _degreesOfFreedom = 0;
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
            }
        }

        public override void ProcessButtonInput()
        {
            if (IsAvailable)
            {
                // Trigger Input
                bool triggerPressed = _activeController.TriggerValue > 0.8f;
                UpdateButtonState(InputButton.Trigger, triggerPressed);

                // Bumper Input
                bool bumperPressed = _activeController.IsBumperDown;
                UpdateButtonState(InputButton.Bumper, bumperPressed);

                // Bumper Haptics
                // TODO: Move haptics to their own manager and subscribe to button events or user actions. (tbrown)
                if (GetButtonState(InputButton.Bumper) == InputButtonState.Pressed)
                {
                    // TriggerHaptics(MLStudiosHaptics.VibrationPattern.ForceDown, MLStudiosHaptics.Intensity.High);
                }
                else if (GetButtonState(InputButton.Bumper) == InputButtonState.Released)
                {
                    // TriggerHaptics(MLStudiosHaptics.VibrationPattern.ForceUp, MLStudiosHaptics.Intensity.High);
                }
            }
            else
            {
                // Controller is disconnected (or tracking has failed)...
                // If a button is down when we disconnect, we'll miss the buttonUp
                // message if it's released before reconnecting. To prevent
                // weird behavior, we'll pump our button states with false to make
                // sure they cleanly exit any current state, and are prepared for a 
                // clean reconnect.  Also, this prevents unexpected behavior if the
                // system tries to fallback to 3/0 DOF.
                UpdateButtonState(InputButton.Trigger, isPressed: false);
                UpdateButtonState(InputButton.Bumper, isPressed: false);
            }
        }

        public override TouchPadData ProcessTouchpadInput(int touchId)
        {
            _previousTouchpadData = _touchpadData;
            _touchpadData = new TouchPadData(touchId, ControllerType.MLControl);

            // Early exit if the device is not available
            if (IsAvailable == false)
            {
                _touchActive = false;
                return _touchpadData;
            }

            // Early exit if no touch is detected on the controller
            bool touchId0 = touchId == 0;
            _touchpadData.isTouched = touchId0 ? _activeController.Touch1Active : _activeController.Touch2Active;
            if (_touchpadData.isTouched == false)
            {
                _touchActive = false;
                return _touchpadData;
            }

            // Get touchpad data: position and force
            Vector3 touchPosAndForce = touchId0 ? _activeController.Touch1PosAndForce : _activeController.Touch2PosAndForce;
            _touchpadData.position = new Vector2(touchPosAndForce.x, touchPosAndForce.y);
            _touchpadData.force = touchPosAndForce.z;

            // Touchpad Button Input
            if (_previousTouchpadData.isButtonPressed)
            {
                // To prevent accidental double-clicks, the button will remain pressed until force goes under a lower threshold.
                _touchpadData.isButtonPressed = _touchpadData.force > HARD_PRESS_FORCE_END;
            }
            else
            {
                _touchpadData.isButtonPressed = _touchpadData.force > HARD_PRESS_FORCE_BEGIN;
            }

            // Touchpad Button Haptics
            if (_previousTouchpadData.isButtonPressed == false && _touchpadData.isButtonPressed)
            {
                // On Press
                // TriggerHaptics(MLStudiosHaptics.VibrationPattern.ForceDown, MLStudiosHaptics.Intensity.High);
            }
            else if (_previousTouchpadData.isButtonPressed && _touchpadData.isButtonPressed == false)
            {
                // On Release
                // TriggerHaptics(MLStudiosHaptics.VibrationPattern.ForceUp, MLStudiosHaptics.Intensity.High);
            }

            // On the first touch, scroll velocity remains the default of zero.
            if (_touchActive == false)
            {
                _averageVector2OverTime.Clear();
                _touchActive = true;
            }
            // After the first touch, we have enough data to determine scroll velocity.
            else
            {
                // Clamp and normalize scroll velocity to a range of -1 to 1
                Vector2 scrollVelocity = _touchpadData.position - _previousTouchpadData.position;
                scrollVelocity.x = Mathf.Clamp(scrollVelocity.x, -SCROLL_VELOCITY_CLAMP, SCROLL_VELOCITY_CLAMP);
                scrollVelocity.y = Mathf.Clamp(scrollVelocity.y, -SCROLL_VELOCITY_CLAMP, SCROLL_VELOCITY_CLAMP);
                scrollVelocity /= SCROLL_VELOCITY_CLAMP;
                _averageVector2OverTime.AddValue(scrollVelocity);
                _touchpadData.velocity = _averageVector2OverTime.GetAverage();
            }

            return _touchpadData;
        }

        public override void TriggerHaptics(Haptics.VibrationPattern vibrationPattern, Haptics.Intensity intensity)
        {
            if (IsAvailable)
            {
                // currently there is a one to one mapping between our internal enums and the sdk enums, so casting is fine.
                _activeController.StartFeedbackPatternVibe((MLInput.Controller.FeedbackPatternVibe)vibrationPattern, (MLInput.Controller.FeedbackIntensity)intensity);
            }
        }
        #endregion

        #region Private Methods
        private void AttachController(byte id)
        {
			MLInput.Controller tempController = MLInput.GetController(id);
			Debug.Log("MLControllerInput: Controller type on attach: " + tempController.Type);
			if(tempController.Type == MLInput.Controller.ControlType.Control)
			{
				_activeController = tempController;
				_activeControllerID = id;
            SixDofControllerManager.Instance.RegisterDevice(this);
        }
			else
			{  
				Debug.Log("MLControllerInput type " + tempController.Type + "is unsupported.  Ignoring...");
			}
		}

        private void HandleControllerConnected(byte id)
        {
			Debug.Log("MLControllerInput: HandleControllerConnected! ID=" + id);
            AttachController(id);
        }

        private void HandleControllerDisconnected(byte id)
        {
			Debug.Log("MLControllerInput: HandleControllerDisconnected! ID=" + id);
			if(_activeControllerID == id)
			{
            SixDofControllerManager.Instance.UnregisterDevice(this, "Disconnected by MLInput");
				_activeControllerID = -1;
        }
			else
			{
				Debug.Log("HandleControllerDisconnected:  No registered device with that ID (" + _activeControllerID + ").  Ignoring...");
			}
		}

        private void OnButtonUp(byte controller_id, MLInput.Controller.Button button)
        {
            // Due to SDK design choices, the Home button does NOT report "button down" events.
            // It only reports "button up" events.
            if (button == MLInput.Controller.Button.HomeTap)
            {
                HomeTapThisFrame = true;
            }
        }

        private void UpdateTransformSmooth(Vector3 position, Quaternion orientation, float smoothingLevel)
        {
            // smoothingLevel = 0 means no smoothing at all, always use latest sample. 
            // ("weight" will be 1-smoothingLevel)

            // we're assuming a target frame rate of 60hz
            // if we're running slower than that, we'll adjust the weight accordingly
            // this should keep the perceived latency at constant level.
            // All that being said, here is a table of weights, along with how much latency they will
            // cause... measured in how many 60hz updates are needed to get to 95% of your target:
            // weight				frames		seconds (at 60fps)
            //	.9					2			.03
            //	.8					3			.05
            //	.7					4			.07
            //	.6					5			.08
            //	.5					6			.10
            //	.4					8			.13
            //	.3					10			.17
            //	.2					14			.23
            //	.1					30			.5

            float deltaTime = Time.deltaTime;
            float targetDelta = 0.0166f;  // we assume 60fps, but adjust weights if we're running slow
            float adjustedWeight = (1 - smoothingLevel) * (deltaTime / targetDelta);

            _previousPosition = Vector3.Lerp(_previousPosition, position, adjustedWeight);
            _previousRotation = Quaternion.Slerp(_previousRotation, orientation, adjustedWeight);

            transform.position = _previousPosition;
            transform.rotation = _previousRotation;
        }
        #endregion
#endif

        #region Monobehaviour Events

        protected void Start()
        {
#if PLATFORM_LUMIN
            _averageVector2OverTime = new AverageVector2OverTime(SCROLL_VELOCITY_SMOOTHING_DURATION);

            if (MLInput.IsStarted == false)
            {
                MLResult result = MLInput.Start();

                if (result.IsOk == false)
                {
#if DEBUG
                    Debug.LogError("MLControllerInput: Failed to start MLInput service.");
#endif
                    return;
                }
            }
            MLInput.OnControllerConnected += HandleControllerConnected;
            MLInput.OnControllerDisconnected += HandleControllerDisconnected;
            MLInput.OnControllerButtonUp += OnButtonUp;
#else
			Debug.LogError("MLControllerInput is only accessible on Lumin platforms");
			Destroy(this.gameObject);
#endif
        }

		protected override void OnEnable()
		{
			// Do nothing; prevent auto-registration in base class, which we
			// handle on connection event
		}

#if PLATFORM_LUMIN
        protected void OnDestroy()
        {
            MLInput.OnControllerConnected -= HandleControllerConnected;
            MLInput.OnControllerDisconnected -= HandleControllerDisconnected;
            MLInput.OnControllerButtonUp -= OnButtonUp;
            MLInput.Stop();
        }
#endif
        #endregion
    }

}
