using UnityEngine;
using UnityEngine.UI;

namespace XrPrototypeKit.Menus
{
	/// <summary>
	/// Alpha-blends in/out the specified hover renderer on highlight.
	/// Must be paired with a MenuButton.
	/// </summary>
	public class ButtonHighlightEffect : MonoBehaviour
	{
		#pragma warning disable 0649
		[SerializeField] private Renderer _renderer;
		[SerializeField] private string _materialColorName = "_Color";
		[SerializeField] private Image _image;
		#pragma warning restore 0649

		private static MaterialPropertyBlock s_propertyBlock;

		private float _timer;
		private float _direction;
		private MenuButton _menuButton;

		void Start()
		{
			if (s_propertyBlock == null)
			{
				s_propertyBlock = new MaterialPropertyBlock();
			}

			_menuButton = GetComponent<MenuButton>();
			if (_menuButton == null)
			{
				Debug.LogError("null menu button on " + gameObject.name);
				Destroy(this);
				return;
			}
			_menuButton.OnHighlightBegin += HandleHighlightBegin;
			_menuButton.OnHighlightEnd += HandleHighlightEnd;
		}

		void Update()
		{
			_timer = Mathf.Clamp(_timer + _direction * Time.deltaTime, 0, _menuButton.MenuController.IconSettings.HoverColorTime);
			float t = Mathf.Clamp01(_timer / _menuButton.MenuController.IconSettings.HoverColorTime);
			Color color = Color.Lerp(_menuButton.MenuController.IconSettings.UnhoveredColor, _menuButton.MenuController.IconSettings.HoveredColor, t);

			if (_renderer)
			{
				_renderer.GetPropertyBlock(s_propertyBlock);
				s_propertyBlock.SetColor(_materialColorName, color);
				_renderer.SetPropertyBlock(s_propertyBlock);
			}
			else if (_image)
			{
				_image.color = color;
			}
		}

		void OnDestroy()
		{
			if (_menuButton)
			{
				_menuButton.OnHighlightBegin -= HandleHighlightBegin;
				_menuButton.OnHighlightEnd -= HandleHighlightEnd;
			}
		}

		void HandleHighlightBegin()
		{
			_direction = 1;
		}

		void HandleHighlightEnd()
		{
			_direction = -1;
		}
	}
}
