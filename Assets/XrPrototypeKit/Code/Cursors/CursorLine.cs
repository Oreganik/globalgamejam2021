using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// A line renderer that uses information from Cursors and Tools
	/// </summary>
	public class CursorLine : MonoBehaviour 
	{
		const int LINE_SEGMENTS = 20;

		private Cursor _cursor;
		private LineRenderer _line;


		protected void LateUpdate ()
		{
			// float segmentLength = Vector3.Distance(origin, _hitpoint) / LINE_SEGMENTS;
			// for (int i = 0; i < LINE_SEGMENTS; ++i)
			// {
			// 	_line.SetPosition(i, origin + direction * i * segmentLength);
			// }
		}
	}
}
