using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace XrPrototypeKit.Menus
{
	// The prefab should have two buttons. The primary button will be centered for a single input dialog.
	public class DialogMenuController : MenuController
	{
		private static DialogMenuController s_instance;

		#pragma warning disable 0649
		[SerializeField] private TMP_Text _title;
		[SerializeField] private TMP_Text _text;
		#pragma warning restore 0649

		private static Action[] s_actions;
		private static Vector3[] s_baseButtonPositions;

		public static void Close (bool immediately = true)
		{
			s_instance.MenuManager.Close(s_instance.MenuId, immediately);
		}

		public static void OpenNoOptions (string title, string text)
		{
			s_instance.ShowDialog(title, text, new string[0], new Action[0]);
		}

		public static void OpenOneOption (string title, string text, string label0, Action action0)
		{
			s_instance.ShowDialog(title, text, new string[] { label0 }, new Action[] { action0 });
		}

		public static void OpenTwoOptions (string title, string text, string label0, Action action0, string label1, Action action1)
		{
			s_instance.ShowDialog(title, text, new string[] { label0, label1 }, new Action[] { action0, action1 });
		}

		public void ClickButton (int id)
		{
			// Close the dialog first in case an option opens another dialog!
			Close();

			if (s_actions[id] != null)
			{
				s_actions[id]();
			}
			else
			{
				Debug.LogError("Option " + id + " is null!");
			}
		}

		public static void ResetPosition ()
		{
			ObjectTracker tracker = s_instance.gameObject.GetComponentInParent<ObjectTracker>();
			if (tracker)
			{
				tracker.SnapToLocation();
			}
		}

		private void ShowDialog (string title, string text, string[] labels, Action[] actions)
		{
			_title.text = title;
			_text.text = text;

			s_actions = new Action[actions.Length];
			Array.Copy(actions, s_actions, actions.Length);

			for (int i = 0; i < _menuIcons.Length; i++)
			{
				_menuIcons[i].gameObject.SetActive(labels.Length > i);

				if (i < labels.Length)
				{
					_menuIcons[i].SetLabel(labels[i]);
					_menuIcons[i].SetClickAction(actions[i]);
				}
			}

			// TODO: Smarter support for 3 or more buttons
			// TODO: Handle vertical alignment

			// Center button 0 if it's the only option
			if (labels.Length == 1)
			{
				_menuIcons[0].rectTransform.anchoredPosition = (s_baseButtonPositions[0] + s_baseButtonPositions[1]) / 2;
			}
			// Otherwise, if two buttons are visible, reset their positions
			else if (labels.Length == 2)
			{
				for (int i = 0; i < _menuIcons.Length; i++)
				{
					_menuIcons[i].rectTransform.anchoredPosition = s_baseButtonPositions[i];
				}
			}

			s_instance.MenuManager.Open(this.MenuId);
		}

		protected override void AfterAwake ()
		{
			if (s_instance)
			{
				Debug.LogError("Destroying duplicate DialogMenuController on " + gameObject.name);
				Destroy(this);
				return;
			}

			s_instance = this;

			s_baseButtonPositions = new Vector3[_menuIcons.Length];

			for (int i = 0; i < _menuIcons.Length; i++)
			{
				s_baseButtonPositions[i] = _menuIcons[i].rectTransform.anchoredPosition;
			}
		}
	}
}
