using System;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// Scans for a CursorTarget component and broadcasts its GameObject to subscribers on Hover/Unhover events.
	/// </summary>
	public class RaycastCursor : Cursor
	{
		public enum State { Scan, Hold }
		const float MAX_LENGTH = 5;
		const int LINE_SEGMENTS = 20;

		public Vector3 Hitpoint
		{
			get { return _hitpoint; }
		}

		public Vector3 Direction
		{
			get { return _direction; }
		}

		public Vector3 Origin 
		{
			get { return _origin; }
		}

		private bool _useDistanceOverride;
		private float _distanceOverride;
		private LineRenderer _line;
		private MaterialPropertyBlock _propertyBlock;
		private State _state;
		private Vector3 _hitpoint;
		private Vector3 _direction;
		private Vector3 _origin;
		
		public void ClearMaxDistanceOverride ()
		{
			_useDistanceOverride = false;
		}

		/// <summary>
		/// Forces the raycast to a specified max distance until cleared
		/// </summary>
		public void OverrideMaxDistance (float distance)
		{
			_useDistanceOverride = true;
			_distanceOverride = distance;
		}

		public void Hide()
		{
			if (_line)
			{
				_line.enabled = false;
			}
		}

		public void Hold (CursorTarget raycastTarget)
		{
			_state = State.Hold;
			_cursorTarget = raycastTarget;
		}

		public void Reset ()
		{
			_hitMask = _defaultLayerMask;
			_useDistanceOverride = false;
		}

		public void ResetLayerMask ()
		{
			_hitMask = _defaultLayerMask;
		}

		public void Release ()
		{
			_state = State.Scan;
			_cursorTarget = null;
		}

		public override void SetColor (Color color)
		{
			_line.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetColor("_Color", color);
			_line.SetPropertyBlock(_propertyBlock);
		}

		public void SetHitTriggers (bool hitTriggers)
		{
			if (hitTriggers)
			{
				_queryTriggerInteraction = QueryTriggerInteraction.Collide;
			}
			else
			{
				_queryTriggerInteraction = QueryTriggerInteraction.Ignore;
			}
		}

		public void SetTargetLayer (int layer)
		{
			_hitMask = 1 << layer;
		}

		public void Show()
		{
			if (_line)
			{
				_line.enabled = true;
			}
		}

		private void Hover (CursorTarget target)
		{
			Unhover();
			// Set the active target as current target and hover it
			_cursorTarget = target;
			_cursorTarget.Hover();
			if (OnHover != null)
			{
				OnHover(target.gameObject, this);
			}
		}

		private void Unhover ()
		{
			// Ignore if we have no target
			if (_cursorTarget == null) return;
			_cursorTarget.Unhover();
			if (OnUnhover != null)
			{
				OnUnhover(_cursorTarget.gameObject, this);
			}
			_cursorTarget = null;
		}

		protected void Awake()
		{
			_line = GetComponent<LineRenderer>();
			if (!_line)
			{
				Debug.LogWarning("InputRaycast: no LineRenderer attached");
				enabled = false;
			}

			// Separate line into 10 segments keeping the start z and the end z the same value.
			var startZ = _line.GetPosition(0).z;
			var endZ = _line.GetPosition(_line.positionCount - 1).z;

			_line.positionCount = LINE_SEGMENTS;
			for (int i = 0; i < LINE_SEGMENTS; ++i)
			{
				_line.SetPosition(i, Vector3.forward * ((i / (float)LINE_SEGMENTS) * (endZ - startZ) + startZ));
			}

			_propertyBlock = new MaterialPropertyBlock();
			_queryTriggerInteraction = QueryTriggerInteraction.Ignore;
			_defaultLayerMask = _hitMask;
		}

		public GameObject Scan ()
		{
			return Scan(transform.position, transform.forward);
		}

		public GameObject Scan (Vector3 origin, Vector3 direction)
		{
			return Scan(origin, direction, _useDistanceOverride ? _distanceOverride : MAX_LENGTH, _hitMask);
		}

		public GameObject Scan (Vector3 origin, Vector3 direction, float distance)
		{
			return Scan(origin, direction, distance, _hitMask);
		}

		public GameObject Scan (Vector3 origin, Vector3 direction, float distance, LayerMask layerMask)
		{
			_origin = origin;
			_direction = direction;

			GameObject targetObject = null;

			_hitpoint = origin + direction * distance;

			if (_state == State.Hold && _cursorTarget)
			{
				_hitpoint = _cursorTarget.transform.position;
			}
			else
			{
				RaycastHit hit;

				if (Physics.Raycast(origin, direction, out hit, distance, layerMask, _queryTriggerInteraction))
				{
					_hitpoint = hit.point;

					CursorTarget currentTarget = hit.collider.GetComponentInParent<CursorTarget>();
					
					if (currentTarget) // we're hitting something
					{
						if (currentTarget != _cursorTarget && currentTarget.CanBeHovered) // it's different than it was last time
						{
							Hover(currentTarget);
							targetObject = hit.collider.gameObject;
						}
						// otherwise, current target is same as active target. don't do anything.
					}
					else // we're hitting nothing
					{
						Unhover();
					}
				}
				else
				{
					Unhover();
				}

			}

			float segmentLength = Vector3.Distance(origin, _hitpoint) / LINE_SEGMENTS;
			for (int i = 0; i < LINE_SEGMENTS; ++i)
			{
				_line.SetPosition(i, origin + direction * i * segmentLength);
			}

			return targetObject;
		}

		// protected void Update()
		// {
		// 	Vector3 startPoint = transform.position;
		// 	Vector3 forward = transform.forward;
		// 	float raycastDistance = _useDistanceOverride ? _distanceOverride : MAX_LENGTH;
		// 	_hitpoint = startPoint + forward * raycastDistance;

		// 	if (_state == State.Hold)
		// 	{
		// 		_hitpoint = _heldTransform.position;
		// 	}
		// 	else
		// 	{
		// 		RaycastHit hit;

		// 		if (Physics.Raycast(startPoint, forward, out hit, raycastDistance, _hitMask, _queryTriggerInteraction))
		// 		{
		// 			_hitpoint = hit.point;

		// 			RaycastTarget currentTarget = hit.collider.GetComponentInParent<RaycastTarget>();
					
		// 			if (currentTarget) // we're hitting something
		// 			{
		// 				if (currentTarget != _target) // it's different than it was last time
		// 				{
		// 					Hover(currentTarget);
		// 				}
		// 				// otherwise, current target is same as active target. don't do anything.
		// 			}
		// 			else // we're hitting nothing
		// 			{
		// 				Unhover();
		// 			}
		// 		}
		// 		else
		// 		{
		// 			Unhover();
		// 		}

		// 	}

		// 	float segmentLength = Vector3.Distance(startPoint, _hitpoint) / LINE_SEGMENTS;
		// 	for (int i = 0; i < LINE_SEGMENTS; ++i)
		// 	{
		// 		_line.SetPosition(i, startPoint + forward * i * segmentLength);
		// 	}
		// }
	}
}
