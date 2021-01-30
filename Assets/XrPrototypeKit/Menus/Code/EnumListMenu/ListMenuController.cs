using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Menus
{
	// Same as a menu controller, but it generates its buttons based on a specified enum.
	public class ListMenuController : MenuController
	{
		[Tooltip("Use the full typename, including namespace")]
		public string _enumType;
		public GameObject _iconPrefab;
		[Tooltip("Any enum value with an index up to and including this value will be excluded")]
		public int _startingIndex = -1;
		public RectTransform _canvasTransform;
		public RectTransform _header;

		public static Type GetEnumType(string enumName)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var type = assembly.GetType(enumName);
				if (type == null)
					continue;
				if (type.IsEnum)
					return type;
			}
			return null;
		}

		protected override bool InitializeIcons ()
		{
			Type t = GetEnumType(_enumType);
			if (t == null)
			{
				Debug.LogErrorFormat("[ {0} ] MenuControllerList: [ {1} ] can't be parsed as an enum type", gameObject.name, _enumType);
				return false;
			}

//			Debug.Log("Parsed enumType as " + t.ToString());

			string[] names = Enum.GetNames(t);
			int start = _startingIndex >= 0 ? _startingIndex : 0;
			int count = 0;
			Vector3 itemOffset = Vector3.zero;
			for (int i = start; i < names.Length; i++)
			{
				ListItemOption icon = Instantiate(_iconPrefab, _canvasTransform == null ? transform : _canvasTransform).GetComponent<ListItemOption>();
				icon.SetLabel(names[i], insertSpaces: true);
				icon.Initialize(names[i], count);
				itemOffset = icon.Offset;
				Debug.LogFormat("[ {0} ] MenuControllerList: Added option for [ {1} ]", gameObject.name, name);
				count++;
			}

			_menuIcons = GetComponentsInChildren<MenuButton>();

			// shift all objects by half the total height so it's centered vertically.
			// we're making a big assumption here, which is that there is a header on the top.
			// And navigation buttons? logos? who knows?
			// Definitely an area for future improvement.
			Vector3 finalOffset = ((count - 1) / 2) * itemOffset * -1;

			if (_header)
			{
				Vector3 headerOffset = finalOffset - itemOffset * 0.5f;
				if (_header.GetComponent<RectTransform>())
				{
					Vector2 headerOffset2d = new Vector2(headerOffset.x, headerOffset.y);
					_header.GetComponent<RectTransform>().anchoredPosition += headerOffset2d;
				}
				else
				{
					_header.transform.localPosition += headerOffset;
				}
			}

			foreach (ListItemOption itemOption in GetComponentsInChildren<ListItemOption>())
			{
				itemOption.AdjustPosition(finalOffset);
			}

			return true;
		}
	}
}
