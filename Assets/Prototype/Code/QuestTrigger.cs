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
	public class QuestTrigger : MonoBehaviour 
	{
		public enum Type { Giver, Item, Receiver }

		public Type _type;

		private QuestInstance _questInstance;

		public void Initialize (QuestInstance questInstance)
		{
			_questInstance = questInstance;
		}

		protected void OnTriggerEnter (Collider collider)
		{
			if (collider.gameObject.layer == LayerMask.NameToLayer("Hero"))
			{
				_questInstance.HandleTrigger(_type);
			}
		}
	}
}
