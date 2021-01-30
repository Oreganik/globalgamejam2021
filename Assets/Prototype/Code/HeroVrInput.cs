// GLOBAL GAME JAM 2021
// Shaquan Ladson & Ted Brown

using Jambox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
	/// <summary>
	/// 
	/// </summary>
	public class HeroVrInput : MonoBehaviour 
	{
		private const float DOT_FLY = 0.5f;
		private const float DOT_LAND = 0.5f;

		public Transform _leftHand;
		public Transform _head;
		public Transform _rightHand;

		private HeroMotion _heroMotion;

		protected void Update ()
		{
			// One hand above head: Lift
			if (_leftHand.position.y > _head.position.y || _rightHand.position.y > _head.position.y)
			{
				_heroMotion.Lift();
				return;
			}

			// Both hands forward: Fly
			// Get a dot product of head forward and head to hand.
			// If the value is greater than 0.5, should be pretty close to "forward."
			Vector3 dirToLeft = (_leftHand.position - _head.position).normalized;
			Vector3 dirToRight = (_rightHand.position - _head.position).normalized;
			Vector3 forward = _head.forward;

			if (Vector3.Dot(dirToLeft, forward) > DOT_FLY && Vector3.Dot(dirToRight, forward) > DOT_FLY)
			{
				_heroMotion.Fly(forward);
				return;
			}

			// Hands held sideways and away from body: Land
			// Get a dot product of world down and head to hand.
			// Greater than 0.8 or 0.9 is probably right at the user's side,
			// so check for less than 0.5 (45 degree angle or higher).
			if (Vector3.Dot(dirToLeft, Vector3.down) < DOT_LAND && Vector3.Dot(dirToRight, Vector3.down) < DOT_LAND)
			{
				_heroMotion.Land();
				return;
			}

			// If we can't determine a specific input, hover in place.
			_heroMotion.Hover();
		}

		protected void Awake ()
		{
			_heroMotion = GetComponent<HeroMotion>();
		}
	}
}
