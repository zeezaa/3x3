﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneControllerScript : MonoBehaviour { //might need renaming, this and SceneChanges
	//permanent object = keeps the Object SceneController in all scenes - (TokenControl - SceneChange - (FindToken))

	//to move the player tokens to next scene:
	//token choosing script
	TokenControl tokenControl; 

	//player1 - new, works
	public GameObject currentToken;

	//player2 - new, writing
	public GameObject randomToken;

	//keep this object
	void Awake () {
		DontDestroyOnLoad(gameObject);

	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
