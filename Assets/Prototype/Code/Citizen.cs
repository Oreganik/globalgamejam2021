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
	public class Citizen : MonoBehaviour 
	{
		public Renderer[] _skin;
		public Renderer[] _shirt;
		public Renderer[] _pants;
		public ColorPalette _skinPalette;
		public ColorPalette _shirtPalette;
		public ColorPalette _pantPalette;

		private static MaterialPropertyBlock s_propertyBlock;

		protected void Awake ()
		{
			if (s_propertyBlock == null) s_propertyBlock = new MaterialPropertyBlock();

			Color skinColor = _skinPalette.GetRandom();
			foreach (Renderer r in _skin)
			{
				SetColor(r, skinColor);
			}

			Color shirtColor = _shirtPalette.GetRandom();
			foreach (Renderer r in _shirt)
			{
				SetColor(r, shirtColor);
			}

			Color pantColor = _pantPalette.GetRandom();
			foreach (Renderer r in _pants)
			{
				SetColor(r, pantColor);
			}
		}

		private void SetColor (Renderer r, Color c)
		{
			r.GetPropertyBlock(s_propertyBlock);
			s_propertyBlock.SetColor("_Color", c);
			r.SetPropertyBlock(s_propertyBlock);
		}
	}
}
