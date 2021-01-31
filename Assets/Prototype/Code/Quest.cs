// ANGRY MOUNTAIN GODS
// Copyright 2018 Ted Brown
// Created for Ludum Dare 43
// December 2018

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
	[CreateAssetMenu(fileName = "Quest", menuName = "Prototype/Quest", order = 1)]
	public class Quest : ScriptableObject 
	{
		public string Id;
		public GameObject GiverPrefab;
		public GameObject ReceiverPrefab;
		public GameObject ItemPrefab;
		public string GiverLocationId;
		public string ReceiverLocationId;
		public string ItemLocationId;
		public string IntroText = "Hello";
		public string FinishText = "Thank you";
	}
}
