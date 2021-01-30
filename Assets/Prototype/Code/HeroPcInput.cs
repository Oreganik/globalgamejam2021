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
	public class HeroPcInput : MonoBehaviour 
	{
		private bool _hasFocus;
		private HeroMotion _heroMotion;
		private Transform _cameraTransform;

		private void SetFocus (bool value)
		{
			_hasFocus = value;
			if (_hasFocus)
			{
				UnityEngine.Cursor.lockState = CursorLockMode.Confined;
				UnityEngine.Cursor.visible = false;
			}
			else
			{
				UnityEngine.Cursor.lockState = CursorLockMode.None;
				UnityEngine.Cursor.visible = true;
			}
		}

		/// <summary>Rotates camera in editor.</summary>
		private void RotateCamera ()
		{
			// Rotation
			float h = Input.GetAxis("Mouse X");
			float v = -Input.GetAxis("Mouse Y");
			
			_cameraTransform.Rotate(v, h, 0);

			// Overwrite roll accumulated from rotation with user-defined roll
			Vector3 euler = _cameraTransform.rotation.eulerAngles;
			euler.z = 0;
			_cameraTransform.rotation = Quaternion.Euler(euler);
		}

		protected void Update ()
		{
			if (_hasFocus)
			{
				if (Input.GetMouseButtonDown(0))
				{
					SetFocus(true);
				}
			}
			else
			{
				if (Input.GetKeyDown(KeyCode.Escape))
				{
					SetFocus(false);
				}				
			}

			RotateCamera();

			if (Input.GetMouseButton(1))
			{
				_heroMotion.Fly(_cameraTransform.forward);
			}
			else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.Q))
			{
				_heroMotion.Land();
			}
			else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.E))
			{
				_heroMotion.Lift();
			}
			else
			{
				_heroMotion.Hover();
			}
		}

		protected void Awake ()
		{
			_cameraTransform = Camera.main.transform;
			_heroMotion = GetComponent<HeroMotion>();
			SetFocus(true);
		}
	}
}
