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
	public class GiverTrigger : MonoBehaviour 
	{
		protected void OnTriggerEnter (Collider collider)
		{
			if (collider.gameObject.layer == LayerMask.NameToLayer("Hero"))
			{
				Debug.Log("player entered");
				QuestManager.Instance.ShowQuestIntro();
				Destroy(gameObject);
			}
			else
			{
				Debug.Log("something else?");
			}
		}
	}
}
