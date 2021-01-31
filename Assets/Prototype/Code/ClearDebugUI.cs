// GLOBAL GAME JAM 2021
// Shaquan Ladson & Ted Brown

using Jambox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
	/// <summary>
	/// Takes advantage of OnPostRender to clear DebugUI text that built up that frame.
	/// Automatically added by DebugUI
	/// </summary>
	public class ClearDebugUI : MonoBehaviour 
	{
		protected void OnPostRender ()
		{
			DebugUI.Instance.Clear();
		}
	}
}
