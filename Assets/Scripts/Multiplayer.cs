﻿using System;
using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;

public class Multiplayer : MonoBehaviour {

	private Client client;
	private Player localPlayer;
    private Player remotePlayer;

	public bool isOnline;
	public bool isLocalTurn{ get; private set; }

	public GameObject localPlayerObject;
	public GameObject remotePlayerObject;
    PlayerAbilities localPlayerAbilities;
    PlayerAbilities remotePlayerAbilities;
    public GameObject turnControllerObject;
    public GameObject tileObject;
    TilePlacements tilePlacements;
    ButtonSelection buttonSelection;
    TurnControl turnControlScript;
    public GameObject cardsp1;
    public GameObject cardsp2;
    public bool gameEnded;
     CardHandler CH1;
     CardHandler CH2;

    public int[] p1cards;
    public int[] p2cards;

    public bool isTested;
    public bool isPlayer1;

    private Stack<object> messageQueue;

	// use this for initialization
	void Start()
	{
        Application.runInBackground = true;
		if(isOnline)
			StartOnlineGame();

		messageQueue = new Stack<object>();
        tilePlacements = tileObject.GetComponent<TilePlacements>();
        buttonSelection = tileObject.GetComponent<ButtonSelection>();
        localPlayerAbilities = localPlayerObject.GetComponent<PlayerAbilities>();
        remotePlayerAbilities = remotePlayerObject.GetComponent<PlayerAbilities>();
        turnControlScript = turnControllerObject.GetComponent<TurnControl>();
        CH1 = cardsp1.GetComponent<CardHandler>();
        CH2 = cardsp2.GetComponent<CardHandler>();

        p1cards = new int[3];
        p2cards = new int[3];
    }

	void Stop() // TODO the client must be disconnected when the scene is exited
	{
		if (isOnline) {
			client.Disconnect();
		}
	}

	public void StartOnlineGame()
	{
		
		System.Random random = new System.Random (); // TODO
		String username = SystemInfo.deviceUniqueIdentifier + random.Next (9999);
        Debug.Log(username);
        localPlayer = new Player();
        localPlayer.username = username;
		client = new Client ("172.20.146.21", username: username); // this method has an optional 'token'
		client.Connect();
		client.StartGame(OnGameUpdate);
	}

	public void MovePiece(int button) // call this when local player moves their piece
	{
		Debug.Log("Send Move");
		var msg = new Player();
		msg = localPlayer;
		msg.position = button;
		client.Move(msg);
	}

	private void OnGameUpdate(object message) // client callbacks this function when a message is received
	{
		messageQueue.Push(message);
	}

