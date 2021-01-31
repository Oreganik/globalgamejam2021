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
	public class AutoRotate : MonoBehaviour 
	{
		public float _speed = 720;

		private Vector3 _rotation;

		protected void Awake ()
		{
			_rotation = Random.insideUnitSphere;
		}

		protected void Update ()
		{
			transform.Rotate(_rotation * _speed * Time.deltaTime);
		}
	}
}
