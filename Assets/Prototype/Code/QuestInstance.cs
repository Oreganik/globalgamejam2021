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
	public class QuestInstance : MonoBehaviour
	{
		private enum State { Invalid, Inactive, Available, Intro, Search, Return, Outro, Results, Complete }

		public bool IsComplete
		{
			get { return _state == State.Complete; }
		}

		private GameObject _giverObject;
		private GameObject _itemObject;
		private GameObject _receiverObject;
		private GameObject _triggerObject;
		private Quest _quest;
		private QuestManager _questManager;
		private State _state;

		public void HandleTrigger (QuestTrigger.Type type)
		{
			switch (type)
			{
				// Hero has interacted with the quest giver
				case QuestTrigger.Type.Giver:
					// Only react if the quest is available
					if (_state == State.Available)
					{
						GoToState(State.Intro);
					}
					break;

				// Hero has interacted with the quest item
				case QuestTrigger.Type.Item:
					// Only react if the hero is search for the item
					if (_state == State.Search)
					{
						GoToState(State.Return);
					}
					break;

				// Hero has interacted with the quest receiver
				case QuestTrigger.Type.Receiver:
					// Only react if the hero is returning an item
					if (_state == State.Return)
					{
						GoToState(State.Outro);
					}
					break;
			}
		}

		public void Initialize (Quest quest, QuestManager questManager)
		{
			_quest = quest;
			_questManager = questManager;

			if (quest.GiverPrefab == null || quest.ItemPrefab == null)
			{
				Debug.LogErrorFormat("Quest " + quest.Id + " is missing a prefab reference");
				_state = State.Complete;
				return;
			}

			Location location = LocationDatabase.Get(quest.GiverLocationId);
			_giverObject = Instantiate(quest.GiverPrefab, location.transform.position, location.transform.rotation);
			_giverObject.transform.parent = transform;

			location = LocationDatabase.Get(quest.ItemLocationId);
			_itemObject = Instantiate(quest.ItemPrefab, location.transform.position, location.transform.rotation);
			_itemObject.transform.parent = transform;

			if (quest.ReturnToOtherEntity)
			{
				if (quest.ReceiverPrefab == null)
				{
					Debug.LogErrorFormat("Quest " + quest.Id + " is missing a prefab reference");
					_state = State.Complete;
					return;
				}

				location = LocationDatabase.Get(quest.ReceiverLocationId);
				_receiverObject = Instantiate(quest.ReceiverPrefab, location.transform.position, location.transform.rotation);
				_receiverObject.transform.parent = transform;
			}

			GoToState(State.Inactive);
		}

		public void SetAvailable ()
		{
			GoToState(State.Available);
		}

		public void SetUnavailable ()
		{
			GoToState(State.Inactive);
		}

		public void StartQuest ()
		{
			GoToState(State.Search);
		}

		private void GoToState (State newState)
		{
			// Once the state is complete, do nothing else
			if (_state == State.Complete) return;

			// Don't re-enter the same state
			if (_state == newState) 
			{
				Debug.LogWarningFormat("Tried to re-enter state {0} on quest id {1}", newState.ToString(), _quest.Id);
				return;
			}

			_state = newState;

			// Make state easy to read in editor
			if (Application.isEditor)
			{
				gameObject.name = "[Quest] " + _quest.Id + " - " + _state.ToString();
			}

			if (_triggerObject != null)
			{
				Destroy(_triggerObject);
			}

			// In the order they progress during gameplay
			switch (newState)
			{
				case State.Inactive:
					break;

				case State.Available:
					_triggerObject = Instantiate(_questManager.GiverTriggerPrefab, _giverObject.transform);
					_triggerObject.transform.localPosition = Vector3.zero;
					_triggerObject.transform.localRotation = Quaternion.identity;
					_triggerObject.GetComponent<QuestTrigger>().Initialize(this);
					break;

				case State.Intro:
					HeroMotion.Enabled = false;
					XrPrototypeKit.Menus.DialogMenuController.OpenOneOption("LOST!", _quest.IntroText, "Ok", () => 
					{ 
						XrPrototypeKit.Menus.DialogMenuController.Close();
						HeroMotion.Enabled = true;
						GoToState(State.Search);
					});
					break;

				case State.Search:
					_triggerObject = Instantiate(_questManager.ItemTriggerPrefab, _itemObject.transform);
					_triggerObject.transform.localPosition = Vector3.zero;
					_triggerObject.transform.localRotation = Quaternion.identity;
					_triggerObject.GetComponent<QuestTrigger>().Initialize(this);
					Compass.Instance.SetTarget(_itemObject.transform);
					break;

				case State.Return:
					// Destroy the item object (this is currently confusing b/c it has no feedback)
					Destroy(_itemObject);

					// Create a quest trigger for the receiver
					Transform receiverTransform = _quest.ReturnToOtherEntity ? _receiverObject.transform : _giverObject.transform;
					_triggerObject = Instantiate(_questManager.ReceiverTriggerPrefab, receiverTransform);
					_triggerObject.transform.localPosition = Vector3.zero;
					_triggerObject.transform.localRotation = Quaternion.identity;
					_triggerObject.GetComponent<QuestTrigger>().Initialize(this);

					// Point the compass at the receiver
					Compass.Instance.SetTarget(receiverTransform);
					break;

				case State.Outro:
					Compass.Instance.Hide();
					HeroMotion.Enabled = false;
					XrPrototypeKit.Menus.DialogMenuController.OpenOneOption("FOUND!", _quest.FinishText, "Ok", () => 
					{ 
						XrPrototypeKit.Menus.DialogMenuController.Close();
						HeroMotion.Enabled = true;
						GoToState(State.Results);
					});
					break;

				case State.Results:
					GoToState(State.Complete);
					break;

				case State.Complete:
					break;
			}
		}
	}
}
