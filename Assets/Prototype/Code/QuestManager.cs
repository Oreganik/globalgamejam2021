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

		public GameObject GiverTriggerPrefab
		{
			get { return _giverTriggerPrefab; }
		}

		public GameObject ItemTriggerPrefab
		{
			get { return _itemTriggerPrefab; }
		}

		public GameObject ReceiverTriggerPrefab
		{
			get { return _receiverTriggerPrefab; }
		}

		#pragma warning disable 0649
		[SerializeField] private Quest[] _quests;
		[SerializeField] private GameObject _giverTriggerPrefab;
		[SerializeField] private GameObject _itemTriggerPrefab;
		[SerializeField] private GameObject _receiverTriggerPrefab;
		[SerializeField] private GameObject _defaultCitizenPrefab;
		#pragma warning restore 0649

		private QuestInstance[] _questInstances;

		public void MakeQuestsAvailable ()
		{
			foreach (QuestInstance quest in _questInstances)
			{
				quest.SetAvailable();
			}
		}

		public void HandleQuestComplete (QuestInstance questInstance)
		{
			foreach (QuestInstance qi in _questInstances)
			{
				if (qi.IsComplete) continue;
				qi.SetAvailable();
			}
		}

		public void HandleQuestStart (QuestInstance questInstance)
		{
			foreach (QuestInstance qi in _questInstances)
			{
				if (qi.IsComplete) continue;
				if (qi == questInstance) continue;
				qi.SetUnavailable();
			}
		}

		protected void Awake ()
		{
			Instance = this;
		}

		protected void Start ()
		{
			// Initialize quests on start so locations can register themselves
			_questInstances = new QuestInstance[_quests.Length];
			GameObject questObject = null;
			for (int i = 0; i < _quests.Length; i++)
			{
				questObject = new GameObject();
				_questInstances[i] = questObject.AddComponent<QuestInstance>();
				
				if (_quests[i].GiverPrefab == null)
				{
					_quests[i].GiverPrefab = _defaultCitizenPrefab;
				}

				if (_quests[i].ReceiverPrefab == null)
				{
					_quests[i].ReceiverPrefab = _defaultCitizenPrefab;
				}

				_questInstances[i].Initialize(_quests[i], this);
			}
		}
	}
}
