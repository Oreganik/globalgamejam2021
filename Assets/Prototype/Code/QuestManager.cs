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
		#pragma warning restore 0649

		private QuestInstance[] _questInstances;

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
				_questInstances[i].Initialize(_quests[i], this);
				_questInstances[i].SetAvailable();
			}
		}
	}
}
