using XrPrototypeKit.SixDofControllers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XrPrototypeKit.Menus
{
	public class MenuController : MonoBehaviour
	{
		public enum State
		{
			Closed,
			Appear,
			Open,
			Disappear
		}

		public Action OnFinishClosing;
		public Action OnFinishOpening;
		public Action OnStartOpening;
		public Action OnStartClosing;

		public bool IsOpen
		{
			get { return _state == State.Open; }
		}

		public bool IsClosed
		{
			get { return _state == State.Closed; }
		}

		public bool IsInteractable
		{
			get 
			{ 
				// This enforces a "one button can be clicked at a time" rule.
				// Remove this to allow multi-click. (tbrown)
				if (_clickedIcon != null)
				{
					return false;
				}

				return _isInteractable; 
			}
		}

		public ButtonSettings IconSettings
		{
			get { return _customIconSettings == null ? _menuManager.IconSettings : _customIconSettings; }
		}

		/// <summary>Icons can be read and their values modified, but they can't be removed from the controller</summary>
		public IList<MenuButton> MenuIcons
		{
			get { return Array.AsReadOnly(_menuIcons); }
		}

		public MenuManager MenuManager
		{
			get { return _menuManager; }
		}

		public string MenuId
		{
			get { return _menuId; }
		}

		private static List<RaycastResult> s_raycastResults;

		#pragma warning disable 0649
		[SerializeField] private MenuName _menuName;
		[Tooltip("Setting a value here means the menu name is ignored")]
		[SerializeField] private string _customMenuName;
		[Tooltip("Override icon settings by putting a different reference here")]
		[SerializeField] private ButtonSettings _customIconSettings;
		#pragma warning restore 0649

		protected MenuButton[] _menuIcons;

		private bool _isInteractable;
		private float _openTime;
		private LayerMask _layerMask;
		private MenuButton _clickedIcon;
		private MenuButton _hoveredIcon;
		private MenuHeader[] _menuHeaders;
		private MenuIconConfig _config;
		private MenuManager _menuManager;
		private State _state;
		private string _menuId;
		private Timer _timer;

		public void Close (bool immediately = false)
		{
			// Gracefully handle application shutdown
			if (this == null || gameObject == null)
			{
				return;
			}

			// TODO: This will lead to unexpected behavior if you:
			// 1. close a menu
			// 2. while it is opening
			// 3. by opening another menu
			if (_state != State.Open && immediately == false)
			{
				return;
			}

			_isInteractable = false;

			if (OnStartClosing != null)
			{
				OnStartClosing();
			}

			if (immediately || _menuIcons.Length == 0)
			{
				FinishClosing();
			}
			else
			{
				float transitionTime = _config ? _config.TimeToExit : 0;

				foreach (MenuButton icon in _menuIcons)
				{
					icon.Disappear(transitionTime);
				}

				foreach (MenuHeader header in _menuHeaders)
				{
					header.Disappear(transitionTime);
				}
				
				SetOpenPercentage(1);
				_timer = new Timer(transitionTime);
				_state = State.Disappear;
				enabled = true;
			}
		}

		public static string ConvertMenuNameToId (MenuName menuName)
		{
			return menuName.ToString().ToLower();
		}

		public void DisableInteraction ()
		{
			if (_hoveredIcon != null)
			{
				_hoveredIcon.Unhover();
			}
			_hoveredIcon = null;

			_isInteractable = false;
		}

		public void EnableInteraction ()
		{
			_isInteractable = true;
		}

		public void HandleIconClick (MenuButton menuIcon)
		{
			if (_isInteractable && _clickedIcon == null)
			{
				_clickedIcon = menuIcon;
				_clickedIcon.OnClickEnd += HandleIconClickEnd;
				_clickedIcon.Click();
			}
		}

		public void HideIconsExcept (MenuButton target)
		{
			float transitionTime = _config ? _config.TimeToExit : 0;

			foreach (MenuButton icon in _menuIcons)
			{
				if (icon != target)
				{
					icon.Disappear(transitionTime);
				}
			}

			foreach (MenuHeader header in _menuHeaders)
			{
				header.Disappear(transitionTime);
			}
		}

		public void Open (bool immediately = false)
		{
			if (_state != State.Closed)
			{
				Debug.LogError("Can't open " + MenuId + " because its state is " + _state.ToString());
				return;
			}

			gameObject.SetActive(true);

			if (OnStartOpening != null)
			{
				OnStartOpening();
			}

			if (immediately || _menuIcons.Length == 0)
			{
				FinishOpening();
			}
			else
			{
				float transitionTime = _config ? _config.TimeToEnter : 0;

				if (_config == null)
				{
					//Debug.LogWarning("No menu configuration set");
				}

				foreach (MenuButton icon in _menuIcons)
				{
					icon.Appear(transitionTime);
				}

				foreach (MenuHeader header in _menuHeaders)
				{
					header.Appear(transitionTime);
				}

				SetOpenPercentage(0);
				_timer = new Timer(transitionTime);
				_state = State.Appear;
				enabled = true;
			}

			_openTime = Time.time;
		}

		public void PlaySelectEffectAndClose (MenuButton targetIcon)
		{
			if (_state != State.Open)
			{
				return;
			}

			foreach (MenuButton icon in _menuIcons)
			{
				if (icon != targetIcon)
				{
					icon.Disappear(_config.TimeToExit);
				}
			}

			foreach (MenuHeader header in _menuHeaders)
			{
				header.Disappear(_config.TimeToExit);
			}

			SetOpenPercentage(1);
			_timer = new Timer(_config.TimeToExit);
			_state = State.Disappear;
			enabled = true;
		}

		/// <summary>Specifically used to restore state after HideIconsExcept is called</summary>
		public void ShowIconsAfterBeingHidden ()
		{
			foreach (MenuButton icon in _menuIcons)
			{
				icon.Appear(_config.TimeToEnter);
			}

			foreach (MenuHeader header in _menuHeaders)
			{
				header.Appear(_config.TimeToEnter);
			}
		}

		protected virtual void AfterAwake ()
		{
		}

		protected virtual bool InitializeIcons ()
		{
			_menuIcons = GetComponentsInChildren<MenuButton>();
			return true;
		}

		private void FinishClosing ()
		{
			_state = State.Closed;
			SetOpenPercentage(0);
			gameObject.SetActive(false);
			enabled = false;

			if (OnFinishClosing != null)
			{
				OnFinishClosing();
			}
		}

		private void FinishOpening ()
		{
			_hoveredIcon = null;
			_isInteractable = true;
			_state = State.Open;
			SetOpenPercentage(1);
			gameObject.SetActive(true);

			if (OnFinishOpening != null)
			{
				OnFinishOpening();
			}
		}

		private void HandleIconClickEnd ()
		{
			if (_clickedIcon)
			{
				_clickedIcon.OnClickEnd -= HandleIconClickEnd;
			}
			_clickedIcon = null;
		}

		private void UpdateClose ()
		{
			_timer.Update(Time.deltaTime);
			if (_timer.IsComplete)
			{
				FinishClosing();
			}
			else
			{
				SetOpenPercentage(1 - _timer.t);
			}
		}

		private void UpdateOpen ()
		{
			_timer.Update(Time.deltaTime);
			if (_timer.IsComplete)
			{
				FinishOpening();
			}
			else
			{
				SetOpenPercentage(_timer.t);
			}
		}

		// kept around for curiosity purposes. arbitrary raycasts don't hit canvas objects,
		// so be sure to add box collider components to your menu buttons.
		// also be sure to go through and add the world camera to every child canvas.
		private static GameObject GetCanvasObjectUnderMouseCursor ()
		{
			if (s_raycastResults == null)
			{
				s_raycastResults = new List<RaycastResult>();
			}

			GameObject targetObject = null;
			float nearestDistance = float.MaxValue;
			PointerEventData pointerData = new PointerEventData(EventSystem.current);
			pointerData.position = Input.mousePosition;

			EventSystem.current.RaycastAll(pointerData, s_raycastResults);

			foreach (RaycastResult result in s_raycastResults)
			{
				if (result.distance < nearestDistance)
				{
					targetObject = result.gameObject;
					nearestDistance = result.distance;
				}
			}

			s_raycastResults.Clear();
			return targetObject;
		}

		private void UpdateActive ()
		{
			if (_isInteractable == false)
			{
				return;
			}

			// If our controller goes offline, unhover the active icon.
			SixDofController device = SixDofControllerManager.Instance?.GetBestActiveDevice();
			if(device == null)
			{
				Debug.LogWarning("No device found. Unhovering everything.");
				if(_hoveredIcon != null)
				{
					_hoveredIcon.Unhover();
					_hoveredIcon = null;
				}
				return;
			}

			// Detect the nearest hover target
			Debug.DrawRay(device.transform.position, device.transform.position + (device.transform.forward * 1000), Color.blue, 0.02f);

			RaycastHit hit;
			MenuButton currentTarget = null;

			// Scan for 3D physics colliders
			if (Physics.Raycast(device.transform.position, device.transform.forward, out hit, 1000, _layerMask, QueryTriggerInteraction.Ignore))
			{
				currentTarget = hit.collider.GetComponentInParent<MenuButton>();
			}

			// Hover and Unhover if necessary
			if (_hoveredIcon != currentTarget)
			{
				if (_hoveredIcon != null)
				{
					_hoveredIcon.Unhover();

					// Clear the hovered icon reference every time the target changes.
					// If we are waiting for a clicked icon to finish animating,
					// we won't be setting it again this frame.
					_hoveredIcon = null;
				}

				// Only hover another icon if a clicked icon is not finishing its animation.
				if (_clickedIcon == null && currentTarget != null)
				{
					// The target may refuse to be hovered due to a timeout
					if (currentTarget.Hover())
					{
						_hoveredIcon = currentTarget;
					}
					else
					{
						_hoveredIcon = null;
					}
				}
			}

			if (_hoveredIcon != null && SixDofControllerManager.Instance.GetButtonPressedThisFrame(InputButton.Trigger))
			{
				HandleIconClick(_hoveredIcon);
			}
		}

		private void SetOpenPercentage (float value)
		{
			value = Mathf.Clamp01(value);
		}

		protected void Awake ()
		{
			// Set the Menu ID
			if (_menuName == MenuName.None || _menuName == MenuName.PREVIOUS)
			{
				Debug.LogErrorFormat("{0} has an invalid MenuName of {1}", gameObject.name, _menuName);
				_menuId = "INVALID";
			}
			else if (_menuName == MenuName.CUSTOM)
			{
				_menuId = _customMenuName.ToLower();
			}
			else
			{
				if (string.IsNullOrEmpty(_customMenuName) == false)
				{
					Debug.LogWarningFormat("{0} has a custom name [{1}] but also a menu name {2}. Using {2}.", gameObject.name, _customMenuName, _menuName);
				}
				_menuId = ConvertMenuNameToId(_menuName);
			}

			InitializeIcons();
			_menuHeaders = GetComponentsInChildren<MenuHeader>();
			_config = GetComponentInParent<MenuIconConfig>();
			_menuManager = GetComponentInParent<MenuManager>();
			_layerMask = 1 << LayerMask.NameToLayer("UI");
			AfterAwake();
		}

		protected void Start ()
		{
			// If we close (and set this inactive) on Awake, the buttons won't get their own Awake call.
			// This creates null refs and other errors if we open the menu and immediately run functions on its buttons.
			// If you want to open a menu on Start, you'll need to work around this somehow.
			FinishClosing();
		}

		protected void Update ()
		{
			switch (_state)
			{
				case State.Disappear:
					UpdateClose();
					break;

				case State.Open:
					UpdateActive();
					break;

				case State.Appear:
					UpdateOpen();
					break;
			}
		}	
	}
}
