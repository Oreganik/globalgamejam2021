using XrPrototypeKit;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XrPrototypeKit.Menus
{
	public class MenuButton : MonoBehaviour, IPointerClickHandler
	{
		// The collider is scaled down while idle, and is restored to 100% of its base size when highlighted.
		// This prevents situations where the pointer is rapidly flicking back and forth between "hover" and "unhover."
		private const float IDLE_COLLIDER_SCALE = 0.8f;

		public enum State
		{
			Inactive,
			Appear,
			Idle,
			Highlight,
			Click,
			Disappear
		}

		public bool IsBusy
		{
			get { return _isHovered || _state == State.Click; }
		}

		public MenuController MenuController
		{
			get { return _menuController; }
		}

		public RectTransform rectTransform
		{
			get
			{
				if (_rectTransform == null)
				{
					_rectTransform = GetComponent<RectTransform>();
				}
				return _rectTransform;
			}
		}

		public Action OnAppearBegin;
		public Action<float> OnAppearUpdate; // _timer.t is passed as the float, indicating how close the Enter transition is to completion.
		public Action OnAppearEnd;

		public Action OnClickBegin;
		public Action OnClickUpdate;
		public Action OnClickEnd;

		public Action OnDisappearBegin;
		public Action<float> OnDisappearUpdate; // _timer.t is passed as the float, indicating how close the Exit transition is to completion.
		public Action OnDisappearEnd;

		public Action OnHighlightBegin;
		public Action OnHighlightUpdate;
		public Action OnHighlightEnd;

		public Action OnIdleBegin;
		public Action OnIdleUpdate;
		public Action OnIdleEnd;

		[Tooltip("If false, clicking this button will suspend menu interaction until FinishClickResponse is called")]
		[SerializeField] protected bool _autoFinishClickResponse = true;

		// It's certainly possible -- and sometimes preferable -- to make a child of this class for every function.
		// However, for prototyping, etc., setting an action is an easier lift.
		protected Action _clickAction;

		protected ButtonSelectEffect _iconSelectEffect; // this may be null: always protect against it
		protected MenuController _menuController;
		protected State _state;
		protected TMP_Text _label;

		// We want to keep track of whether or not the icon is hovered, regardless of its current state.
		// This is because an icon that is hovered in the "Appear" state won't react until it transitions to "Idle".
		private bool _isHovered;
		private BoxCollider _boxCollider;
		private RectTransform _rectTransform;
		private Timer _timer;
		private Vector3 _baseColliderSize;

		public void Appear (float duration)
		{
			_timer.StartNewTimer(duration);
			GoToState(State.Appear);
		}

		public void Click ()
		{
			// These rules enforce proper state prior to click.
			// Definitely remove if this cramps your style. (tbrown)
#if UNITY_ANDROID || UNITY_IOS
			// Icons have no Highlight state on mobile.
			// THIS BREAKS OCULUS
			//if (_state != State.Idle) return;
#else
			// This is an arbitrary rule for devices with 6dof pointers.
			if (_state != State.Highlight) return;
#endif
			GoToState(State.Click);
		}

		public void Disappear (float duration)
		{
			_timer.StartNewTimer(duration);
			GoToState(State.Disappear);
		}

		protected void FinishClickResponse(bool enableMenuInteraction = true)
		{
			if (enableMenuInteraction)
			{
				_menuController.EnableInteraction();
			}

			if (_isHovered)
			{
				GoToState(State.Highlight);
			}
			else
			{
				GoToState(State.Idle);
			}
		}

		public bool Hover ()
		{
			// This boolean specifies whether or not the cursor is over the icon.
			// It does not describe the state of the icon.
			_isHovered = true;

			if (_state == State.Idle)
			{
				GoToState(State.Highlight);
				return true;
			}

			return false;
		}

		// Used by mobile devices, not Magic Leap
		public void OnPointerClick (PointerEventData data)
		{
			if (_state == State.Idle)
			{
				_menuController.HandleIconClick(this);
			}
		}

		public void SetClickAction (Action action)
		{
			_clickAction = action;
		}

		public void SetLabel (string text, bool insertSpaces = false)
		{
			if (_label)
			{
				if (insertSpaces)
				{
					text = text.Replace("_", " ");
				}
				_label.text = text;
			}
		}

		public void Unhover ()
		{
			_isHovered = false;
			if (_state == State.Highlight)
			{
				GoToState(State.Idle);
			}
		}

		// Give child classes a chance to respond to a click before the OnClickBegin event is called.
		protected virtual void RespondToClick ()
		{
			//_menuController.HideIconsExcept(this);
			_menuController.DisableInteraction();

			if (_iconSelectEffect)
			{
				_iconSelectEffect.Play( () => 
				{ 
					HandleClick();
					if (_autoFinishClickResponse)
					{
						FinishClickResponse();
					}
				});
			}
			else
			{
				HandleClick();
				if (_autoFinishClickResponse)
				{
					FinishClickResponse();
				}
			}
		}

		protected virtual void HandleClick ()
		{
			if (_clickAction != null)
			{
				_clickAction();
			}
			else
			{
				Debug.Log(gameObject.name + " was clicked, but has no action or override.");
			}
		}

		protected virtual void OnAwake ()
		{
		}

		private void GoToState (State newState)
		{
			if (_state == newState)
			{
				return;
			}

			//Debug.Log(gameObject.name + " > " + newState);

			// Exit the previous state
			switch (_state)
			{
				case State.Appear:
					if (OnAppearEnd != null)
					{
						OnAppearEnd();
					}
					break;

				case State.Click:
					if (OnClickEnd != null)
					{
						OnClickEnd();
					}
					break;

				case State.Disappear:
					if (OnDisappearEnd != null)
					{
						OnDisappearEnd();
					}
					break;

				case State.Highlight:
					if (OnHighlightEnd != null)
					{
						OnHighlightEnd();
					}
					break;

				case State.Idle:
					if (OnIdleEnd != null)
					{
						OnIdleEnd();
					}
					break;
			}			

			// Set the new state
			_state = newState;

			// Enter the new state
			switch (_state)
			{
				case State.Appear:
					if (OnAppearBegin != null)
					{
						OnAppearBegin();
					}
					break;

				case State.Click:
					RespondToClick();
					if (OnClickBegin != null)
					{
						OnClickBegin();
					}
					break;

				case State.Disappear:
					if (OnDisappearBegin != null)
					{
						OnDisappearBegin();
					}
					break;

				case State.Highlight:
					if (_boxCollider)
					{
						_boxCollider.size = _baseColliderSize;
					}
					if (OnHighlightBegin != null)
					{
						OnHighlightBegin();
					}
					break;

				case State.Idle:
					if (_boxCollider)
					{
						_boxCollider.size = _baseColliderSize * IDLE_COLLIDER_SCALE;
					}
					if (OnIdleBegin != null)
					{
						OnIdleBegin();
					}
					break;

				case State.Inactive:
					_isHovered = false;
					break;
			}
		}

		protected void Awake ()
		{
			_timer = new Timer(0);
			_menuController = GetComponentInParent<MenuController>();
			_boxCollider = GetComponent<BoxCollider>();
			_iconSelectEffect = GetComponentInChildren<ButtonSelectEffect>();
			_label = GetComponentInChildren<TMP_Text>();

			// canvas buttons won't have a box collider
			if (_boxCollider)
			{
				_baseColliderSize = _boxCollider.size;
			}

			if (GetComponents<MenuButton>().Length > 1)
			{
				Debug.LogError(gameObject.name + " has multiple instances of " + this.GetType().ToString());
			}

			OnAwake();
		}

		// If the object is disabled while in a click state, this will disrupt menu operation.
		protected virtual void OnDisable ()
		{
			if (_state == State.Click)
			{
				FinishClickResponse();
			}
		}

		protected void Update ()
		{
			// State.Click is handled by other components.
			// In that state, we wait for FinishClickResponse to be called.

			_timer.Update(Time.deltaTime);

			switch (_state)
			{
				case State.Appear:
					if (OnAppearUpdate != null)
					{
						OnAppearUpdate(_timer.t);
					}

					if (_timer.IsComplete)
					{
						GoToState(State.Idle);
					}
					break;

				case State.Click:
					if (OnClickUpdate != null)
					{
						OnClickUpdate();
					}
					break;

				case State.Disappear:
					if (OnDisappearUpdate != null)
					{
						OnDisappearUpdate(_timer.t);
					}

					if (_timer.IsComplete)
					{
						GoToState(State.Inactive);
					}
					break;

				case State.Highlight:
					if (OnHighlightUpdate != null)
					{
						OnHighlightUpdate();
					}
					break;

				case State.Idle:
					if (OnIdleUpdate != null)
					{
						OnIdleUpdate();
					}
					break;
			}
		}
	}
}
