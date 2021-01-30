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
	public class ContentTools : MonoBehaviour 
	{
		public static ContentTools Instance;

		public BaseTool ActiveTool
		{
			get { return _activeTool; }
		}
		
		[SerializeField] private BaseTool _defaultTool;

		private Dictionary<Enum, BaseTool> _toolsMap;
		private BaseTool _activeTool;
		private BaseTool[] _tools;
		private Cursor _cursor;

		public BaseTool Activate (BaseTool newTool)
		{
			if (_activeTool)
			{
				_activeTool.Deactivate();
			}

			_activeTool = newTool;			
			_activeTool.Activate(_cursor);

			// yo. ted here. just want to say... yeah... that's a really big line of references. probably too big.
			// also, shouldn't this be on the tool itself?
			if (SixDofControllerManager.Instance.ActiveDevice.RaycastCursor.CursorTarget != null)
			{
				_activeTool.ForceHover(SixDofControllerManager.Instance.ActiveDevice.RaycastCursor.CursorTarget.gameObject);
			}
			
			return _activeTool;
		}

		public BaseTool Activate (Enum toolType)
		{
			if (_cursor == null)
			{
				Debug.LogWarning("XrPrototypeKit.Cursor has not been set");
				return null;
			}

			BaseTool newTool = null;

			if (_toolsMap.TryGetValue(toolType, out newTool) == false)
			{
				Debug.LogErrorFormat("ContentTools: ToolType.{0} not found", toolType.ToString()); 
				return null;
			}

			return Activate(newTool);
		}

		public BaseTool ActivateDefaultTool ()
		{
			if (_cursor == null) return null;
			return Activate(_defaultTool);
		}

		public void SetCursor (Cursor cursor)
		{
			_cursor = cursor;
			transform.parent = cursor.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			ActivateDefaultTool();
		}

		protected void Awake ()
		{
			Instance = this;

			if (_defaultTool == null)
			{
				Debug.LogError("ContentTools must have a default tool assigned in the inspector.", gameObject);
				Debug.Log("(Using the first tool we find, but seriously, please fix this)");
				Debug.Break();
			}

			// Find all child objects with BaseTool components and add them to a dictionary
			_toolsMap = new Dictionary<Enum, BaseTool>();
			_tools = GetComponentsInChildren<BaseTool>();
			foreach (BaseTool tool in _tools)
			{
				// fix the missing default tool if necessary
				if (_defaultTool == null)
				{
					_defaultTool = tool;
					Debug.LogError("ContentTools has no default tool assigned. Using " + _defaultTool.Type + " to keep things working.");
				}

				if (_toolsMap.ContainsKey(tool.Type) == false)
				{
					_toolsMap.Add(tool.Type, tool);
				}
				else
				{
					Debug.LogErrorFormat("ContentTools: Multiple tools of ToolType.{0}", tool.Type.ToString());
					Debug.Break();
					return;
				}
			}

			// Disable all tools on start
			foreach (BaseTool tool in _tools)
			{
				tool.Initialize(this);
			}
		}
	}
}
