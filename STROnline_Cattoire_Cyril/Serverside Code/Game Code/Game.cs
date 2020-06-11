using System;
using System.Collections.Generic;
using PlayerIO.GameLibrary;

namespace WorldWarGame {
	public class Player : BasePlayer {
		public int colorId;
	}

	public class PositionTeam
	{
		public float x;
		public float y;
		public float z;
	}


	[RoomType("TheWorldWarGame")]
	public class GameCode : Game<Player> {

		private Dictionary<int, PositionTeam> SpawnTeam = new Dictionary<int, PositionTeam>
		{
			{0, new PositionTeam{x=400, y = 0, z =400}},
			{1, new PositionTeam{x=600, y = 0, z =600}},
		};

		private int _currentNumberPlayers;
		private int _currentColorId;
		private int _currentBuildId;

		// This method is called when an instance of your the game is created
		public override void GameStarted() {
			// anything you write to the Console will show up in the 
			// output window of the development server
			Console.WriteLine("Game is started: " + RoomId);
		}

		//private void resetgame() {
			
		//}

		// This method is called when the last player leaves the room, and it's closed down.
		public override void GameClosed() {
			Console.WriteLine("RoomId: " + RoomId);
		}

		// This method is called whenever a player joins the game
		public override void UserJoined(Player player) {

			player.colorId = _currentColorId++;
			_currentNumberPlayers++;

			Console.WriteLine ("ça passe par là");

			foreach (Player pl in Players) {

				if(pl.ConnectUserId != player.ConnectUserId) {
					player.Send("PlayerJoined", pl.ConnectUserId, pl.colorId);
				}
				
				pl.Send("PlayerJoined", player.ConnectUserId, player.colorId);
			}

			if (_currentNumberPlayers == 2)
			{
				foreach (Player pl in Players)
				{
					int _currentColorIdPlayer = pl.colorId;
					_currentBuildId++;

					foreach (Player p in Players)
					{
						Console.WriteLine (_currentBuildId);
						p.Send ("CreateBuild", _currentBuildId, SpawnTeam[_currentColorIdPlayer].x, SpawnTeam[_currentColorIdPlayer].y, SpawnTeam[_currentColorIdPlayer].z, _currentColorIdPlayer);
					}
				}
				foreach (Player pl in Players)
				{
					pl.Send ("StartGame");
				}
			}
		}


		// This method is called when a player leaves the game
		public override void UserLeft(Player player) {
			Broadcast ("PlayerLeft", player.ConnectUserId);
			_currentNumberPlayers--;
		}


		// This method is called when a player sends a message into the server code
		public override void GotMessage(Player player, Message message) {
			switch(message.Type) {


				case "CreateUnit":
					var Team = message.GetInt (0);
					break;

				case "MoveUnity":
					var UnityId = message.GetInt (0);
					var PointX = message.GetFloat (1);
					var PointY = message.GetFloat (2);
					var PointZ = message.GetFloat (3);
					var State = message.GetInt (4);
					Broadcast ("MoveUnity", UnityId, PointX, PointY, PointZ, State);
					break;

				// called when a player clicks on the ground
				case "Move":
					//player.posx = message.GetFloat(0);
					//player.posz = message.GetFloat(1);
					//Broadcast("Move", player.ConnectUserId, player.posx, player.posz);
					break;
				case "MoveHarvest":
					// called when a player clicks on a harvesting node
					// sends back a harvesting command to the player, a move command to everyone else
					//player.posx = message.GetFloat(0);
					//player.posz = message.GetFloat(1);
					//foreach(Player pl in Players) {
					//	if(pl.ConnectUserId != player.ConnectUserId) {
					//		pl.Send("Move", player.ConnectUserId, player.posx, player.posz);
					//	}
					//}
					//player.Send("Harvest", player.ConnectUserId, player.posx, player.posz);
					break;
			}
		}
	}
}