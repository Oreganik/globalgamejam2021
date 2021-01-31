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
	public class Session : MonoBehaviour 
	{
		public bool _debugSkipTutorial;
		public bool _debugLoadTestTown;

		private SceneLoader _sceneLoader;

		public static bool GetClick ()
		{
			if (HeroVrInput.Instance)
			{
				return HeroVrInput.Instance.GetClick;
			}

			if (HeroPcInput.Instance)
			{
				return HeroPcInput.Instance.GetClick;
			}

			return false;
		}

		protected void Awake ()
		{
			//if (Application.isEditor && _debugLoadTestTown)
			if (_debugLoadTestTown)
			{
				_sceneLoader = new SceneLoader("TestTown");
			}
			else
			{
				_sceneLoader = new SceneLoader("Prototype");
			}
			
			// hey. you. this is total garbage. don't copy this pattern. 
			// 1. don't allow silent fails by calling a coroutine instead of starting it.
			// 2. don't pass actions when you can subscribe to events.
			StartCoroutine(_sceneLoader.LoadAndRunAction(() => { HandleSceneLoad(); } ));
		}

		protected void HandleSceneLoad ()
		{
			if (Application.isEditor && _debugSkipTutorial)
			{
				QuestManager.Instance.MakeQuestsAvailable();
			}
			else
			{
				MenuManager.Instance.Open(MenuName.Title);
				MenuManager.Instance.OnBeginClose += HandleMenuClose;
			}
		}

		private void HandleMenuClose ()
		{
			MenuManager.Instance.OnBeginClose -= HandleMenuClose;
			if (QuestManager.Instance)
			{
				QuestManager.Instance.MakeQuestsAvailable();
			}
		}
	}
}
