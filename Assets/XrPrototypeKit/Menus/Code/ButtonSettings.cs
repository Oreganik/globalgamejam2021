using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Menus
{
	[CreateAssetMenu(fileName = "NewButtonSettings", menuName = "XR Prototype Kit/Button Settings", order = 1)]
	public class ButtonSettings : ScriptableObject
	{
		[Header("Hover")]
		public float HoverColorTime = 1;
		public Color UnhoveredColor = Color.gray;
		public Color HoveredColor = Color.white;
	}
}
