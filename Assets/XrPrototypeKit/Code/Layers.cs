using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// 
	/// </summary>
	public class Layers
	{
		public static LayerMask CreateMask (params string[] layerNames)
		{
			LayerMask mask = 0;
			foreach (string name in layerNames)
			{
				int id = LayerMask.NameToLayer(name);
				if (id < 0)
				{
					Debug.LogErrorFormat("Can't add non-existent layer [{0}] to mask", name);
					continue;
				}
				mask = mask | 1 << id;
			}
			return mask;
		}
	}
}
