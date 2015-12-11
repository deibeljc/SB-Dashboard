using System;
using System.Runtime.Serialization.Json;
using SmartBot.Plugins.API;
using System.Collections.Generic;
using Fleck;
using System.IO;

namespace SmartBot.Plugins {
    [Serializable]
    class BPluginDataContainer : PluginDataContainer {
        public int SomeGoldAmount { get; set; }

        public BPluginDataContainer() {
            Name = "SmartBotDashboard";
        }
    }

    public class DashBoard : Plugin {
        readonly Server _server = new Server();
        readonly BoardCdm _board = new BoardCdm();
        MemoryStream _stream = new MemoryStream();
        DataContractJsonSerializer _json = new DataContractJsonSerializer(typeof(BoardCdm));

        public override void OnPluginCreated() {
            Init();
        }

        public override void OnStarted() {
        }

        public override void OnVictory() {
            _board.Wins++;
        }

        public override void OnDefeat() {
            _board.Losses++;
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
        }

        private void Init() {
            Bot.Log("[PLUGIN] -> Dashboard: Plugin Created");
            _server.Start();
        }
    }

    public class BoardCdm {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public Card Hero { get; set; }
        public String HeroId { get; set; }
        public Card Enemy { get; set; }
        public String EnemyId { get; set; }
    }

    
    public class Server {
        readonly List<IWebSocketConnection> _allSockets = new List<IWebSocketConnection>();

        public void Start() {
            var server = new WebSocketServer("ws://127.0.0.1:8081");
            server.Start(socket => {
                socket.OnOpen = () => {
                    Bot.Log("[Plugin] -> Dashboard: Client connected...");
                    _allSockets.Add(socket);
                };
                socket.OnClose = () => {
                    _allSockets.Remove(socket);
                    Bot.Log("[Plugin] -> Dashboard: Client disconnected...");
                };
            });

            Bot.Log("[Plugin] -> Dashboard: Server started...");
        }

        public void Send(String message) {
            foreach(var socket in _allSockets) {
                socket.Send(message);
            }
        }
    }
}

