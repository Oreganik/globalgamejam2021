using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Menus
{
	/// <summary>
	/// 
	/// </summary>
	public class BackButton : MenuButton 
	{
		public bool _immediately;

		protected override void HandleClick ()
		{
			_menuController.MenuManager.Open(MenuName.PREVIOUS, _immediately);
		}
	}
}
