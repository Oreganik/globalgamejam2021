using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Array-backed circular buffer with fixed capacity
/// </summary>
public class CircularBuffer<T> : ICollection<T>
{
	protected int _count = 0;
	protected int _start = 0;
	protected T[] _data;

	public CircularBuffer(int capacity)
	{
		_data = new T[capacity];
	}

	#region ICollection

	public int Count
	{
		get { return _count; }
	}

	public bool IsReadOnly
	{
		get { return false; }
	}

	public virtual void Add(T element)
	{
		int lastIdx = (_start + _count) % _data.Length;
		_data[lastIdx] = element;
		_count++;
		if (_count > _data.Length)
		{
			// Wrap
			_count = _data.Length;
			_start++;
			if (_start > _data.Length)
			{
				_start = 0;
			}
		}
	}

	/// <summary>
	/// Resets the buffer to begin at zero; does not clear out any held references.
	/// </summary>
	public virtual void Clear()
	{
		_start = 0;
		_count = 0;
	}

	public bool Contains(T item)
	{
		if (item == null)
		{
			return false;
		}

		foreach (T t in this)
		{
			if (item.Equals(t))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		// Copy the first part of the circular buffer: from _start to end of _data
		int length1 = _data.Length - _start;
		if (length1 > 0)
		{
			length1 = Math.Min(length1, array.Length - arrayIndex);
			if (length1 > 0)
			{
				Array.Copy(_data, _start, array, arrayIndex, length1);
			}
		}

		// Copy the second part of the circular buffer: from _data[0] through remaining length
		int length2 = _count - length1;
		if (length2 > 0)
		{
			length2 = Math.Min(length2, array.Length - arrayIndex - length1);
			if (length2 > 0)
			{
				Array.Copy(_data, 0, array, arrayIndex + length1, length2);
			}
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = _start; i < _count && i < _data.Length; i++)
		{
			yield return _data[i];
		}
		for (int i = 0; i < _start && i < (_data.Length - (_data.Length - _start)); i++)
		{
			yield return _data[i];
		}
	}

	public bool Remove(T item)
	{
		throw new NotImplementedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion ICollection
}
