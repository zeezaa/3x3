﻿using System;
using System.Collections.Generic;

namespace Networking
{
	public class Message // never send a Message using SendObject
	{
		public Status status;

		public AuthenticationRequest authenticationRequest;
		public AuthenticationResponse authenticationResponse;

		public OnMove onMove;
	}

	// types used in messages, do not send these

	public class Position
	{
		public uint x;
		public uint y;
	}

	public class Player
	{
		public string profileId;
		public Position position;
	}

	public class Inventory
	{
		public string selectedSkin;
	}

	public class Profile
	{
		public string profileId;
		public string username;
		public Inventory inventory;
	}

	public enum Status { Ok, Fail };
	public enum GameStatus { None, Waiting, YourTurn, RemoteTurn, Ended };

	// message types, use these to send messages
	public class OnMove
	{
		public GameStatus gameStatus;
		public Player localPlayer;
		public Player remotePlayer;
		public string playTileAnimation;
	}

	public class AuthenticationRequest
	{
		public string username;
		public string token;
		public uint protocolVersion;
	}

	public class AuthenticationResponse
	{
		public Status status;
	}

}
