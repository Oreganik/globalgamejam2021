// GLOBAL GAME JAM 2021
// Shaquan Ladson & Ted Brown

using Jambox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prototype
{
	/// <summary>
	/// 
	/// </summary>
	public class DebugUI : MonoBehaviour 
	{
		public static DebugUI Instance;

		public Text _leftHandText;
		public Text _rightHandText;

		private bool _isVisible;
		private Transform _cameraTransform;

		public void Clear ()
		{
			_leftHandText.text = string.Empty;
			_rightHandText.text = string.Empty;
		}

		public void ShowLeftHand (HeroVrHand hand)
		{
			ShowHand(_leftHandText, hand);
		}

		public void ShowRightHand (HeroVrHand hand)
		{
			ShowHand(_rightHandText, hand);
		}

		public void Toggle ()
		{
#if DEBUG
			_isVisible = !_isVisible;
			gameObject.SetActive(_isVisible);
#endif
		}

		private void ShowHand (Text label, HeroVrHand hand)
		{
			label.text = "Dist: " + hand.DistanceFromHead.ToString("F2") + "\n" +
				"HeadZ: " + hand.DistanceHeadZ.ToString("F2") + "\n" +
				"Height: " + hand.HeightAboveHead.ToString("F2") + "\n" +
				"Head Dot: " + hand.DotProductHeadForward.ToString("F2") + "\n" + 
				"Down Dot: " + hand.DotProductDown.ToString("F2");
		}

		protected void Awake ()
		{
			Instance = this;
			_cameraTransform = Camera.main.transform;
			_cameraTransform.gameObject.AddComponent<ClearDebugUI>();
			gameObject.SetActive(false);
		}

		protected void Update ()
		{
			Vector3 fwd = _cameraTransform.forward;
			fwd.y = 0;
			transform.position = _cameraTransform.position + fwd;
			transform.rotation = _cameraTransform.rotation;
		}
	}
}