	private void UpdateGame(object message) // the 'global state' of the 'game' is maintained by this function when isOnline, its called by Update()
	{
		var @switch = new Dictionary<Type, Action> {
			{ typeof(Move), () => {
					Debug.Log("A Move message was received");
					var move = (Move)message;
                    if (move.player.profileId == localPlayer.profileId) {  // this  move is a response to a local move that the loca player just made
                        ChangeTile(move);
                        isLocalTurn = false;
                    } else { // remote player made a move
						remotePlayerAbilities.MoveButton(move.player.position, remote: true);
                        ChangeTile(move, enemy: true);
                        isLocalTurn = true;
                    }
                    Debug.Log(move.player.position + " " + localPlayer.position);
                    if(move.player.position == localPlayer.position && !isTested)
                    {
                        //this is player1
                        isPlayer1 = true;
                    }
                    isTested = true;
                                        
					// TODO: animations by server
				} },
            { typeof(TurnChange), () => {
                    var turnChange = (TurnChange)message;
                    Debug.Log("A TurnChange message was received " + turnChange.turn);
                    //check game status
                    if(turnChange.turn == GameStatus.Ended)
                    { // game has ended
                        Debug.Log("game has been ended");
                        localPlayerObject.GetComponent<HealthController>().checkHealth(0);
                        remotePlayerObject.GetComponent<HealthController>().checkHealth(0);
                        gameEnded = true;
                        isLocalTurn = false;
                    }
                    else if (turnChange.turn == GameStatus.YourTurn) {  // your turn
                        Debug.Log("localturn");
						isLocalTurn = true;
                        turnControlScript.ChangeTurn();
                    } else { // remote player's turn
                        Debug.Log("remoteturn");
						isLocalTurn = false;
                        turnControlScript.ChangeTurn();
                    }
                } },
            { typeof(SendCards), () => {
                    var sendCards = (SendCards)message;
                    Debug.Log("A SendCards message was received");
                    //check game status
                    if(!gameEnded)
                    {
                        Debug.Log("sending cards to cardholders");
                        ChangeCards(sendCards.types);
                        if(!CH1.HasCards()) CH1.DrawCards(p1cards);
                        if(!CH2.HasCards()) CH2.DrawCards(p2cards);
                    }
                } },
            { typeof(GameInit), () => {
					var gameInit = (GameInit)message;
                    Debug.Log("A GameInit message was received " +  gameInit.gameStatus);
                    if (gameInit.gameStatus == GameStatus.YourTurn) { // TODO init turncontroller
						isLocalTurn = true;
						// local player gets to start so turn controller is set correctly
					} else {
						isLocalTurn = false;
						// remote player starts Warning: If game init is sent more than once during a game this will cause a race condition
						turnControlScript.ChangeTurn();
                    }

					localPlayer = gameInit.localPlayer;
					remotePlayer = gameInit.remotePlayer;

                    TileArray(gameInit); // create gameboard tiles

					// TODO: init pieces with correct skins

				} }
		};

		@switch[message.GetType()]();
	}

    void TileArray(GameInit gameInit)
    {
        for (int i = 0; i < 9; i++)
        {
            switch (gameInit.tiles[i].type)
            {
                case MessageTileType.attack:
                    buttonSelection.tiles[i].type = tilePlacements.GetEffect(0, gameInit.tiles[i].strength);
                    buttonSelection.tiles[i].position = buttonSelection.tiles[i].gameObject.transform.position;
                    tilePlacements.CreateTile(buttonSelection.tiles[i], i);
                    continue;
                case MessageTileType.heal:
                    buttonSelection.tiles[i].type = tilePlacements.GetEffect(1, gameInit.tiles[i].strength);
                    buttonSelection.tiles[i].position = buttonSelection.tiles[i].gameObject.transform.position;
                    tilePlacements.CreateTile(buttonSelection.tiles[i], i);
                    continue;
                case MessageTileType.poison:
                    buttonSelection.tiles[i].type = tilePlacements.GetEffect(2, gameInit.tiles[i].strength);
                    buttonSelection.tiles[i].position = buttonSelection.tiles[i].gameObject.transform.position;
                    tilePlacements.CreateTile(buttonSelection.tiles[i], i);
                    continue;
            }
        }
    }

    int TileType(MessageTileType type)
    {
        switch (type)
        {
            case MessageTileType.attack:
                return 0;
            case MessageTileType.heal:
                return 1;
            case MessageTileType.poison:
                return 2;
        }
        Debug.Log("Wrong tile type, setting tile to attack type instead");
        return 0;
    }

    void ChangeTile(Move move, bool enemy = false)
    {
        int type = TileType(move.newTile.type);
        if (!enemy) localPlayerAbilities.ChangeTile(type, move.newTile.strength);
        else remotePlayerAbilities.ChangeTile(type, move.newTile.strength);
    }

    void ChangeCards(int[,] cards)
    {
        for (int i = 0; i < 3; i++)
        {
            p1cards[i] = cards[0, i];
            p2cards[i] = cards[1, i];
        }
    }

    public void UseCard(int type, int value)
    {
        Debug.Log("using card");
        client.UseCard(type, value);
    }

    // Update is called once per frame
    void Update()
	{
		try {
			UpdateGame(messageQueue.Pop());
		} catch {
			// InvalidOperationException: stack is empty
		}
	}
}
