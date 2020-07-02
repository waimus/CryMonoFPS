// Copyright 2001-2016 Crytek GmbH / Crytek Group. All rights reserved.

namespace CryEngine.Game.Weapons
{
	public class DefaultWeapon : BaseWeapon
	{
		private static readonly ProjectileData _projectileData = new ProjectileData
		{
			//Uses primiteive shapes as bullet geometry, can be modified to use custom mesh.
			Shape = Primitives.Sphere,
			// This material has the 'mat_bullet' surface type applied, which is set up to play sounds on collision with 'mat_concrete' objects in Libs/MaterialEffects
			MaterialUrl = "materials/user_defaultmat",
			Scale = 0.1f,
			Speed = 5000.0f,
			Mass = 1000.0f,
			//Distance of this type of bullet will use raycasting to shoot until bullet act as projectile.
			ProjectileInitDistance = 20.0f
		};

		public override ProjectileData ProjectileData { get => _projectileData; }

		public override string Name { get => "DefaultWeapon"; }

		public override void RequestFire(Vector3 firePosition, Quaternion projectileRotation)
		{
			//Spawn the bullet entity
			var bullet = Entity.SpawnWithComponent<Projectile>("Default Projectile", firePosition, projectileRotation, ProjectileData.Scale);
			//This will set the prepare the bullet and set the initial velocity.
			bullet.Initialize(ProjectileData);
		}
	}
}
