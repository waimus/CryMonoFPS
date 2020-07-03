using System;
using CryEngine.Game.Weapons;

namespace CryEngine.Game
{
	[EntityComponent(Category = "Game", Guid = "cdd2245e-3f20-7713-58fd-997a04794d60")]
	public class Player : EntityComponent
	{
		private const string InputActionMapURL = "Libs/config/defaultprofile.xml";
		private const string InputActionMapName = "player";

		[SerializeValue]
		private float _mass = 90.0f;
		[SerializeValue]
		private float _airResistance = 0.0f;
		[SerializeValue]
		private float _eyeHeight = 0.935f;

		[SerializeValue]
		private ActionHandler _actionHandler;

		private PlayerView _playerView;
		private Vector2 _movement = Vector2.Zero;
		private Vector2 _rotationMovement = Vector2.Zero;
		private Vector3 _projectileInitPosition = Vector3.Zero;
		private Quaternion _lookDirection = Quaternion.Identity;

		public BaseWeapon Weapon { get; set; }

		/// <summary>
		/// Mass of the player entity in kg.
		/// </summary>
		[EntityProperty(EntityPropertyType.Primitive, "Mass of the entity in Kg.")]
		public float Mass
		{
			get => _mass;
			set { _mass = value; Physicalize(); }
		}

		/// <summary>
		/// The air-resistance of the player. Higher air-resistance will make the player float when falling down.
		/// </summary>
		[EntityProperty(EntityPropertyType.Primitive, "Air resistance of the player entity. Higher air-resistance will make the player float when falling down.")]
		public float AirResistance
		{
			get => _airResistance;
			set { _airResistance = value; Physicalize(); }
		}

		/// <summary>
		/// The eye-height of the player.
		/// </summary>
		[EntityProperty(EntityPropertyType.Primitive, "The eye-height of the player.")]
		public float EyeHeight
		{
			get => _eyeHeight;
			set { _eyeHeight = value; Physicalize(); }
		}

		/// <summary>
		/// Strength of the per-frame impulse when holding inputs
		/// </summary>
		/// <value>The move impulse strength.</value>
		[EntityProperty(EntityPropertyType.Primitive, "Speed of the player in meters per second.")]
		public float MoveSpeed { get; set; } = 30.0f;

		/// <summary>
		/// Speed of the player in meters per second when running
		/// </summary>
		/// <value>The sprint impulse strength</value>
		[EntityProperty(EntityPropertyType.Primitive, "Speed of the player in meters per second when running")]
		public float RunSpeed { get; set; } = 50.0f;

		/// <summary>
		/// Force to apply when jumping
		/// </summary>
		/// <value>Jump force strength.</value>
		[EntityProperty(EntityPropertyType.Primitive, "Jump height when jump button is pressed")]
		public float JumpHeight { get; set; } = 4.0f;

		/// <summary>
		/// Speed at which the player rotates entity yaw
		/// </summary>
		/// <value>The rotation speed yaw.</value>
		[EntityProperty(EntityPropertyType.Primitive, "Speed at which the player rotates entity yaw")]
		public float RotationSpeedYaw { get; set; } = 0.002f;

		/// <summary>
		/// Speed at which the player rotates entity pitch
		/// </summary>
		/// <value>The rotation speed pitch.</value>
		[EntityProperty(EntityPropertyType.Primitive, "Speed at which the player rotates entity pitch")]
		public float RotationSpeedPitch { get; set; } = 0.002f;

		/// <summary>
		/// Minimum entity pitch limit
		/// </summary>
		/// <value>The rotation limits minimum pitch.</value>
		[EntityProperty(EntityPropertyType.Primitive, "Minimum entity pitch limit")]
		public float RotationLimitsMinPitch { get; set; } = -1.4f;

		/// <summary>
		/// Maximum entity pitch limit
		/// </summary>
		/// <value>The rotation limits max pitch.</value>
		[EntityProperty(EntityPropertyType.Primitive, "Maximum entity pitch limit")]
		public float RotationLimitsMaxPitch { get; set; } = 1.5f;

		/// <summary>
		/// Normal value of the player active camera Field of View
		/// </summary>
		[EntityProperty(EntityPropertyType.Primitive, "Normal value of the player active camera Field of View")]
		public float CameraFOV { get; set; } = 70.0f;

		/// <summary>
		/// Value of the camera FOV when zoomed in
		/// </summary>
		[EntityProperty(EntityPropertyType.Primitive, "Value of the camera FOV when zoomed in")]
		public float ZoomCameraFOV { get; set; } = 45.0f;

		/// <summary>
		/// Is character sprinting?
		/// </summary>
		/// <value>boolean: true/false</value>
		[EntityProperty(EntityPropertyType.Primitive, "Is character sprinting?")]
		public bool Sprint { get; set; } = false;

