// Copyright 2001-2016 Crytek GmbH / Crytek Group. All rights reserved.

namespace CryEngine.Game.Weapons
{
	public abstract class BaseWeapon
	{
		public abstract string Name { get; }

		public abstract ProjectileData ProjectileData { get; }

		public abstract void RequestFire(Vector3 firePosition, Quaternion bulletRotation);
	}
}
