using HogWarp.Lib;
using HogWarp.Lib.Game;
using HogWarp.Lib.System;
using Buffer = HogWarp.Lib.System.Buffer;

namespace Dueling
{
    public class DuelingPlugin : IPluginBase
    {
        public string Name => "Dueling";
        public string Description => "Dueling plugin";

        private Server? _server;

        public void Initialize(Server server)
        {
            _server = server;
            _server.UpdateEvent += Update;
            _server.ChatEvent += Chat;
            _server.PlayerJoinEvent += PlayerJoin;
            _server.RegisterMessageHandler(Name, HandleMessage);
        }

        public void Update(float deltaSeconds)
        {
        }

        public void Chat(Player player, string message, ref bool cancel)
        {
            if (message.StartsWith("/duel"))
            {
                var split = message.Split("/duel ");
                if (split.Length < 2)
                {
                    player.SendMessage("Missing player name! /duel <duel name>");
                    cancel = true;
                    return;
                }

                foreach (var playerName in _server!.PlayerManager.Players)
                {
                    if (String.Compare(playerName.Name, split[1], StringComparison.OrdinalIgnoreCase) == 0)
                        Console.WriteLine("we found player");
                    else
                    {
                        player.SendMessage("Player not found!");
                    }
                }
            }
        }

        public void PlayerJoin(Player player)
        {
            Serilog.Log.Information("Player joined!");

            SendPing(player, 0);
        }

        public void HandleMessage(Player player, ushort opcode, Buffer buffer)
        {
            var reader = new BufferReader(buffer);

            if (opcode == 43)
            {
                reader.ReadBits(out var ping, 64);

                Serilog.Log.Information($"Ping: {ping}");

                SendPing(player, ping);
            }
        }

        private void SendPing(Player player, ulong id)
        {
            var buffer = new Buffer(1000);
            var writer = new BufferWriter(buffer);
            writer.Write(id);

            _server!.PlayerManager.SendTo(player, Name, 43, writer);
        }
    }
}