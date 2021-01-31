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
	public class Location : MonoBehaviour 
	{
		public string Id;

		protected void Awake ()
		{
			LocationDatabase.Register(this);
			gameObject.name = "[Location] " + Id;
		}
	}
}
