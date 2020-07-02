using System;
using CryEngine.Rendering;

namespace CryEngine.Game
{
	public class PlayerView
	{
		private readonly Player _player;
		private readonly Entity _cameraPivot;
		private ViewCamera _camera;

		//3D cursor
		public Entity _debugRaycastPoint;

		public PlayerView(Player player)
		{
			_player = player ?? throw new ArgumentNullException(nameof(player));
			_cameraPivot = Entity.Spawn("CameraPivot", Vector3.Zero, Quaternion.Identity, Vector3.One);
			_camera = Entity.SpawnWithComponent<ViewCamera>("PlayerCamera", Vector3.Zero, Quaternion.Identity, Vector3.One);

			// raycasated cursor
			_debugRaycastPoint = Entity.Spawn("RaycastPoint", _player.Entity.Position, _player.Entity.Rotation, Vector3.One * 0.12f);
			_debugRaycastPoint.LoadGeometry(0, Primitives.Sphere);
			_debugRaycastPoint.LoadMaterial("materials/user_defaultmat");
			_debugRaycastPoint.SetViewDistanceRatio(1.0f); // always render regardless of the distance
		}

		public Quaternion UpdateView(float frameTime, Vector2 rotationDelta)
		{
			_cameraPivot.Position = _player.Entity.Position;
			Quaternion rotation = _cameraPivot.Rotation;
			float yawSpeed = _player.RotationSpeedYaw;
			float pitchSpeed = _player.RotationSpeedPitch;
			float pitchMin = _player.RotationLimitsMinPitch;
			float pitchMax = _player.RotationLimitsMaxPitch;
			float eyeHeight = _player.EyeHeight;

			//Invert the rotation to have proper third-person camera-control.
			rotationDelta = -rotationDelta;

			var ypr = rotation.YawPitchRoll;

			ypr.X += rotationDelta.X * yawSpeed;

			float pitchDelta = rotationDelta.Y * pitchSpeed;
			ypr.Y = MathHelpers.Clamp(ypr.Y + pitchDelta, pitchMin, pitchMax);

			ypr.Z = 0;

			rotation.YawPitchRoll = ypr;
			_cameraPivot.Rotation = rotation;
			

			var entity = _cameraPivot;
			var forward = entity.Forward;
			
			//By default the camera will look at the player's feet, so we set the focus point a bit higher.
			var position = entity.WorldPosition + Vector3.Up * eyeHeight;

			_camera.Entity.Position = position;
			_camera.Entity.Rotation = rotation;
			return rotation;
		}

		/// <summary>
		/// Use raycasting to get screen center point and use it as bullet spawn position
		/// </summary>
		/// <param name="projectileInitDistance">The distance to use raycast shooting when target is under this value, then bullet will act as projectile over the specified value</param>
		/// <param name="projectileInitPosition">Used to save projectile spawn position center point of the screen</param>
		/// <param name="lookDirection">Use to save projectile spawn direction based on the camera direction</param>
		public void RaycastView(float projectileInitDistance, out Vector3 projectileInitPosition, out Quaternion lookDirection)
		{
			float maxDistance = 150.0f;
			float minDistance = projectileInitDistance;

			int screenX = Renderer.ScreenWidth / 2;
			int screenY = Renderer.ScreenHeight / 2;

			Camera.ScreenPointToWorldPoint(screenX, screenY, maxDistance, out Vector3 origin);
			Camera.ScreenPointToDirection(screenX, screenY, out Vector3 direction);

			Vector3 forward = _player.Entity.Forward;
			forward = Camera.TransformDirection(forward);

			lookDirection = _camera.Entity.Rotation;

			//raycasting will always start exactly from camera position,
			//but bullet spawn may need to spawn further away at minDistance
			projectileInitPosition = origin + forward * minDistance;

			//raycasting under the maxDistance value, ignoring the Player entity
			if (Physics.Raycast(origin, direction, maxDistance, EntityRaycastFlags.All, out RaycastHit hit, _player.Entity.Physics))
			{
				//keep the cursor always at the raycast point
				_debugRaycastPoint.Position = hit.Point;

				//if raycast distance is lower than the minDistance, it will spawn bullet at the raycast point instead
				//this will make the bullet act as raycasted bullet under minDistance,
				//then act as projectile when further than minDistance
				if (hit.Distance < minDistance)
				{
					projectileInitPosition = hit.Point;
				}
			}
			else
			{
				//if raycast point doesn't hit anything or target is further away than maxDistance, put the cursor away.
				_debugRaycastPoint.Position = _player.Entity.Position + Vector3.Down;
			}
		}

		/// <summary>
		/// Does not work at the moment.
		/// </summary>
		/// <param name="fieldOfView">new FOV value</param>
		public void SetFOV(float fieldOfView)
		{
			Camera.FieldOfView = fieldOfView;
		}

		public void Deinitialize()
		{
			if (_cameraPivot != null)
			{
				_cameraPivot.Remove();
			}
		}
	}
}