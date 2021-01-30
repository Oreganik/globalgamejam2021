using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Menus
{
	/// <summary>
	/// 
	/// </summary>
	public class CloseMenuButton : MenuButton 
	{
		public bool _immediately;

		protected override void HandleClick ()
		{
			_menuController.MenuManager.Close(_immediately);
		}
	}
}
