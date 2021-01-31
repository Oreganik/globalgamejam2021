// GLOBAL GAME JAM 2021
// Shaquan Ladson & Ted Brown

using Jambox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prototype
{
	/// <summary>
	/// 
	/// </summary>
	[CustomEditor(typeof(Quest))]
	public class QuestEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("IntroText"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("FinishText"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("GiverPrefab"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("GiverLocationId"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemPrefab"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemLocationId"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ReturnToOtherEntity"));

			if (serializedObject.FindProperty("ReturnToOtherEntity").boolValue)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("ReceiverPrefab"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("ReceiverLocationId"));
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
