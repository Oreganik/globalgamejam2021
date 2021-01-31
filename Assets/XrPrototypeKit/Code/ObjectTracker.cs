using XrPrototypeKit.Menus; // only here to handle menu manager
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit
{
	public class ObjectTracker : MonoBehaviour
	{
		public bool _updateContinuously = true;
		public bool _followCamera = true;
		public Transform _altTargetTransform;
		public bool _useWorldUp = true;
		public Vector3 _offset = Vector3.forward;
		public float _lerp = 0.01f;
		public bool _fixedHeight = true;

		private MenuManager _menuManager; // this needs to be refactored
		private Transform _targetTransform;

		public void MoveToLocation (float lerp)
		{
			Vector3 direction = _targetTransform.rotation * _offset;
			Vector3 targetPosition = _targetTransform.position + direction;
			if (_fixedHeight) targetPosition.y = _targetTransform.position.y;
			transform.position = Vector3.Lerp(transform.position, targetPosition, lerp);
			
			if (_useWorldUp)
			{
				Quaternion targetRotation = Quaternion.LookRotation(_targetTransform.forward, Vector3.up);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lerp);
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, _targetTransform.rotation, lerp);
			}
		}

		public void SnapToLocation ()
		{
			MoveToLocation(1);
		}

		private void HandleOpenMenuManager (MenuManager menuManager)
		{
			if (menuManager == _menuManager)
			{
				MoveToLocation(1);
			}
		}

		protected void Awake ()
		{
			_menuManager = GetComponent<MenuManager>();
			if (_menuManager)
			{
				MenuManager.OnOpenMenuManager += HandleOpenMenuManager;
			}
		}

		protected void LateUpdate ()
		{
			if (_updateContinuously)
			{
				MoveToLocation(_lerp);
			}
		}

		protected void OnEnable ()
		{
			if (_targetTransform == null)
			{
				if (_followCamera)
				{
					_targetTransform = Camera.main.transform;
				}
				else
				{
					_targetTransform = _altTargetTransform;
				}
			}
			
			SnapToLocation();
		}
	}
}
