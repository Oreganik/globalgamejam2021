using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// Manages the various cursors that can be used for each mode.
	/// </summary>
	/// There are multiple ways for users to highlight content they are interested in:
	/// - Raycast from a hand-held 6DOF controller (or two controllers!)
	/// - Raycast from the camera ("head dof")
	/// - Raycast based on eye gaze
	/// - Volume trigger on a 6DOF controller (or anything else, such as a finger or hand)
	///
	/// The system can't be bound to a specific input device. Eye Gaze has no buttons, for example.
	/// So we need a way to let Eye Gaze be the cursor, but another controller handle input.
	/// The system must be written in a way that enables one "tool" per raycast.
	public class CursorManager : MonoBehaviour 
	{
		public void AddCursor (GameObject gameObject, CursorType cursorType)
		{
			
		}
	}
}
