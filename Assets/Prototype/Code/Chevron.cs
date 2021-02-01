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
	public class Chevron : MonoBehaviour 
	{
		static float BounceHeight = 1;
		static float SinScaler = 3;
		private Vector3 _basePosition;

		protected void Awake ()
		{
			_basePosition = transform.localPosition;
		}

		protected void Update ()
		{
			float t = (1 + Mathf.Sin(Time.timeSinceLevelLoad* 3)) / 2;
			transform.localPosition = _basePosition + Vector3.up * t * BounceHeight;
		}
	}
}
