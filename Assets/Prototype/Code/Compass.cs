// GLOBAL GAME JAM 2021
// Shaquan Ladson & Ted Brown

using Jambox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
	/// <summary>
	/// 
	/// </summary>
	public class Compass : MonoBehaviour 
	{
		public static Compass Instance;

		public Transform _meshTransform;

		private Camera _camera;
		private Transform _targetTransform;

		public void Hide ()
		{
			enabled = false;
			_meshTransform.gameObject.SetActive(false);
		}

		public void SetTarget (Transform transform)
		{
			_targetTransform = transform;
			Show();
		}

		public void Show ()
		{
			enabled = true;
			_meshTransform.gameObject.SetActive(true);
		}

		protected void Awake ()
		{
			Instance = this;
			_camera = Camera.main;
			Hide();
		}

		protected void LateUpdate ()
		{
			if (_targetTransform == null)
			{
				Hide();
				return;
			}

			Vector3 viewportPoint = _camera.WorldToViewportPoint(_targetTransform.position);
			viewportPoint.x = Mathf.Clamp01(viewportPoint.x);
			viewportPoint.y = Mathf.Clamp01(viewportPoint.y);
			viewportPoint.z = 1;
			transform.position = _camera.ViewportToWorldPoint(viewportPoint);

			Vector3 directionToCompass = (transform.position - _camera.transform.position).normalized;
			float dot = Vector3.Dot(directionToCompass, _camera.transform.forward);
			_meshTransform.gameObject.SetActive(dot < 0.95f);

			DebugUI.Instance.ShowCenter(dot.ToString("F2"));
			transform.rotation = _camera.transform.rotation;
			//_meshTransform.localRotation = Quaternion.LookRotation(Vector3.forward, viewportPoint);
		}
	}
}
