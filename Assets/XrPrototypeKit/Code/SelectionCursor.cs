using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrPrototypeKit
{
	/// <summary>
	/// 
	/// </summary>
	public class SelectionCursor : MonoBehaviour 
	{
		public static SelectionCursor Instance { get; private set; }

		public Transform _bottomLeft;
		public Transform _bottomRight;
		public Transform _topLeft;
		public Transform _topRight;

		private float _cornerSize = 0.1f;
		private float _expandTime = 0.4f;
		private Vector3 _targetSize;

		public void Contract ()
		{
			FixPosition(_bottomLeft, _targetSize, -1, -1, expand: false);
			FixPosition(_bottomRight, _targetSize, 1, -1, expand: false);
			FixPosition(_topLeft, _targetSize, -1, 1, expand: false);
			FixPosition(_topRight, _targetSize, 1, 1, expand: false);
		}

		public void Expand ()
		{
			Debug.Log("expand");
			FixPosition(_bottomLeft, _targetSize, -1, -1, expand: true);
			FixPosition(_bottomRight, _targetSize, 1, -1, expand: true);
			FixPosition(_topLeft, _targetSize, -1, 1, expand: true);
			FixPosition(_topRight, _targetSize, 1, 1, expand: true);
		}

		public void Hide ()
		{
			ObjectMover.Cancel(_bottomLeft.gameObject);
			ObjectMover.Cancel(_bottomRight.gameObject);
			ObjectMover.Cancel(_topLeft.gameObject);
			ObjectMover.Cancel(_topRight.gameObject);

			_bottomLeft.gameObject.SetActive(false);
			_bottomRight.gameObject.SetActive(false);
			_topLeft.gameObject.SetActive(false);
			_topRight.gameObject.SetActive(false);
		}

		public void SetLocation (Vector3 position, Quaternion rotation)
		{
			transform.position = position;
			transform.rotation = rotation;
			transform.Rotate(Vector3.up * 180, UnityEngine.Space.Self);
		}

		public void Show (Vector3 position, Quaternion rotation, Vector3 size)
		{
			SetLocation(position, rotation);
			_targetSize = size;
			Expand();
		}

		private void FixPosition (Transform t, Vector3 size, int offsetX, int offsetY, bool expand)
		{
			t.gameObject.SetActive(true);
			size = size * 0.5f;
			Vector3 expanded = Vector3.right * size.x * offsetX + Vector3.up * size.y * offsetY;

			size -= Vector3.one * _cornerSize / 2;
			Vector3 contracted = Vector3.right * size.x * offsetX + Vector3.up * size.y * offsetY;

			if (expand)
			{
				ObjectMover.Translate(t.gameObject, contracted, expanded, _expandTime, inWorldSpace: false);
			}
			else
			{
				ObjectMover.Translate(t.gameObject, expanded, contracted, _expandTime, inWorldSpace: false);
			}
		}

		protected void Awake ()
		{
			Instance = this;
			Hide();
		}
	}
}
