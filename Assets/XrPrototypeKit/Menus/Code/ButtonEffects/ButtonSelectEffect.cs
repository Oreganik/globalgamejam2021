using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Menus
{
	/// <summary>
	/// When a child class derives from ButtonSelectEffect and is placed on a MenuButton,
	/// the MenuController hands control to that class on a click event.
	/// This means that "actionOnComplete" must be called by the derived class,
	/// or the menu system will hang.
	/// </summary>
	public abstract class ButtonSelectEffect : MonoBehaviour
	{
		public abstract void Play(Action actionOnComplete);
	}
}
