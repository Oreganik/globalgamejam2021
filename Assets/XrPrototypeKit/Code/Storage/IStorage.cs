using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// An interface for saving and loading text streams.
	/// Assumes data has both a category and a unique id (the key).
	/// If no categories are used, implement methods on the child class that pass an empty string as the category.
	/// </summary>
	public interface IStorage 
	{
		bool Delete (string key);

		bool Exists (string key);

		/// <summary>Returns a text stream for a specific key in a specific category.</summary>
		bool GetTextStream (string key, out string text);

		/// <summary>Writes a text stream for a specific key in a specific category.</summary>
		bool SaveTextStream (string key, string text);
	}
}
