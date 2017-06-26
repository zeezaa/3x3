﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using Networking;


namespace Server
{
	public class Connection
	{
		private const int POLL_INTERVAL = 5000;
		private const uint PROTO_VERSION = 0; // Every time you make chance that breaks the interface, you should increment this

		private TcpClient client;
		private BinaryWriter clientWriter;
		private BinaryReader clientReader;

		private Action<object> serverCallback;

		private Stopwatch pollTimer;

		private Parser parser;

		public User user;

		public bool connected;

		public Connection()
		{
		}

		public void Start(object socket, Action<object> callback) 
		{
			pollTimer = new Stopwatch ();
			pollTimer.Start();

			parser = new Parser();

			serverCallback = callback;

			client = (TcpClient)socket;

			clientWriter = new BinaryWriter(client.GetStream());
			clientReader = new BinaryReader(client.GetStream());

			connected = true;

			// create a new worker that will handle the connection
			BackgroundWorker waiter = new BackgroundWorker();
			waiter.DoWork += (object sender, DoWorkEventArgs e) => WaitForRequest();
			waiter.RunWorkerAsync();
		}

		public void Stop()
		{
			user = null;
			client.Close();
		}

		private void WaitForRequest()
		{
			while (connected && client.Connected) {
				parser.RecvObject(clientReader, OnRequest); // this blocks until a request has been received
				PollClient();
			}
			Stop();
		}

		// sends any object
		public void SendObject(object msg)
		{
			connected = parser.SendObject (clientWriter, msg);
		}

		private void OnRequest(object req)
		{
			// every new connection has to be authenticated
			if (user.IsAuthenticated()) {
				serverCallback(req);
			} else {
				OnAuthenticationRequest(req);
			}
		}

		private void OnAuthenticationRequest(object req)
		{
			if (req.Equals(typeof(Message.AuthenticationRequest))) {
				Message.AuthenticationRequest authenticationRequest = (Message.AuthenticationRequest)req;

				if (authenticationRequest.ProtocolVersion != PROTO_VERSION) {
					Log("Protocol version mismatch");
				}

				user = new User (authenticationRequest.username);
				user.Authenticate (authenticationRequest.token);
				if (user.IsAuthenticated ()) {
					Message.AuthenticationResponse res = new Message.AuthenticationResponse ();
					res.status = Message.Status.Ok;
					SendObject(res);
				} else {
					Message.AuthenticationResponse res = new Message.AuthenticationResponse ();
					res.status = Message.Status.Fail;
					SendObject(res);
				}
			} else {
				Message.Status res = Message.Status.Fail;
			}
		}

		// polls the client every POLL_INTERVAL
		private void PollClient()
		{ 
			if (pollTimer.ElapsedMilliseconds > POLL_INTERVAL) {
				pollTimer.Restart();

				Message message = new Message();
				message.Response = Message.Status.Ok;

				// are you still there?
				SendObject(message);
			}
		}

		private void Log(string msg)
		{
			Console.WriteLine(msg);
		}
	}
}