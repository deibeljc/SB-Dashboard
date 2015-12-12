using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using Fleck;
using SmartBot.Plugins.API;

namespace SmartBot.Plugins {
    [Serializable]
    class BPluginDataContainer : PluginDataContainer {
    
        public BPluginDataContainer() {
            Name = "SmartBotDashboard";
        }
    }

    public class DashBoard : Plugin {
        // Variables
        readonly Server _server = new Server();
        readonly BoardCdm _board = new BoardCdm();
        MemoryStream _stream = new MemoryStream();
        readonly DataContractJsonSerializer _json = new DataContractJsonSerializer(typeof(BoardCdm));

        public override void OnPluginCreated() {
            Init();
        }

        public override void Dispose() {
            _server.Stop();
            Debug.OnLogReceived -= OnLogging;
        }

        public override void OnVictory() {
            _board.Wins++;
        }

        public override void OnDefeat() {
            _board.Losses++;
        }

        public override void OnStarted() {
            _board.BotStarted = true;
        }

        public override void OnStopped() {
            _board.BotStarted = false;
        }

        private void OnLogging(string message) {
            _board.Log.Add(message);
        }

        public override void OnTick() {
            if (Bot.CurrentBoard != null) {
                _board.Hero = Bot.CurrentBoard.HeroFriend;
                _board.HeroId = Bot.CurrentBoard.HeroFriend.Template.Id.ToString();
                _board.Enemy = Bot.CurrentBoard.HeroEnemy;
                _board.EnemyId = Bot.CurrentBoard.HeroEnemy.Template.Id.ToString();
            }
          
            _stream = new MemoryStream();
            _json.WriteObject(_stream, _board);
            _stream.Position = 0;
            var sr = new StreamReader(_stream);
            var boardStateString = sr.ReadToEnd();
            _server.Send(boardStateString);

            // After send dump the logs since we already sent them.
            _board.Log = new List<string>();
        }

        private void Init() {
            Bot.Log("[PLUGIN] -> Dashboard: Plugin Created");
            // Events
            Debug.OnLogReceived += OnLogging;
            _server.Start();

            // Init the _board objects if needed.
            _board.Log = new List<string>();
            _board.BotStarted = false;
        }
    }

    public class BoardCdm {
        public Boolean BotStarted { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public Card Hero { get; set; }
        public String HeroId { get; set; }
        public Card Enemy { get; set; }
        public String EnemyId { get; set; }
        public List<String> Log { get; set; } 
    }

    
    public class Server {
        readonly List<IWebSocketConnection> _allSockets = new List<IWebSocketConnection>();
        readonly IWebSocketServer _server = new WebSocketServer("ws://127.0.0.1:8081");

        public void Start() {
            _server.Start(socket => {
                socket.OnOpen = () => {
                    Bot.Log("[PLUGIN] -> Dashboard: Client connected...");
                    _allSockets.Add(socket);
                };
                socket.OnClose = () => {
                    _allSockets.Remove(socket);
                    Bot.Log("[PLUGIN] -> Dashboard: Client disconnected...");
                };
                socket.OnMessage = HandleMessage;
            });

            Bot.Log("[PLUGIN] -> Dashboard: Server started...");
        }

        public void Stop() {
            _server.Dispose();
        }

        public void Send(String message) {
            foreach(var socket in _allSockets) {
                socket.Send(message);
            }
        }

        private void HandleMessage(String message) {
            switch (message) {
                case "start":
                    Bot.StartBot();
                    break;
                case "stop":
                    Bot.StopBot();
                    break;
            }
        }
    }
}

