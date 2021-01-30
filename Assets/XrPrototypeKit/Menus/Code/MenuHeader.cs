using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Menus
{
	public class MenuHeader : MonoBehaviour
	{
		// Seems lacking, but we may add an animation
		// in here at some point.
		
		public void Appear (float duration)
		{
			gameObject.SetActive(true);
		}

		public void Disappear (float duration)
		{
			gameObject.SetActive(false);
		}

		public void SetLabel (string text)
		{
			TMPro.TMP_Text tmpText = GetComponent<TMPro.TMP_Text>();
			if (tmpText)
			{
				tmpText.text = text;
			}
		}
	}
}
