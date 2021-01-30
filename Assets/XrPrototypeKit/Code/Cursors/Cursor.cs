using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// 
	/// </summary>
	public class Cursor : MonoBehaviour 
	{
		public Action<GameObject, Cursor> OnHover;
		public Action<GameObject, Cursor> OnUnhover;

		public CursorTarget CursorTarget
		{
			get { return _cursorTarget; }
		}



		#pragma warning disable 0649
		[Header("Targets must have CursorTarget component")]
		[SerializeField] protected LayerMask _hitMask;
		#pragma warning restore 0649

		protected CursorTarget _cursorTarget;
		protected LayerMask _defaultLayerMask;
		protected QueryTriggerInteraction _queryTriggerInteraction;

		public virtual void SetColor (Color color)
		{
		}
	}
}
