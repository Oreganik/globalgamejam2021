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

		public static float MinHeight = 0.4f;
		public static float AccelerationTime = 2;
		public static float DecelerationTime = 1;
		public static float FlySpeedNormal = 5;
		public static float LandSpeed = 2;
		public static float LiftSpeed = 4;
		public static float FlySpeedBoost = 15;
		public static float LiftSpeedBoost = 8;
		public static float LandSpeedBoost = 4;

		private float _acceleration;
		private float _speed;
		private float _targetSpeed;
		private State _state;
		private Vector3 _direction;
		private Vector3 _targetDirection;

		public void Fly (Vector3 direction, bool boost)
		{
			_state = State.Fly;
			_targetDirection = direction.normalized;
			_targetSpeed = boost ? FlySpeedBoost : FlySpeedNormal;
			_acceleration = _targetSpeed / AccelerationTime;
		}

		public void Hover ()
		{
			// don't re-enter the same state when hovering, as it breaks the acceleration
			if (_state == State.Idle) return;
			_state = State.Idle;
			_targetSpeed = 0;
			_acceleration = -_speed / DecelerationTime;
		}

		public void Land (bool boost)
		{
			_state = State.Land;
			_targetDirection = Vector3.down;
			_targetSpeed = boost ? LandSpeedBoost : LandSpeed;
			_acceleration = _targetSpeed / AccelerationTime;
		}

		public void Lift (bool boost)
		{
			_state = State.Lift;
			_targetDirection = Vector3.up;
			_targetSpeed = boost ? LiftSpeedBoost : LiftSpeed;
			_acceleration = _targetSpeed / AccelerationTime;
		}

		protected void FixedUpdate ()
		{
			if (_state == State.Idle)
			{
				_speed += _acceleration * Time.fixedDeltaTime;
				if (_speed < 0) _speed = 0;
			}
			else
			{
				_speed = Mathf.Clamp(_speed + _acceleration * Time.fixedDeltaTime, 0, _targetSpeed);
			}
			_direction = Vector3.Lerp(_direction, _targetDirection, 0.1f);
			transform.Translate(_targetDirection * _speed * Time.fixedDeltaTime, Space.World);

			if (transform.position.y < MinHeight)
			{
				Vector3 p = transform.position;
				p.y = MinHeight;
				transform.position = p;
			}
		}
	}
}
