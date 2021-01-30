using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.SixDofControllers
{
	public class SwipeInput : MonoBehaviour
	{
		public delegate void TouchpadDownEvent();
		public TouchpadDownEvent OnTouchpadDown;

		public delegate void TouchpadScrollEvent(Vector2 scrollDelta);
		public TouchpadScrollEvent OnTouchpadScroll;

		public delegate void TouchpadUpEvent();
		public TouchpadUpEvent OnTouchpadUp;

		public delegate void SwipeStartedEvent(Vector2 swipeDirection);
		/// <summary>
		/// Event raised after a scroll has passed a deadzone, or when it changes direction.
		/// </summary>
		public SwipeStartedEvent OnSwipeStarted;

		#pragma warning disable 0649 // Disable unassigned warning (fields assigned from Unity Editor)

		/// <summary>
		/// Allows consumers to temporarily disable new input data
		/// maintains inputs coming from the update loop, and scroll delta falloff to continue
		/// </summary>

		[SerializeField] private Vector2 _scrollSpeedMulitplier;
		[SerializeField] private float _scrollDampTime;
		[SerializeField] private bool _singleAxisPerSwipe;
		[SerializeField] protected float _axisDeadZone;
		[SerializeField] private float _onMoveIntegrationAmount = 0.7f;
		[SerializeField] protected float _mouseScrollSpeedMultiplier = 0.1f;
		[SerializeField] protected float _mouseScrollVelocityMultiplier = 0.1f;
		[SerializeField] private float _minimumRequiredMoveDistance;

		#pragma warning restore 0649

		private Vector2 _currentScrollVelocity;
		private bool _isTouchpadDown;
		private bool _inputsEnabled = true;
		protected Vector2 _lastTouchPosition;
		protected Vector2 _startPosition;
		private Vector2 _singleAxisMultiplier;
		private Vector2 _touchDelta;
		private bool _sentSwipeStartedEvent;

		public bool IsTouchpadDown
		{
			get
			{
				return _isTouchpadDown;
			}
		}

		public Vector2 ScrollVelocity
		{
			get
			{
				return _currentScrollVelocity;
			}
		}

		public float MaxScrollVelocityXY
		{
			get
			{
				// Get the the largest of the swipe axes

				if (Mathf.Abs(ScrollVelocity.x) > Mathf.Abs(ScrollVelocity.y))
				{
					return ScrollVelocity.x;
				}
				else
				{
					return ScrollVelocity.y;
				}
			}
		}

		protected void Start()
		{
			_minimumRequiredMoveDistance *= _minimumRequiredMoveDistance;
		}
		private void OnEnable()
		{
			SixDofControllerManager.Instance.OnTouchpadPressed += TouchDown;
			SixDofControllerManager.Instance.OnTouchpadReleased += TouchPadReleased;
			SixDofControllerManager.Instance.OnTouchpadMoved += TouchMoved;
		}
		private void OnDisable()
		{
			if (SixDofControllerManager.Instance)
			{
				SixDofControllerManager.Instance.OnTouchpadPressed -= TouchDown;
				SixDofControllerManager.Instance.OnTouchpadReleased -= TouchPadReleased;
				SixDofControllerManager.Instance.OnTouchpadMoved -= TouchMoved;
			}
		}

		protected virtual void ScrolledKeyboard(Vector2 scroll)
		{
			if (OnTouchpadScroll != null && scroll != Vector2.zero)
			{
				scroll = Vector2.Scale(scroll, _scrollSpeedMulitplier) * _mouseScrollSpeedMultiplier;
				OnTouchpadScroll(scroll);
				_currentScrollVelocity = scroll * _mouseScrollVelocityMultiplier;
			}
		}

		protected virtual void TouchDown(TouchPadData data)
		{
			_currentScrollVelocity = Vector2.zero;
			_isTouchpadDown = true;
			_lastTouchPosition = data.position;
			// the DialInput subclass uses this data to decide if which input mode it is in
			_startPosition = data.position;
			if (_singleAxisPerSwipe)
			{
				_singleAxisMultiplier = Vector2.zero;
			}

			if (OnTouchpadDown != null)
			{
				OnTouchpadDown();
			}
		}

		protected virtual void TouchMoved(TouchPadData data)
		{
			if (!_inputsEnabled)
			{
				_lastTouchPosition = data.position;
				return;
			}

			if (data.inputDeviceType == ControllerType.MouseAndKeyboard)
			{
				ScrolledKeyboard(data.position);
				return;
			}

			if (_isTouchpadDown)
			{
				_touchDelta = data.position - _lastTouchPosition;
				if (_singleAxisPerSwipe && _singleAxisMultiplier == Vector2.zero)
				{
					// check to see if we have gone over the threshold for deadzone axis input
					if (Mathf.Abs(_startPosition.y - data.position.y) > _axisDeadZone)
					{
						_singleAxisMultiplier = new Vector2(0, 1);
					}
					else if (Mathf.Abs(_startPosition.x - data.position.x) > _axisDeadZone)
					{
						_singleAxisMultiplier = new Vector2(1, 0);
					}
					else
					{
						return;
					}
				}
				_touchDelta = new Vector2(_touchDelta.x * _scrollSpeedMulitplier.x, _touchDelta.y * _scrollSpeedMulitplier.y);
				if (_singleAxisPerSwipe)
				{
					_touchDelta = new Vector2(_singleAxisMultiplier.x * _touchDelta.x, _singleAxisMultiplier.y * _touchDelta.y);
				}
				if(!_sentSwipeStartedEvent)
				{
					if(OnSwipeStarted != null)
					{
						OnSwipeStarted(_touchDelta);
					}
					_sentSwipeStartedEvent = true;
				}
				// integrate the touchDelta with the scroll velocity
				_currentScrollVelocity = Vector2.Lerp(_touchDelta, _currentScrollVelocity, _onMoveIntegrationAmount); // this integration amount should take into account delta time
				if (OnTouchpadScroll != null && _touchDelta.sqrMagnitude > _minimumRequiredMoveDistance)
				{
					OnTouchpadScroll(_touchDelta);
				}
				_lastTouchPosition = data.position;
			}
		}
		protected void TouchPadReleased(TouchPadData data)
		{
			_isTouchpadDown = false;
			// reset this so that the event will be sent on the next swipe
			_sentSwipeStartedEvent = false;

			if (OnTouchpadUp != null)
			{
				OnTouchpadUp();
			}
		}
		private void Update()
		{
			if (!_isTouchpadDown && _currentScrollVelocity != Vector2.zero)
			{
				if (OnTouchpadScroll != null)
				{
					OnTouchpadScroll(_currentScrollVelocity);
				}
				_currentScrollVelocity = Vector2.Lerp(_currentScrollVelocity, Vector2.zero, Time.deltaTime * 1.0f / _scrollDampTime);
			}
		}
		/// <summary>
		/// Force set the current velocity of this component. Useful for clearing the velocity on first hover.
		/// </summary>
		/// <param name="newVelocity"></param>
		public void SetVelocity(Vector2 newVelocity)
		{
			_currentScrollVelocity = newVelocity;
		}
	}
}
