﻿using Lidgren.Network;
using SquareCubed.Network;

namespace SquareCubed.Server.Units
{
	internal class UnitsNetwork
	{
		private readonly ushort _dataPacketType;
		private readonly ushort _physicsPacketType;
		private readonly Server _server;

		public UnitsNetwork(Server server)
		{
			_server = server;
			_physicsPacketType = _server.Network.PacketHandlers.ResolveType("units.physics");
			_dataPacketType = _server.Network.PacketHandlers.ResolveType("units.data");
		}

		public void SendUnitPhysics(Unit unit)
		{
			var msg = _server.Network.Peer.CreateMessage();

			// Add the packet type Id
			msg.Write(_physicsPacketType);
			msg.WritePadBits();

			// Add data
			msg.Write(unit.Id);
			msg.Write(unit.Position.X);
			msg.Write(unit.Position.Y);

			// Send data to appropriate players
			unit.World.SendToAllPlayers(msg, NetDeliveryMethod.UnreliableSequenced, (int) SequenceChannels.UnitPhysics);
		}

		public void SendUnitData(Unit unit)
		{
			var msg = _server.Network.Peer.CreateMessage();

			// Add the packet type Id
			msg.Write(_dataPacketType);
			msg.WritePadBits();

			// The client knows what unit to update using the Id
			msg.Write(unit.Id);

			// Send the data to all players in the world the unit is in
			unit.World.SendToAllPlayers(msg, NetDeliveryMethod.ReliableOrdered, (int) SequenceChannels.UnitData);
		}
	}
}