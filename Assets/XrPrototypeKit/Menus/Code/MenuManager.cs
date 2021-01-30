using System;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit.Menus
{
	public class MenuManager : MonoBehaviour
	{
		public static MenuManager Instance;

		public static Action<MenuManager> OnOpenMenuManager;
		public static Action<MenuController> OnOpenSubMenu;

		// this is fragile. I know!
		private const string EMPTY_MENU = "none"; // the first element of MenuName
		private const string PREVIOUS_MENU = "previous"; // probably the second element of MenuName

		public Action OnBeginClose;

		public ButtonSettings IconSettings
		{
			get { return _iconSettings; }
		}

		public string ActiveMenuId
		{
			get
			{
				if (_menuHistory == null)
				{
					return EMPTY_MENU;
				}

				int count = _menuHistory.Count;

				if (count > 0)
				{
					return _menuHistory[count - 1];
				}

				return EMPTY_MENU;
			}
		}

		public string PreviousMenuId
		{
			get
			{
				int count = _menuHistory.Count;
				if (count > 1)
				{
					return _menuHistory[count - 2];
				}
				return EMPTY_MENU;
			}
		}

		#pragma warning disable 0649
		[SerializeField] private GameObject[] _menuPrefabs;
		[SerializeField] private ButtonSettings _iconSettings;
		[Tooltip("Scales all menus by this amount")]
		[SerializeField] private float _menuScaler = 1;
		#pragma warning restore 0649

		//private FlapjackUI _flapjackUI;
		private MenuController _closingMenu;
		private MenuController _nextMenuToOpen;
		private Dictionary<string, MenuController> _menus;
		private List<string> _menuHistory;

		/// <summary>Closes the active menu if (and only if) it matches the menu name parameter</summary>
		public void Close (MenuName menuName, bool immediately = false)
		{
			Close(MenuController.ConvertMenuNameToId(menuName), immediately);
		}

		public void Close (string menuName, bool immediately = false)
		{
			if (menuName.Equals(ActiveMenuId))
			{
				Close(immediately);
			}
			else
			{
//				Debug.LogErrorFormat("Can't close {0} because it doesn't match ActiveMenuId {1}", menuName, ActiveMenuId);
			}
		}

		public void Close (MenuController menuController, bool immediately = false)
		{
			Close(menuController.MenuId, immediately);
		}

		// This will automatically be called when the finite state machine is destroyed,
		// which occurs on shutdown. Do null ref checks on all objects to avoid crashes.
		public void Close (bool immediately = false)
		{
			if (OnBeginClose != null)
			{
				OnBeginClose();
			}
			
			if (ActiveMenuId.Equals(EMPTY_MENU) == false)
			{
				_closingMenu = GetMenu(ActiveMenuId);
				
				if (_closingMenu)
				{
					_closingMenu.Close(immediately);
				}
			}

			if (_menuHistory != null)
			{
				_menuHistory.Clear();		
			}

			SetMenuTitle(string.Empty);
		}

		public MenuController GetMenu (MenuName menuName)
		{
			return GetMenu(MenuController.ConvertMenuNameToId(menuName));
		}

		public MenuController GetMenu (string menuName)
		{
			MenuController menu = null;
			menuName = menuName.ToLower();
			if (_menus != null && _menus.TryGetValue(menuName, out menu) == false)
			{
				Debug.LogError("MenuManager.GetMenuObject did not recognize MenuName." + menuName.ToString());
			}
			return menu;
		}

		public bool IsMenuOpen ()
		{
			if (ActiveMenuId.Equals(PREVIOUS_MENU) || ActiveMenuId.Equals(EMPTY_MENU))
			{
				return false;
			}

			MenuController activeMenu = GetMenu(ActiveMenuId);
			if (activeMenu)
			{
				return activeMenu.IsOpen;
			}

			return false;
		}

		public bool IsMenuOpen (MenuName menuName)
		{
			return IsMenuOpen(MenuController.ConvertMenuNameToId(menuName));
		}

		public bool IsMenuOpen (string menuId)
		{
			MenuController activeMenu = GetMenu(menuId);
			if (activeMenu)
			{
				return activeMenu.IsOpen;
			}
			return false;
		}

		/// <summary>Open the Main Menu</summary>
		public void Open (bool immediately = false)
		{
			Open(MenuName.Home, immediately);
		}

		/// <summary>Open the specified menu</summary>
		public void Open (MenuName menuName, bool immediately = false)
		{
			Open(MenuController.ConvertMenuNameToId(menuName), immediately);
		}

		/// <summary>Open the specified menu</summary>
		public MenuController Open (string menuId, bool immediately = false)
		{
			// TODO: SHOW RAYCAST IF ON LUMIN OR VR
			// Show raycast
			// var device = CoreInputManager.Instance.GetBestActiveDevice();
			// if (!device)
			// {
			// 	return;
			// }
			// var raycast = device.GetComponent<InputRaycast>();
			// if (raycast)
			// {
			// 	if (showRaycast)
			// 	{
			// 		raycast.Show();
			// 	}
			// 	else
			// 	{
			// 		raycast.Hide();
			// 	}
			// }

			string activeMenuName = ActiveMenuId;
			string previousMenuName = PreviousMenuId;
			menuId = menuId.ToLower();

			//Debug.Log("open menu " + menuId);

			// if (activeMenuName == menuId)
			// {
			// 	Debug.LogWarning("Can't reopen a menu: '" + menuId.ToString() + "' is already open. Did you call Close on MenuController instead of MenuManager?");
			// 	return null;
			// }

			if (menuId.Equals(EMPTY_MENU))
			{
				Debug.LogWarning("Can't open a menu named None");
				return null;
			}

			if (menuId.Equals(PREVIOUS_MENU))
			{
				// If opening the previous menu, and the previous menu is empty, just close everything
				if (previousMenuName.Equals(EMPTY_MENU))
				{
					menuId = EMPTY_MENU;
				}
				else
				{
					// Overwrite the next menu name parameter with the last one we opened
					menuId = previousMenuName;

					// Remove one item from the menu history
					_menuHistory.RemoveAt(_menuHistory.Count - 1);
				}
			}
			else
			{
				// Add one item to the menu history
				_menuHistory.Add(menuId);
			}

			// If there is an active menu, close it
			if (activeMenuName.Equals(EMPTY_MENU) == false)
			{
				_closingMenu = GetMenu(activeMenuName);
				_closingMenu.Close(immediately);
			}
			else
			{
				_closingMenu = null;
			}

			MenuController targetMenu = null;

			// The "next menu" is "none" if we are going to the previous menu, and no previous menu exists.
			if (menuId.Equals(EMPTY_MENU))
			{
				Close(immediately);
			}
			else
			{
				targetMenu = GetMenu(menuId);
				// Either open the next menu immediately, or set it to open once the active menu is closed.
				if (immediately)
				{
					targetMenu.Open(immediately: true);
					_nextMenuToOpen = null;
				}
				else
				{
					_nextMenuToOpen = targetMenu;
				}
			}

			// If there was no menu to close, then this is the first menu we've opened.
			if (_closingMenu == null && OnOpenMenuManager != null)
			{
				OnOpenMenuManager(this);
			}

			return targetMenu;
		}

		public void SetMenuTitle (string text)
		{
			// Use this if you have a single element with a menu name
		}

		private void AddMenu (GameObject prefab)
		{
			GameObject menuObject = Instantiate(prefab, transform);
			MenuController menu = menuObject.GetComponent<MenuController>();

			if (menu.MenuId.Equals(EMPTY_MENU) || 
				menu.MenuId.Equals(MenuController.ConvertMenuNameToId(MenuName.PREVIOUS)))
			{
				Debug.LogError(menuObject.name + " menu type of '" + menu.MenuId + "' is not supported", menu.gameObject);
				Destroy(menuObject);
				return;
			}

			if (_menus.ContainsKey(menu.MenuId))
			{
				Debug.LogError(menuObject.name + " can't be added as a duplicate menu of type '" + menu.MenuId + "'", menu.gameObject);
				Destroy(menuObject);
				return;
			}

			menu.transform.localScale = Vector3.one * _menuScaler;

			_menus.Add(menu.MenuId, menu);

			// don't set it inactive here, or awake won't run on the menucontrollers and buttons
		}

		protected void Awake ()
		{
			Instance = this;
			_menuHistory = new List<string>();
			_menus = new Dictionary<string, MenuController>();

			foreach (GameObject prefab in _menuPrefabs)
			{
				AddMenu(prefab);
			}

			// If we close each menu (and set it inactive) on Awake, the buttons won't get their own Awake call.
			// This creates null refs and other errors if we open the menu and immediately run functions on its buttons.
			// Therefore, each menu closes itself on Start.
			// If you want to open a menu on Start, you'll need to work around this somehow.
		}

		protected void Update ()
		{
			// If the next menu is not null, we are waiting for a chance to open it
			if (_nextMenuToOpen != null)
			{
				if (_closingMenu == null || _closingMenu.IsClosed)
				{
					if (OnOpenSubMenu != null)
					{
						OnOpenSubMenu(_nextMenuToOpen);
					}

					_nextMenuToOpen.Open();
					_nextMenuToOpen = null;
				}
			}
		}
	}
}
