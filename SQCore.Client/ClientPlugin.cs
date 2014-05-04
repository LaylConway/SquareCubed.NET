﻿using SQCore.Client.Background;
using SQCore.Client.Gui;
using SQCore.Client.Objects;
using SQCore.Client.Tiles;
using SQCore.Common;
using SquareCubed.Client;
using SquareCubed.Client.Structures.Objects;
using SquareCubed.Client.Structures.Tiles;

namespace SQCore.Client
{
	public sealed class ClientPlugin : CommonPlugin, IClientPlugin
	{
		#region Tile Types

		private readonly CorridorTileType _corridorTile;
		private readonly MetalFloorTileType _metalFloorTile;

		#endregion

		#region Object Types

		private readonly PilotSeatObjectType _pilotSeatType;

		#endregion

		#region External Componentes

		private readonly SquareCubed.Client.Client _client;
		private readonly ObjectTypes _objectTypes;
		private readonly TileTypes _tileTypes;

		#endregion

		private readonly ContextInfoPanel _infoPanel;
		private readonly Chat.Chat _chat;
		private readonly Space _stars;

		public ClientPlugin(SquareCubed.Client.Client client)
		{
			Logger.LogInfo("Initializing core plugin...");

			_client = client;
			_tileTypes = _client.Structures.TileTypes;
			_objectTypes = _client.Structures.ObjectTypes;

			_stars = new Space(_client.Graphics.Camera.Resolution, new SpaceRenderer(_client.Graphics.Camera));
			_stars.GenerateStars();

			_chat = new Chat.Chat(_client.Gui, _client.Network);
			_infoPanel = new ContextInfoPanel(_client.Gui);

			// Add tile types
			_corridorTile = new CorridorTileType();
			_tileTypes.RegisterType(_corridorTile, 2);
			_metalFloorTile = new MetalFloorTileType();
			_tileTypes.RegisterType(_metalFloorTile, 3);

			// Add object types
			_pilotSeatType = new PilotSeatObjectType(_client, _infoPanel);
			_objectTypes.RegisterType(_pilotSeatType, 0);

			// Bind events
			_client.BackgroundRenderTick += RenderBackground;

			Logger.LogInfo("Finished initializing core plugin!");
		}

		public void Dispose()
		{
			// Unbind events
			_client.BackgroundRenderTick -= RenderBackground;

			// Clean up the Gui
			_chat.Dispose();
			_infoPanel.Dispose();

			// Remove tile types
			_tileTypes.UnregisterType(_corridorTile);
			_tileTypes.UnregisterType(_metalFloorTile);

			// Remove object types
			_objectTypes.UnregisterType(typeof (PilotSeatObject));
		}

		private void RenderBackground(object sender, TickEventArgs e)
		{
			_stars.Render();
		}
	}
}