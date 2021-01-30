using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace XrPrototypeKit.Menus
{
	public class ListItemOption : MenuButton
	{
		public string Value
		{
			get { return _value; }
		}

		public Vector3 Offset
		{
			get { return _offset; }
		}

		public Vector3 _offset;

		protected int _index;
		protected string _value;

		public void AdjustPosition (Vector3 offset)
		{
			if (GetComponent<RectTransform>())
			{
				Vector2 offset2d = new Vector2(offset.x, offset.y);
				GetComponent<RectTransform>().anchoredPosition += offset2d;
			}
			else
			{
				transform.localPosition += offset;
			}
		}

		public void Initialize (string value, int index)
		{
			_value = value;
			_index = index;
			AdjustPosition(_offset * index);
		}
	}
}
