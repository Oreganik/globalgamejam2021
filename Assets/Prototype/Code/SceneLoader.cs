// TRADERS
// Copyright (c) 2020 Ted Brown

using Jambox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype
{
	/// <summary>
	/// 
	/// </summary>
	public class SceneLoader
	{
		public bool IsLoaded 
		{
			get 
			{ 
				if (_scene == null) return false;
				return _scene.isLoaded;
			}
		}
		
		public float Progress
		{
			get { return _progress; }
		}
		
		private Action _postLoadAction;
		private bool _autoActivate;
		private bool _isLoaded;
		private float _progress;
		private List<GameObject> _rootGameObjects;
		private Scene _scene;
		private string _sceneName;

		public SceneLoader (string sceneName)
		{
			_sceneName = sceneName;
			_rootGameObjects = new List<GameObject>();
		}

		public bool Activate ()
		{
			if (IsLoaded == false) 
			{
				return false;
			}

			foreach (GameObject go in _rootGameObjects)
			{
				go.SetActive(true);
			}
			_rootGameObjects.Clear();

			return true;
		}

		/// <summary>Manually disables all active root game objects.
		/// Stores them in a list for re-activation when necessary.
		/// </summary>
		public void Deactivate ()
		{
			if (_scene != null && _scene.isLoaded)
			{
				GameObject[] allRootGameObjects = _scene.GetRootGameObjects();
				_rootGameObjects.Clear();

				foreach (GameObject go in allRootGameObjects)
				{
					if (go.activeSelf)
					{
						go.SetActive(false);
						_rootGameObjects.Add(go);
					}
				}
			}
		}

		public void FinishLoading ()
		{
			_autoActivate = true;
		}

		public IEnumerator Load (bool autoActivate = true)
		{
			return LoadAndRunAction(null, autoActivate);
		}

		public IEnumerator LoadAndRunAction (Action action, bool autoActivate = true)
		{
			_postLoadAction = action;
			_autoActivate = autoActivate;
			_progress = 0;

			yield return null;

			AsyncOperation ao = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
			ao.allowSceneActivation = _autoActivate;

			while (!ao.isDone)
			{
				yield return null;

				// [0, 0.9] > [0, 1]
				_progress = Mathf.Clamp01(ao.progress / 0.9f);

				// Loading completed
				if (ao.progress >= 0.9f)
				{
					// If set to false when the method was started,
					// calling FinishLoading will set it to true, and this will complete.
					ao.allowSceneActivation = _autoActivate;
				}
			}

			_scene = SceneManager.GetSceneByName(_sceneName);
			SceneManager.SetActiveScene(_scene);

			if (_postLoadAction != null)
			{
				_postLoadAction();
			}
		}
	}
}
