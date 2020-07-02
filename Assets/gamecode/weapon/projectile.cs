using System;
using CryEngine;

namespace CryEngine.Game
{
	public struct ProjectileData
	{
		public Primitives Shape { get; set; }
		public string MaterialUrl { get; set; }
		public float Scale { get; set; }
		public float Speed { get; set; }
		public float Mass { get; set; }
		public float ProjectileInitDistance { get; set; }
	}

	[EntityComponent(Guid="955f1ef4-3ce9-ea6a-b58f-f58fff923aab")]
	public class Projectile : EntityComponent
	{
		/// <summary>
		/// The lifetime of a bullet. After this amount of time has passed the bullet will be removed.
		/// </summary>
		private const float MaxLifetime = 15.0f;

		/// <summary>
		/// The time this bullet has been alive. If it reaches MaxLifetime this bullet will be removed.
		/// </summary>
		private float _lifetime = 0;

		public void Initialize(ProjectileData projectileData)
		{
			Entity.LoadGeometry(0, projectileData.Shape);
			if (!string.IsNullOrWhiteSpace(projectileData.MaterialUrl))
			{
				Entity.LoadMaterial(projectileData.MaterialUrl);
			}

			var physics = Entity.Physics;
			var physicsParameters = new RigidPhysicalizeParams();
			physicsParameters.Mass = projectileData.Mass;

			physics.Physicalize(physicsParameters);

			Entity.SetViewDistanceRatio(1.0f);

			Vector3 _shotdirection = Entity.WorldRotation.Forward;
			physics.AddImpulse(_shotdirection * projectileData.Speed);
		}

		/// <summary>
		/// Called once every frame when the game is running.
		/// </summary>
		/// <param name="frameTime">The time difference between this and the previous frame.</param>
		protected override void OnUpdate(float frameTime)
		{
			base.OnUpdate(frameTime);

			_lifetime += frameTime;
			if (_lifetime > MaxLifetime)
			{
				Entity.Remove();
			}
		}

		/// <summary>
		/// Event used when bullet hits something
		/// </summary>
		/// <param name="collisionEvent"></param>
		protected override void OnCollision(EntitySystem.CollisionEvent collisionEvent)
		{
			base.OnCollision(collisionEvent);

			//Remove the entity when it hits something.
			//Entity.Remove();
		}
	}
}