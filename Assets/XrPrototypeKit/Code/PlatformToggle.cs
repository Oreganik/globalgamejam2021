using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>Sets this object active or inactive on Awake based on its supported platform</summary>
	// https://forum.unity.com/threads/property-drawer-for-enum-flags-masks-download.517750/
	// https://connect.unity.com/p/bitmasks-and-enum-flags
	[DefaultExecutionOrder(-100)]
	public class PlatformToggle : MonoBehaviour
	{
		public enum SessionType
		{
			Disabled,
			DeviceOnly,
			EditorOnly,
			DeviceAndEditor
		}

		#pragma warning disable 0649
		[SerializeField] private SessionType _magicLeap;
		[SerializeField] private SessionType _iOS;
		[SerializeField] private SessionType _android;
		[SerializeField] private SessionType _standalone;
		#pragma warning restore 0649

		private bool IsSessionSupported (SessionType session)
		{
			if (session == SessionType.Disabled) return false;
			if (session == SessionType.DeviceAndEditor) return true;
			if (session == SessionType.DeviceOnly) return Application.isEditor == false;
			if (session == SessionType.EditorOnly) return Application.isEditor;
			Debug.LogFormat("PlatformToggle.IsSessionSupported: {0} ???", session.ToString());
			return false;
		}

		protected void Awake ()
		{
			bool isSupported = false;
#if PLATFORM_LUMIN
			isSupported = IsSessionSupported(_magicLeap);
#elif UNITY_ANDROID
			isSupported = IsSessionSupported(_android);
#elif UNITY_IOS
			isSupported = IsSessionSupported(_iOS);
#elif UNITY_STANDALONE
			isSupported = IsSessionSupported(_standalone);
#endif
			if (isSupported == false)
			{
				DestroyImmediate(gameObject);
			}
		}
	}
}
