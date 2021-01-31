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
	public class LocationDatabase : MonoBehaviour 
	{
		private static Dictionary<string, Location> s_locationMap;

		public static Location Get (string id)
		{
			if (s_locationMap == null) return null;

			Location location = null;
			if (s_locationMap.TryGetValue(id, out location) == false)
			{
				Debug.LogErrorFormat("LocationDatabase: No location with id {0}", id);
			}
			return location;
		}

		public static void Register (Location location)
		{
			if (s_locationMap == null)
			{
				s_locationMap = new Dictionary<string, Location>();
			}

			if (s_locationMap.ContainsKey(location.Id))
			{
				Debug.LogError("Duplicate location ID " + location.Id);
				return;
			}
			s_locationMap.Add(location.Id, location);
		}
	}
}
