// Copyright 2001-2019 Crytek GmbH / Crytek Group. All rights reserved.

using System;

namespace CryEngine.Game
{
	/// <summary>
	/// Basic sample of running a Mono Application.
	/// </summary>
	public class Game : IGameUpdateReceiver, IDisposable
	{
		private static Game _instance;

		private Game()
		{
			// The server doesn't support rendering UI and receiving input, so initializing those system is not required.
			if(Engine.IsDedicatedServer)
			{
				return;
			}

			GameFramework.RegisterForUpdate(this);
		}

		public static void Initialize()
		{
			if(_instance == null)
			{
				_instance = new Game();
			}
		}

		public virtual void OnUpdate()
		{

		}

		public static void Shutdown()
		{
			_instance?.Dispose();
			_instance = null;
		}

		public void Dispose()
		{
			if(Engine.IsDedicatedServer)
			{
				return;
			}
			GameFramework.UnregisterFromUpdate(this);
		}
	}
}
