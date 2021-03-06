﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Lidgren.Network;
using OpenTK;
using SquareCubed.Common.Utils;
using SquareCubed.Network;

namespace SquareCubed.Server.Players
{
	public class Players
	{
		private readonly Logger _logger = new Logger("Players");
		private readonly Dictionary<NetConnection, string> _names = new Dictionary<NetConnection, string>();
		private readonly PlayersNetwork _network;
		private readonly Dictionary<NetConnection, Player> _players = new Dictionary<NetConnection, Player>();
		private readonly Random _random = new Random();
		private readonly Server _server;
		private readonly List<ISpawnProvider> _spawnProviders = new List<ISpawnProvider>();

		public Players(Server server)
		{
			Debug.Assert(server != null);

			_server = server;
			_network = new PlayersNetwork(_server.Network, this);

			_server.Meta.ClientDataReceived += OnClientDataReceived;
			_server.Network.LostConnection += OnLostConnection;
			_server.Network.ApprovalRequested += OnApprovalRequested;
		}

		public Player this[NetConnection key]
		{
			get { return _players[key]; }
		}

		/// <summary>
		///     Adds a new spawn provider. Spawn providers provide points
		///     for players to spawn at. A spawn provider might give the
		///     player a starting home in a station, or a starting ship.
		///     A spawn provider is randomly picked from the list when a
		///     new player connects to the server.
		/// </summary>
		/// <param name="provider">The provider.</param>
		public void AddSpawnProvider(ISpawnProvider provider)
		{
			_spawnProviders.Add(provider);
		}

		private void OnClientDataReceived(object sender, NetConnection con)
		{
			// TODO: This entire function is filled with hidden traps in which order stuff has to be done, do something about that
			// For example, if you send the unit data before doing _server.Units.Add(unit),
			// the client will somehow attach the unit to the wrong structure. This is probably
			// because of the weird way the units are set up to join worlds and structures.
			// We need to make sure they only join world and structure lists if they're in the
			// master list, to avoid them being linked without us wanting to.

			// Make a random spawn provider provide us with a spawn
			var spawn = _spawnProviders[_random.Next(0, _spawnProviders.Count - 1)].GetNewSpawn();

			// Create the Player and the Player Unit we'll need
			var unit = new PlayerUnit(_server.Units)
			{
				World = spawn.Structure.World,
				Structure = spawn.Structure,
				Position = spawn.Position
			};
			var name = _names[con];
			_names.Remove(con);

			var player = new Player(con, name, unit);

			// Make sure the player knows the existing structures
			_server.Structures.SendStructureDataFor(player);

			// Add the Player and the Player Unit to their collections and send the data
			_server.Units.Add(unit);
			_players.Add(con, player);
			_network.SendPlayerData(player);

			// Make sure the player knows the existing units
			_server.Units.SendUnitDataFor(player);

			// And log it
			_logger.LogInfo("New player \"{0}\" added!", name);
		}

		public void OnPlayerPhysics(NetConnection con, Vector2 position)
		{
			var player = _players[con];
			//if (!player.Unit.TeleportLocked)
			player.Unit.Position = position;
		}

		private void OnLostConnection(object sender, NetIncomingMessage msg)
		{
			// If it wasn't even a player, don't bother doing anything
			if (!_players.ContainsKey(msg.SenderConnection)) return;

			// Clean up Player Data
			var player = _players[msg.SenderConnection];
			_server.Units.Remove(player.Unit);

			// Remove and Log
			_players.Remove(msg.SenderConnection);
			_logger.LogInfo("Player \"{0}\" removed!", player.Name);
		}

		private void OnApprovalRequested(object sender, ConnectApprovalEventArgs args)
		{
			// Remove double spaces and begin/end spaces
			var name = Regex.Replace(args.Name, " {2,}", " ").Trim(' ');

			// Check if the name is already in use
			if (_players.Any(p => string.Equals(p.Value.Name, name, StringComparison.CurrentCultureIgnoreCase)) ||
			    _names.Any(n => string.Equals(n.Value, name, StringComparison.CurrentCultureIgnoreCase)))
			{
				args.Deny = true;
				return;
			}

			// Add the actual name
			_names.Add(args.Connection, name);
		}
	}
}