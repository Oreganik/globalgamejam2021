using XrPrototypeKit.Menus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Example
{
	public class ExampleDialogButton : MenuButton
	{
		protected override void HandleClick()
		{
			int rand = Random.Range(0, 256);
			DialogMenuController.OpenTwoOptions("DIALOG TEST", "Print " + rand.ToString() + " to the log?", 
			"OK", () => { 
				Debug.Log("Dialog Menu: " + rand.ToString()); 
				_menuController.MenuManager.Open(MenuName.PREVIOUS);
			},
			"Cancel", () => { _menuController.MenuManager.Open(MenuName.PREVIOUS); }
			);
		}
	}
}
