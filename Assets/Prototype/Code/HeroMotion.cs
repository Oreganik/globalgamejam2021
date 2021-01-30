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
	public class HeroMotion : MonoBehaviour 
	{
		private enum State { Idle, Fly, Land, Lift }

		public float _flightSpeed = 5;
		public float _landSpeed = 2;
		public float _liftSpeed = 4;

		private float _speed;
		private float _targetSpeed;
		private State _state;
		private Vector3 _direction;
		private Vector3 _targetDirection;

		public void Fly (Vector3 direction)
		{
			_state = State.Fly;
			_targetDirection = direction.normalized;
			_targetSpeed = _flightSpeed;
		}

		public void Hover ()
		{
			_state = State.Idle;
			_targetSpeed = 0;
		}

		public void Land ()
		{
			_state = State.Land;
			_targetDirection = Vector3.down;
			_targetSpeed = _landSpeed;
		}

		public void Lift ()
		{
			_state = State.Lift;
			_targetDirection = Vector3.up;
			_targetSpeed = _liftSpeed;
		}

		protected void FixedUpdate ()
		{
			_speed = Mathf.Lerp(_speed, _targetSpeed, 0.1f);
			_direction = Vector3.Lerp(_direction, _targetDirection, 0.1f);
			transform.Translate(_targetDirection * _speed * Time.fixedDeltaTime, Space.World);
		}
	}
}
