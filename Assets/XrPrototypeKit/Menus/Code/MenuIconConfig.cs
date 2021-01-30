using UnityEngine;

namespace XrPrototypeKit.Menus
{
	public class MenuIconConfig : MonoBehaviour
	{
		public float TimeToEnter
		{
			get { return _timeToEnter; }
		}

		public float TimeToExit
		{
			get { return _timeToExit; }
		}

		#pragma warning disable 0649
		[SerializeField] private float _timeToEnter = 0.4f;
		[SerializeField] private float _timeToExit = 0.4f;
		#pragma warning restore 0649
	}
}
