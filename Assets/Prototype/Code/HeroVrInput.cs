// GLOBAL GAME JAM 2021
// Shaquan Ladson & Ted Brown

using Jambox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Prototype
{
	/// <summary>
	/// 
	/// </summary>
	public class HeroVrInput : MonoBehaviour 
	{
		public static HeroVrInput Instance;

		public bool GetClick
		{
			get { return GetPrimaryButton() && _primaryButtonDownLastFrame == false; }
		}

		public static float Fly_MinDotProduct = 0.7f;
		public static float Fly_MinDistanceHeadZ = 0.15f; // hands must be greater than this distance (6 inches) to trigger flight
		public static float Land_MaxHeadZ = 0.1f; // so, "behind the head" is greater than zero to feel better
		public static float Land_MaxDotProduct = 0.8f;
		/// <summary>How far above the eyeline (i.e. the camera) the hand must be to trigger Lift</summary>
		public static float Lift_AboveHeadDistance = 0.15f; // 0.1m = 4 inches
		public static float MaxRotateSpeed = 30; // degrees per second

		public Transform RightHandTransform
		{
			get { return _rightTransform; }
		}

		public Transform _leftTransform;
		public Transform _headTransform;
		public Transform _rightTransform;

		private bool _controllersConfigured;
		private bool _primaryButtonDownLastFrame;
		private bool _secondaryButtonDown;
		private List<InputDevice> _controllers;
		private HeroMotion _heroMotion;
		private HeroVrHand _leftHand;
		private HeroVrHand _rightHand;

		public bool GetPrimaryButton ()
		{
			foreach (InputDevice device in _controllers)
			{
				bool pressed = false;
				if (device.TryGetFeatureValue(CommonUsages.primaryButton, out pressed) && pressed)
				{
					return true;
				}
			}
			return false;
		}
		
		private bool IsTryingToFly ()
		{
			if (_leftHand.DistanceHeadZ < Fly_MinDistanceHeadZ) return false;
			if (_rightHand.DistanceHeadZ < Fly_MinDistanceHeadZ) return false;
			if (_leftHand.DotProductHeadForward < Fly_MinDotProduct) return false;
			if (_rightHand.DotProductHeadForward < Fly_MinDotProduct) return false;
			return true;
		}

		private bool IsTryingToLand ()
		{
			if (_leftHand.DistanceHeadZ > Land_MaxHeadZ) return false;
			if (_rightHand.DistanceHeadZ > Land_MaxHeadZ) return false;
			if (_leftHand.DotProductDown > Land_MaxDotProduct) return false;
			if (_rightHand.DotProductDown > Land_MaxDotProduct) return false;
			return true;
		}

		private void ConfigureControllers ()
		{
			if (_controllersConfigured) return;

			_controllers = new List<InputDevice>();
			var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller;
			InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, _controllers);
			
			if (_controllers.Count != 2)
			{
				_controllers.Clear();
				return;
			}
			
			foreach (InputDevice device in _controllers)
			{
				Debug.Log(string.Format("GGJ_DEBUG Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
			}

			_controllersConfigured = true;
		}

		private bool GetSecondaryButton ()
		{
			foreach (InputDevice device in _controllers)
			{
				bool pressed = false;
				if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out pressed) && pressed)
				{
					return true;
				}
			}
			return false;
		}

		protected void Awake ()
		{
			Instance = this;
			_heroMotion = GetComponent<HeroMotion>();
			_leftHand = new HeroVrHand(_leftTransform, _headTransform);
			_rightHand = new HeroVrHand(_rightTransform, _headTransform);
		}

		protected void LateUpdate ()
		{
			_primaryButtonDownLastFrame = GetPrimaryButton();
		}

		protected void Update ()
		{
			// Make sure our controllers are setup.
			// Can't do this on Awake: they haven't been registered yet.
			ConfigureControllers();

			// Get primary axis input and check for rotation
			float rotation = 0;
			foreach (InputDevice device in _controllers)
			{
				Vector2 input = Vector2.zero;
				if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out input))
				{
					Debug.Log("GGJ_DEBUG " + device.name + " " + input.ToString("F2"));
					if (Mathf.Abs(input.x) > Mathf.Abs(rotation))
					{
						rotation = input.x;
					}
				}
			}
			if (Mathf.Abs(rotation) > 0.1f)
			{
				transform.Rotate(Vector3.up * rotation * MaxRotateSpeed * Time.deltaTime, Space.World);
			}

			// Toggle DebugUI on secondary button press in debug mode
#if DEBUG
			if (GetSecondaryButton())
			{
				if (_secondaryButtonDown == false)
				{
					DebugUI.Instance.Toggle();
					_secondaryButtonDown = true;
				}
			}
			else
			{
				_secondaryButtonDown = false;
			}
#endif

			_leftHand.Process();
			_rightHand.Process();
			DebugUI.Instance.ShowLeftHand(_leftHand);
			DebugUI.Instance.ShowRightHand(_rightHand);

			bool boost = GetPrimaryButton();

			// Lift if one hand is above the head
			if (_leftHand.HeightAboveHead > Lift_AboveHeadDistance || _rightHand.HeightAboveHead > Lift_AboveHeadDistance)
			{
				_heroMotion.Lift(boost);
				return;
			}

			// Both hands in front to fly
			if (IsTryingToFly())
			{
				// Check for button press to see if there's a boost
				_heroMotion.Fly(_headTransform.forward, boost);
				return;
			}

			// Hands held sideways and away from body AND behind the head: Land
			// Get a dot product of world down and head to hand.
			// Greater than DOT_LAND is probably at rest by the user's side.
			if (IsTryingToLand())
			{
				_heroMotion.Land(boost);
				return;
			}

			// If we can't determine a specific input, hover in place.
			_heroMotion.Hover();
		}
	}
}
