using UnityEngine;

namespace XrPrototypeKit.SixDofControllers
{
	public class MouseKeyboardCamera : MonoBehaviour
	{
		[SerializeField] private float _moveSpeed = 1;
		[SerializeField] private float _rollSpeed = 90;
		[Tooltip("If false, player will fly through scene on forward/back")]
		[SerializeField] private bool _isWalkingCamera = true;

		private const KeyCode UP = KeyCode.E;
		private const KeyCode DOWN = KeyCode.Q;
		private const KeyCode SPRINT = KeyCode.LeftShift;
		private const KeyCode ROLL_LEFT = KeyCode.LeftBracket;
		private const KeyCode ROLL_RIGHT = KeyCode.RightBracket;
		private const KeyCode ROLL_RESET = KeyCode.Backspace;
		
		private float _roll;
		// There is a huge jump on OSX the first time the mouse is moved after the cursor is locked.
		// So we wait a few frames before using mouse input.
		// https://issuetracker.unity3d.com/issues/input-dot-getaxis-mouse-first-given-value-is-wrong-after-cursor-dot-lockstate-is-set-to-cursorlockmode-dot-locked
		private int _fixedInitialInputError; 
		private MouseKeyboardInput _mouseKeyboardInput;
		private Transform _cameraTransform;

		private void Start()
		{
			_cameraTransform = Camera.main.transform;
		}

		private void Update()
		{
			if (!_mouseKeyboardInput && SixDofControllerManager.Instance)
			{
				_mouseKeyboardInput = SixDofControllerManager.Instance.GetBestActiveDevice() as MouseKeyboardInput;
			}

			if (_mouseKeyboardInput == null || _mouseKeyboardInput.HasFocus)
			{
				float moveSpeed = _moveSpeed;
				if (Input.GetKey(SPRINT))
				{
					moveSpeed *= 2;
				}

				// Rotation
				float h = Input.GetAxis("Mouse X");
				float v = -Input.GetAxis("Mouse Y");
				
				if (_fixedInitialInputError < 10)
				{
					// skip the first frame of input to avoid a bug that causes things to jump
					if (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0)
					{
						_fixedInitialInputError++;
					}
				}
				else
				{
					_cameraTransform.Rotate(v, h, 0);
				}

				if (Input.GetKey(ROLL_LEFT))
				{
					_roll += _rollSpeed * Time.deltaTime;
				}
				if (Input.GetKey(ROLL_RIGHT))
				{
					_roll -= _rollSpeed * Time.deltaTime;
				}
				if (Input.GetKey(ROLL_RESET))
				{
					_roll = 0;
				}

				// Overwrite roll accumulated from rotation with user-defined roll
				Vector3 euler = _cameraTransform.rotation.eulerAngles;
				euler.z = _roll;
				_cameraTransform.rotation = Quaternion.Euler(euler);

				// Translation
				Vector3 forward = _cameraTransform.forward;
				if (_isWalkingCamera)
				{
					forward.y = 0;
					forward = forward.normalized;
				}
				forward *= Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

				Vector3 right = _cameraTransform.right;
				if (_isWalkingCamera)
				{
					right.y = 0;
					right = right.normalized;
				}
				right *= Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;

				Vector3 m = forward + right;

				if (Input.GetKey(UP))
				{
					m.y += moveSpeed * Time.deltaTime;
				}
				if (Input.GetKey(DOWN))
				{
					m.y -= moveSpeed * Time.deltaTime;
				}

				//_cameraTransform.Translate(m);
				_cameraTransform.position += m;

				// Set the position and rotation of this object so it can be used like a 6dof controller
				transform.position = _cameraTransform.position;
				transform.rotation = _cameraTransform.rotation;

				// Keep cursor locked
				if (UnityEngine.Cursor.lockState != CursorLockMode.Locked)
				{
					UnityEngine.Cursor.lockState = CursorLockMode.Locked;
				}
			}
		}
	}
}