		/// <summary>
		/// Is character jumping?
		/// </summary>
		/// <value>boolean: true/false</value>
		[EntityProperty(EntityPropertyType.Primitive, "Is character jumping?")]
		public bool Jump { get; set; } = false;

		protected override void OnInitialize()
		{
			base.OnInitialize();

			PrepareRigidbody();
		}

		/// <summary>
		/// Called at the start of the game.
		/// </summary>
		protected override void OnGameplayStart()
		{
			base.OnGameplayStart();

			_playerView = new PlayerView(this);
			_playerView.SetFOV(CameraFOV);
			Weapon = new DefaultWeapon();

			PrepareRigidbody();
			SetPlayerModel();
			InitializeInput();
		}

		/// <summary>
		/// Called once every frame when the game is running.
		/// </summary>
		/// <param name="frameTime">The time difference between this and the previous frame.</param>
		protected override void OnUpdate(float frameTime)
		{
			base.OnUpdate(frameTime);

			var cameraRotation = _playerView == null ? Camera.Rotation : _playerView.UpdateView(frameTime, _rotationMovement);
			_rotationMovement = Vector2.Zero;

			_playerView.RaycastView(Weapon.ProjectileData.ProjectileInitDistance ,out _projectileInitPosition, out _lookDirection);
			UpdateMovement(frameTime);
		}

		protected override void OnRemove()
		{
			base.OnRemove();
			Entity.FreeGeometrySlot(0);
			_actionHandler?.Dispose();
		}

		protected override void OnEditorGameModeChange(bool enterGame)
		{
			base.OnEditorGameModeChange(enterGame);

			if (!enterGame)
			{
				Entity?.Remove();
				_playerView.Deinitialize();
				_playerView = null;
			}
		}

		private void UpdateMovement(float frameTime)
		{
			var entity = Entity;
			var physicalEntity = entity.Physics;
			if (physicalEntity == null) return;

			var status = physicalEntity.GetStatus<LivingStatus>();
			var movement = new Vector3(_movement);

			//Transform the movement to camera-space
			movement = Camera.TransformDirection(movement);

			//Transforming it could have caused the movement to point up or down, so we flatten the z-axis to remove the height.
			movement.Z = 0.0f;
			movement = movement.Normalized;

			//jumping
			if (Jump && !status.IsFlying)
			{
				//Get current player's move velocity to keep it while jumping
				float x = physicalEntity.Velocity.x;
				float y = physicalEntity.Velocity.y;
				float z = physicalEntity.Velocity.z;

				Vector3 _jumpDirection = new Vector3(x, y, z + 1 * JumpHeight);
				physicalEntity.Jump(_jumpDirection);
				Jump = false;
			}

			// Only dispatch the impulse to physics if one was provided
			if (movement.LengthSquared > 0.0f)
			{
				if (status.IsFlying)
				{
					//If we're not touching the ground we're not going to send any more move actions.
					return;
				}

				if (!Sprint)
				{
					// Multiply by frame time to keep consistent across machines
					movement *= MoveSpeed * frameTime;
				}
				else if (Sprint)
				{
					//When sprint key is activated use value of RunSpeed instead.
					movement *= RunSpeed * frameTime;
				}
				physicalEntity.Move(movement);
			}
		}

		private void InitializeInput()
		{
			_actionHandler?.Dispose();
			_actionHandler = new ActionHandler(InputActionMapURL, InputActionMapName);

			//movement handling
			_actionHandler.AddHandler("moveforward", OnMoveForward);
			_actionHandler.AddHandler("moveback", OnMoveBack);
			_actionHandler.AddHandler("moveright", OnMoveRight);
			_actionHandler.AddHandler("moveleft", OnMoveLeft);

			_actionHandler.AddHandler("sprint", OnActionSprint);
			_actionHandler.AddHandler("jump", OnActionJump);

			//mouse input handling
			_actionHandler.AddHandler("mouse_rotateyaw", OnMoveMouseX);
			_actionHandler.AddHandler("mouse_rotatepitch", OnMoveMouseY);

			//joystrick movement handling
			_actionHandler.AddHandler("stickmovex", OnStickMoveX);
			_actionHandler.AddHandler("stickmovey", OnStickMoveY);

			//mouse action handling
			_actionHandler.AddHandler("mouse1", OnMouse1Pressed);
			_actionHandler.AddHandler("mouse2", OnMouse2Pressed);
		}

		private void OnMoveForward(string name, InputState state, float value)
		{
			if (state == InputState.Pressed)
			{
				_movement.Y = 1.0f;
			}
			else if (state == InputState.Released)
			{
				//The movement only needs to be stopped when the player is still moving forward.
				if (_movement.Y > 0.0f)
				{
					_movement.Y = 0.0f;
				}
			}
		}

