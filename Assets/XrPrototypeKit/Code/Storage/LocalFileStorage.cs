using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// Stores text streams as files on the local disk.
	/// Files have a category, which serves as the directory, and an id, which serves as the filename.
	/// </summary>
	// This should be static, but static methods aren't supported for interfaces until C# 8
	public class LocalFileStorage : IStorage
	{
		// note : these could go away if enumerationoptions would compile
		private const string THIS_DIR = ".";
		private const string PREV_DIR = "..";
		private const string BASE_DIR = "PersistentContent"; // the persistent data path has Unity stuff, so we put our info in a subdirectory for easy, safe deletion

		// When an id is passed, we clean it up and save it here.
		private string _cleanKey;
		private string _containerName;
		private string _currentPath;
		private string _dataPath;

		public LocalFileStorage(string containerName)
		{
			_containerName = CleanFilename(containerName);
			_dataPath = Path.Combine(Application.persistentDataPath, BASE_DIR, _containerName);
			if (Directory.Exists(_dataPath) == false)
			{
				Directory.CreateDirectory(_dataPath);
			}
			//Debug.LogFormat("LocalFileStorage: Using directory {0}", _dataPath);
		}

		public bool Delete(string key)
		{
			_cleanKey = CleanFilename(key);
			_currentPath = Path.Combine(_dataPath, _cleanKey);

			// NOTE: Relying on File.Exists before running an operation on that file does not scale in a multiuser environment,
			// because the file might not exist when the delete function is called.
			if (File.Exists(_currentPath))
			{
				try
				{
					File.Delete(_currentPath);
					return true;
				}
				catch
				{
					// no exception is thrown when file.delete fails
					Debug.LogErrorFormat("{0}: Failed to delete {1} [reason unknown]", this.GetType().ToString(), _currentPath);
					return false;
				}
			}
			else
			{
				Debug.LogWarningFormat("{0}: Could not delete {1} [file does not exist, so ... success?]", this.GetType().ToString(), _currentPath);
				return true;
			}
		}

		public static void DestroyAllPersistentContentUsedByThisApplication ()
		{
			DirectoryInfo persistentContentDirectory = new DirectoryInfo(Path.Combine(Application.persistentDataPath, BASE_DIR));
			persistentContentDirectory.Delete(true);
		}

		public bool Exists(string key)
		{
			_cleanKey = CleanFilename(key);
			return File.Exists(Path.Combine(_dataPath, _cleanKey));
		}

		public bool GetTextStream(string key, out string text) // necessary to match the interface
		{
			return GetTextStream(key, out text, ignoreFileNotFound: false);
		}

		/// <summary>Returns a text stream for a specific ID.</summary>
		public bool GetTextStream(string key, out string text, bool ignoreFileNotFound = false)
		{
			_cleanKey = CleanFilename(key);
			_currentPath = Path.Combine(_dataPath, _cleanKey);

			try
			{
				using (StreamReader reader = File.OpenText(_currentPath))
				{
					text = reader.ReadToEnd();
					reader.Dispose();
				}
				return true;
			}
			catch (FileNotFoundException e)
			{
				// Sometimes we expect the file not to exist yet, because we will be creating one.
				if (ignoreFileNotFound == false)
				{
					Debug.LogErrorFormat("{0}.GetTextStream: Error reading key [{1}] in container [{2}]", this.GetType().ToString(), _cleanKey, _containerName);
					Debug.LogErrorFormat($"File not found: '{e}'");
				}
			}
			catch (DirectoryNotFoundException e)
			{
				Debug.LogErrorFormat("{0}.GetTextStream: Error reading key [{1}] in container [{2}]", this.GetType().ToString(), _cleanKey, _containerName);
				Debug.LogErrorFormat($"Directory not found: '{e}'");
			}
			catch (IOException e)
			{
				Debug.LogErrorFormat("{0}.GetTextStream: Error reading key [{1}] in container [{2}]", this.GetType().ToString(), _cleanKey, _containerName);
				Debug.LogErrorFormat($"File could not be opened: '{e}'");
			}

			text = string.Empty;
			return false;
		}

		/// <summary>Writes a text stream for a specific ID in a specific category.</summary>
		public bool SaveTextStream(string key, string text)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.LogErrorFormat("{0}.SaveTextStream: Null or empty string passed as key", this.GetType().ToString());
				return false;
			}

			if (string.IsNullOrEmpty(text))
			{
				Debug.LogErrorFormat("{0}.SaveTextStream: Null or empty string passed as text data", this.GetType().ToString());
				return false;
			}

			_cleanKey = CleanFilename(key);

			if (string.IsNullOrEmpty(_cleanKey))
			{
				Debug.LogErrorFormat("{0}.SaveTextStream: Cleaned key for [{0}] resolved to null or empty string", this.GetType().ToString(), key);
				return false;
			}

			_currentPath = Path.Combine(_dataPath, _cleanKey);

			try
			{
				File.WriteAllText(_currentPath, text);
				Debug.LogFormat("{0}.SaveTextStream: {1}", this.GetType().ToString(), _currentPath);
				return true;
			}
			catch (ArgumentException e)
			{
				Debug.LogErrorFormat("{0}.SaveTextStream: {1}", this.GetType().ToString(), e);
			}
			catch (DirectoryNotFoundException e)
			{
				Debug.LogErrorFormat("{0}.SaveTextStream: {1}", this.GetType().ToString(), e);
			}
			catch (IOException e)
			{
				Debug.LogErrorFormat("{0}.SaveTextStream: {1}", this.GetType().ToString(), e);
			}
			catch (NotSupportedException e)
			{
				Debug.LogErrorFormat("{0}.SaveTextStream: {1}", this.GetType().ToString(), e);
			}
			catch (UnauthorizedAccessException e)
			{
				Debug.LogErrorFormat("{0}.SaveTextStream: {1}", this.GetType().ToString(), e);
				FileAttributes attr = (new FileInfo(_currentPath)).Attributes;
				if ((attr & FileAttributes.ReadOnly) > 0) Debug.LogErrorFormat("File {0} is read-only", _currentPath);
			}
			return false;
		}

		// TODO: Ensure no illegal characters are used for the category or id
		private static string CleanFilename(string name)
		{
			// YO: This is not done yet. absolutely a work in progress. (ted)
			string clean = Regex.Replace(name, "[\\/ :\"*?<>|]+", "");
			return clean;
		}

		public static void TestCleanFilename ()
		{
			Debug.Log("starting clean filename test");
			TestName("test 1", "test1");
			TestName("test\\2", "test2");
			TestName("test\"3", "test3");
			TestName("test???4", "test4");
			Debug.Log("finished clean filename test");
		}

		public static bool TestName (string baseName, string expectedResult)
		{
			string result = CleanFilename(baseName);
			if (result.Equals(expectedResult) == false)
			{
				Debug.LogErrorFormat("FAIL: {0} returned {1} instead of {2}", baseName, result, expectedResult);
				return false;
			}
			return true;
		}
	}
}
