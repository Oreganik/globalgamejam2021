using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace XrPrototypeKit.UI
{
	public class Notification : MonoBehaviour
	{
		public static Notification Instance;

		public float _distance = 1.5f;

		private TMP_Text _label;
		private Transform _cameraTransform;

		public void Hide ()
		{
			_label.text = string.Empty;
		}

		public void Show (string text, float duration = 3)
		{
			CancelInvoke("Hide");
			_label.text = text;
			Invoke("Hide", duration);
		}

		protected void Awake ()
		{
			Instance = this;
			_label = gameObject.GetComponent<TMP_Text>();
			_label.text = string.Empty;
			_cameraTransform = Camera.main.transform;
		}

		protected void LateUpdate ()
		{
			transform.position = _cameraTransform.position + _cameraTransform.forward * _distance;
			transform.rotation = Quaternion.LookRotation((transform.position - _cameraTransform.position).normalized, _cameraTransform.up);
		}
	}
}
