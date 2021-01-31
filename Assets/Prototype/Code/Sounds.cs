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
	public class Sounds : MonoBehaviour 
	{
		public enum Type { Invalid, Music_Intro, Item_Pickup, Get_Quest, Button_Click }

		public static void Play (Type type)
		{
			switch (type)
			{
				case Type.Button_Click:
					SimpleSounds.Play("sfx_button_click");
					break;

				case Type.Get_Quest:
					SimpleSounds.Play("sfx_get_quest", 0.5f);
					break;

				case Type.Item_Pickup:
					SimpleSounds.Play("sfx_pickup", 0.3f);
					break;

				case Type.Music_Intro:
					SimpleSounds.Play("music_intro", 0.5f);
					break;
			}
		}
	}
}
