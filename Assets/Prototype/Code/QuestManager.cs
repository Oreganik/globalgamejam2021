// GLOBAL GAME JAM 2021
// Shaquan Ladson & Ted Brown

using Jambox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
	/// <summary>
	/// 
	/// </summary>
	public class QuestManager : MonoBehaviour 
	{
		public static QuestManager Instance;

		public Quest[] _quests;
		public GameObject _giverTriggerPrefab;

		private GameObject _giver;
		private GameObject _item;
		private Quest _activeQuest;

		public void Activate (string id)
		{
			bool foundQuest = false;
			foreach (Quest quest in _quests)
			{
				if (quest.Id.Equals(id))
				{
					Location location = LocationDatabase.Get(quest.GiverLocationId);
					_giver = Instantiate(quest.GiverPrefab, location.transform.position, location.transform.rotation);

					GameObject trigger = Instantiate(_giverTriggerPrefab, _giver.transform);
					trigger.transform.localPosition = Vector3.zero;
					trigger.transform.localRotation = Quaternion.identity;

					Compass.Instance.SetTarget(_giver.transform);

					foundQuest = true;
					_activeQuest = quest;
				}
			}

			if (foundQuest == false)
			{
				Debug.LogErrorFormat("Could not find quest id {0}", id);
			}
		}

		public void ShowQuestIntro ()
		{
			Debug.Log(_activeQuest.IntroText);
			StartQuest();
		}

		public void StartQuest ()
		{
			Location location = LocationDatabase.Get(_activeQuest.ItemLocationId);
			_item = Instantiate(_activeQuest.ItemPrefab, location.transform.position, location.transform.rotation);
			Compass.Instance.SetTarget(_item.transform);
		}

		protected void Awake ()
		{
			Instance = this;
		}

		protected void Start ()
		{
			Activate("test");
		}
	}
}
