using XrPrototypeKit.Menus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Example
{
	public class MenuKitExample : MonoBehaviour
	{
		public MenuManager _menuManager;

		protected void Start ()
		{
			foreach (MenuButton icon in _menuManager.GetMenu("Rooms").MenuIcons)
			{
				ListItemOption itemOption = icon as ListItemOption;
				if (itemOption)
				{
					itemOption.SetClickAction( () => {
						Debug.Log("Custom click event! " + itemOption.Value);
					});
				}
			}
			_menuManager.Open(MenuName.Home);
		}
	}
}
