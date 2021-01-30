using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XrPrototypeKit.SixDofControllers;

namespace XrPrototypeKit
{
	/// <summary>
	/// Shoots procedurally generated rigidbody spheres on trigger pull.
	/// </summary>
	public class PingPongGunTool : BaseTool 
	{
		public override Enum Type
		{
			get { return ToolType.PingPongGun; }
		}

		public float _lifetime = 10;
		public float _diameter = 0.04f; // standard is 40mm diameter
		public float _mass = 0.0027f; // standard is 2.7g
		public float _velocity = 10; // m/s
		public int _layer = 1; // e.g. Default

		private Dictionary<GameObject, float> _ballSpawnTimes;
		private List<GameObject> _balls;
		private PhysicMaterial _physicMaterial;

		public void SpawnBall (Vector3 position, Vector3 forward)
		{
			// Create the object and set the layer
			GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			ball.transform.position = position;
			ball.transform.localScale = Vector3.one * _diameter;
			ball.layer = _layer;

			// Create the physic material (if necessary)
			if (_physicMaterial == null)
			{
				_physicMaterial = new PhysicMaterial("PingPongBall");
				_physicMaterial.bounciness = 0.9f;
				_physicMaterial.dynamicFriction = 0.1f;
				_physicMaterial.staticFriction = 0.3f;
			}

			// Set the physic material (we know the object has a collider by default)
			ball.GetComponent<Collider>().sharedMaterial = _physicMaterial;

			// Create the rigidbody and set velocity
			Rigidbody rb = ball.AddComponent<Rigidbody>();
			rb.mass = _mass; // standard weight is 2.7g
			rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
			rb.velocity = forward.normalized * _velocity;

			// Register the object
			_balls.Add(ball);
			_ballSpawnTimes.Add(ball, Time.timeSinceLevelLoad);
		}

		protected override void OnAfterActivate () 
		{
			_ballSpawnTimes = new Dictionary<GameObject, float>();
			_balls = new List<GameObject>();
			SixDofControllerManager.OnInputEvent += HandleInputEvent;
		}

		protected override void OnBeforeDeactivate () 
		{
			SixDofControllerManager.OnInputEvent -= HandleInputEvent;

			if (_balls != null)
			{
				foreach (GameObject ball in _balls)
				{
					Destroy(ball);
				}
				_balls.Clear();
				_ballSpawnTimes.Clear();
			}
		}

		private void HandleInputEvent (InputEvent inputEvent)
		{
			if (inputEvent == InputEvent.HomeTap)
			{
				_contentTools.ActivateDefaultTool();
			}
			else if (inputEvent == InputEvent.TriggerPressed)
			{
				SpawnBall(transform.position, transform.forward);
			}
		}

		protected void Update ()
		{
			int count = _balls.Count;
			for (int i = count- 1; i >= 0; i--)
			{
				GameObject ball = _balls[i];
				if (Time.timeSinceLevelLoad - _ballSpawnTimes[ball] > _lifetime)
				{
					_balls.RemoveAt(i);
					_ballSpawnTimes.Remove(ball);
					Destroy(ball);
				}
			}
		}
	}
}
