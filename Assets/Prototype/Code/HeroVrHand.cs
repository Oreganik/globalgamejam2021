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
	public class HeroVrHand
	{
		public float DistanceFromHead;
		public float DotProductHeadForward;
		public float DotProductDown;
		public float DistanceHeadZ;
		public float HeightAboveHead;

		private Transform _handTransform;
		private Transform _headTransform;

		public HeroVrHand (Transform handTransform, Transform headTransform)
		{
			_handTransform = handTransform;
			_headTransform = headTransform;
		}

		public void Process ()
		{
			Vector3 headSpace = _headTransform.InverseTransformPoint(_handTransform.position);
			DistanceHeadZ = headSpace.z;
			HeightAboveHead = _handTransform.position.y - _headTransform.position.y;
			DistanceFromHead = Vector3.Distance(_handTransform.position, _headTransform.position);
			Vector3 directionFromHead = (_handTransform.position - _headTransform.position).normalized;
			DotProductHeadForward = Vector3.Dot(directionFromHead, _headTransform.forward);
			DotProductDown = Vector3.Dot(directionFromHead, Vector3.down);
		}
	}
}
