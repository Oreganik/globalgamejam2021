using System;
using System.Collections;
using System.Collections.Generic;
using XrPrototypeKit.SixDofControllers;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class BaseTool : MonoBehaviour 
	{
		// Note: By using a generic Enum type, developers can use their own enums without conflict.
		// (assuming they don't mix enums!)
		public abstract Enum Type
		{
			get;
		}

		[SerializeField] private Color _hoverColor = Color.white;
		[SerializeField] private Color _unhoverColor = Color.gray;

		protected ContentTools _contentTools;
		protected Cursor _cursor;
		protected SixDofController _controller;

		public void Activate (Cursor cursor)
		{
			gameObject.SetActive(true);
			_cursor = cursor;
			cursor.OnHover += HandleHover;
			cursor.OnUnhover += HandleUnhover;
			cursor.SetColor(_unhoverColor);
			OnAfterActivate();
		}

		public void Deactivate (bool unhover = true)
		{
			if (unhover)
			{
				HandleUnhover(null, null);
			}

			OnBeforeDeactivate();

			if (_cursor)
			{
				_cursor.OnHover -= HandleHover;
				_cursor.OnUnhover -= HandleUnhover;
			}
			gameObject.SetActive(false);
		}

		public void ForceHover (GameObject targetObject)
		{
			HandleHover(targetObject, null);
		}

		// The Magic Leap system can't get the controller launched before Start, so we have to do it lazy style
		public void Initialize (ContentTools contentTools)
		{
			_contentTools = contentTools;
			gameObject.SetActive(false);
		}

		protected void HandleHover (GameObject targetObject, Cursor cursor) 
		{
			_cursor.SetColor(_hoverColor);
			OnAfterHover(targetObject);
		}

		protected void HandleUnhover (GameObject targetObject, Cursor cursor) 
		{
			OnBeforeUnhover(targetObject);
			_cursor.SetColor(_unhoverColor);
		}

		protected virtual void OnAfterActivate () {}
		protected virtual void OnAfterHover (GameObject targetObject) {}
		protected virtual void OnBeforeDeactivate () {}
		protected virtual void OnBeforeUnhover (GameObject targetObject) {}

		protected void OnDestroy ()
		{
			Deactivate();
		}
	}
}
