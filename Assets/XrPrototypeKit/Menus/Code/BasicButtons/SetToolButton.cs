using XrPrototypeKit.SixDofControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Menus
{
	/// <summary>
	/// 
	/// </summary>
	public class SetToolButton : MenuButton 
	{
		public ToolType _toolType;

		protected override void HandleClick ()
		{
			SixDofController controller = SixDofControllerManager.Instance.GetBestActiveDevice();
			if (controller == null)
			{
				Debug.LogWarning("SetToolButton: No SixDofController found");
				return;
			}

			ContentTools contentTools = controller.GetComponentInChildren<ContentTools>();
			if (contentTools == null)
			{
				Debug.LogErrorFormat("SetToolButton: No ContentTools found on SixDofController");
				return;
			}

			contentTools.Activate(_toolType);
			_menuController.MenuManager.Close();
		}
	}
}
