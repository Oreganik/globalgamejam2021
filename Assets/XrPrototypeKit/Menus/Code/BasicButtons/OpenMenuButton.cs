using UnityEngine;

namespace XrPrototypeKit.Menus
{
	public class OpenMenuButton : MenuButton
	{
		public string TargetMenuId
		{
			get
			{
				if (string.IsNullOrEmpty(_customTargetMenu))
				{
					return _targetMenu.ToString().ToLower();
				}
				return _customTargetMenu.ToLower();
			}
		}

		#pragma warning disable 0649
		[SerializeField] private bool _debugOnly;
		[SerializeField] private MenuName _targetMenu;
		[SerializeField] private string _customTargetMenu;
		#pragma warning restore 0649

		protected override void HandleClick ()
		{
			_menuController.MenuManager.Open(TargetMenuId);
		}

#if !DEBUG
		protected override void OnAwake ()
		{
			gameObject.SetActive(!_debugOnly);
		}
#endif
	}
}
