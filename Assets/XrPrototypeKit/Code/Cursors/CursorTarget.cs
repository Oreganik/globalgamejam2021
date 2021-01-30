using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// 
	/// </summary>
	public class CursorTarget : MonoBehaviour 
	{
		public Action OnHover;
		public Action OnUnhover;

		public bool CanBeHovered = true;

		public void Hover ()
		{
			if (OnHover != null)
			{
				OnHover();
			}
		}
		
		public void Unhover ()
		{
			if (OnUnhover != null)
			{
				OnUnhover();
			}
		}
	}
}
