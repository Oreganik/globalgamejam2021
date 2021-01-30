using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// 
	/// </summary>
	public class PMath : MonoBehaviour 
	{
		/// <summary>
		/// https://en.wikipedia.org/wiki/Smoothstep
		/// </summary>
		public static float SmoothStep(float linear)
		{
			return 3 * linear * linear - 2 * linear * linear * linear;
		}
	}
}
