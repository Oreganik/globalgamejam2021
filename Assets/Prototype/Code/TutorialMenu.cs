// GLOBAL GAME JAM 2021
// Shaquan Ladson & Ted Brown

using Jambox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XrPrototypeKit.Menus;

namespace Prototype
{
	/// <summary>
	/// 
	/// </summary>
	public class TutorialMenu : MonoBehaviour 
	{
		public GameObject _buttonContainer;

		private bool _passed;
		private float _minDemoDuration = 2;

		// default is false
		private bool RequiresDemo ()
		{
			MenuName menuName = GetComponent<MenuController>().MenuName;
			switch (menuName)
			{
				case MenuName.HowToDescend: 
				case MenuName.HowToFly: 
				case MenuName.HowToRise: 
					return true;
			}
			return false;
		}

		protected void Awake ()
		{
			GetComponent<MenuController>().OnFinishOpening += HandleMenuFinishOpen;
		}

		private void HandleMenuFinishOpen ()
		{
			if (RequiresDemo() == false || _passed)
			{
				_buttonContainer.SetActive(true);
				return;
			}

			MenuName menuName = GetComponent<MenuController>().MenuName;

			if (menuName == MenuName.HowToRise)
			{
				HeroMotion.OnLift += HandleLift;
			}
			else if (menuName == MenuName.HowToFly)
			{
				HeroMotion.OnFly += HandleFly;
			}
			else if (menuName == MenuName.HowToDescend)
			{
				HeroMotion.OnLand += HandleLand;
			}

			_buttonContainer.SetActive(false);
		}

		protected void OnEnable ()
		{
			// hero can only move if no demo is required
			HeroMotion.Enabled = RequiresDemo();
		}

		private void OnDisable ()
		{
			HeroMotion.Enabled = true;
			HeroMotion.OnLift -= HandleLift;
			HeroMotion.OnFly -= HandleFly;
			HeroMotion.OnLand -= HandleLand;
		}

		private void HandleReadyToProgress ()
		{
			_buttonContainer.SetActive(true);
			_passed = true;
		}

		private void HandleLift (float time)
		{
			Debug.Log(time);
			if (time > _minDemoDuration) HandleReadyToProgress();
		}

		private void HandleFly (float time)
		{
			if (time > _minDemoDuration) HandleReadyToProgress();
		}

		private void HandleLand (float time)
		{
			if (time > _minDemoDuration) HandleReadyToProgress();
		}
	}
}