		private void OnMoveBack(string name, InputState state, float value)
		{
			if (state == InputState.Pressed)
			{
				_movement.Y = -1.0f;
			}
			else if (state == InputState.Released)
			{
				//The movement only needs to be stopped when the player is still moving back.
				if (_movement.Y < 0.0f)
				{
					_movement.Y = 0.0f;
				}
			}
		}

		private void OnMoveRight(string name, InputState state, float value)
		{
			if (state == InputState.Pressed)
			{
				_movement.X = 1.0f;
			}
			else if (state == InputState.Released)
			{
				//The movement only needs to be stopped when the player is still moving right.
				if (_movement.X > 0.0f)
				{
					_movement.X = 0.0f;
				}
			}
		}

		private void OnMoveLeft(string name, InputState state, float value)
		{
			if (state == InputState.Pressed)
			{
				_movement.X = -1.0f;
			}
			else if (state == InputState.Released)
			{
				//The movement only needs to be stopped when the player is still moving left.
				if (_movement.X < 0.0f)
				{
					_movement.X = 0.0f;
				}
			}
		}

		private void OnActionSprint(string name, InputState state, float value)
		{
			if (state == InputState.Pressed)
			{
				Sprint = true;
			}
			else if (state == InputState.Released)
			{
				Sprint = false;
			}
		}

		private void OnActionJump(string name, InputState state, float value)
		{
			if (state == InputState.Pressed)
			{
				Jump = true;
			}
			else if (state == InputState.Released)
			{
				Jump = false;
			}
		}

		private void OnMoveMouseX(string name, InputState state, float value)
		{
			//If for some reason another state than Changed is received, it will be ignored.
			if (state != InputState.Changed)
			{
				return;
			}

			_rotationMovement.X += value;
		}

		private void OnMoveMouseY(string name, InputState state, float value)
		{
			//If for some reason another state than Changed is received, it will be ignored.
			if (state != InputState.Changed)
			{
				return;
			}

			_rotationMovement.Y += value;
		}

		private void OnMouse1Pressed(string name, InputState state, float value)
		{
			if (state != InputState.Pressed)
			{
				return;
			}

			var weapon = Weapon;
			if (weapon == null)
			{
				return;
			}

			Vector3 position = _projectileInitPosition;
			Quaternion rotation = _lookDirection;
			Weapon?.RequestFire(position, rotation);
		}

		private void OnMouse2Pressed(string name, InputState state, float value)
		{
			//Setting FOV does not work at the moment.
			if (state == InputState.Pressed)
			{
				_playerView.SetFOV(ZoomCameraFOV);
			}
			else if (state == InputState.Released)
			{
				_playerView.SetFOV(CameraFOV);
			}
		}

		private void OnStickMoveX(string name, InputState state, float value)
		{
			//If for some reason another state than Changed is received, it will be ignored.
			if (state != InputState.Changed)
			{
				return;
			}

			_movement.X += value;
		}

		private void OnStickMoveY(string name, InputState state, float value)
		{
			//If for some reason another state than Changed is received, it will be ignored.
			if (state != InputState.Changed)
			{
				return;
			}

			_movement.Y += value;
		}

		/// <summary>
		/// Use this to prepare player model. Can be a skeletal mesh or static geometry.
		/// </summary>
		private void SetPlayerModel()
		{
			//test model
			/*var entity = Entity;

			entity.LoadGeometry(0, Primitives.Sphere);
			entity.LoadMaterial("materials/user_defaultmat");*/
		}

		private void PrepareRigidbody()
		{
			var physicsEntity = Entity.Physics;
			if (physicsEntity == null)
			{
				return;
			}

			// Create the physical representation of the entity
			Physicalize();
		}

		private void Physicalize()
		{
			// Physicalize the player as type Living.
			// This physical entity type is specifically implemented for players
			var parameters = new LivingPhysicalizeParams();

			//The player will have settings for the player dimensions and dynamics.
			var playerDimensions = parameters.PlayerDimensions;
			var playerDynamics = parameters.PlayerDynamics;

			parameters.Mass = Mass;

			// Prefer usage of a capsule instead of a cylinder
			playerDimensions.UseCapsule = true;

			// Specify the size of our capsule
			playerDimensions.ColliderSize = new Vector3(0.45f, 0.45f, EyeHeight * 0.25f);

			// Keep pivot at the player's feet (defined in player geometry)
			playerDimensions.PivotHeight = 0.0f;

			// Offset collider upwards
			playerDimensions.ColliderHeight = 1.0f;
			playerDimensions.GroundContactEpsilon = 0.004f;

			playerDynamics.AirControlCoefficient = 0.0f;
			playerDynamics.AirResistance = AirResistance;
			playerDynamics.Mass = Mass;

			Entity.Physics.Physicalize(parameters);
		}
	}
}
