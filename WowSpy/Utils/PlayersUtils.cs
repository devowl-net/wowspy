using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using WowSpy.Serialization;

namespace WowSpy.Utils
{
    public static class PlayersUtils
    {
        private static void Compare(IEnumerable<PlayerObj> bannedPlayers, PlayerObj checkingPlayer, out List<PlayerObj> matchs)
        {
            matchs = new List<PlayerObj>();
            matchs.AddRange(bannedPlayers.Where(banned => banned.Equals(checkingPlayer)));
        }

        public static Dictionary<PlayerObj, List<KeyValuePair<string, IEnumerable<PlayerObj>>>> Check(
            IEnumerable<PlayerObj> bannedPlayers, 
            IEnumerable<GuildObject> bannedGuilds, 
            IEnumerable<PlayerObj> checkPlayerNames)
        {
            

            // [ПроверяемыйИгрок - Список[ИмяГильдии - Список[Имя персонажа]]]
            var resultPlayerDictionary = new Dictionary<PlayerObj, List<KeyValuePair<string, IEnumerable<PlayerObj>>>>();

            foreach (var player in checkPlayerNames)
            {
                if (player.Pets == null || player.Pets.Count < 5)
                {
                    continue;
                }

                List<PlayerObj> buff;

                // Проверка по забанным игрокам
                PlayersUtils.Compare(bannedPlayers.ToList(), player, out buff);

                KeyValuePair<string, IEnumerable<PlayerObj>> value;

                if (buff.Any())
                {
                    value = new KeyValuePair<string, IEnumerable<PlayerObj>>(
                        player.GetGuild() , buff);

                    resultPlayerDictionary.AddOrUpdate(player, new[] { value });
                }

                // Проверка по всем забаннеым гильдиям
                foreach (var bannedGuild in bannedGuilds)
                {
                    PlayersUtils.Compare(bannedGuild.Players.ToList(), player, out buff);

                    if (buff.Any())
                    {
                        value = new KeyValuePair<string, IEnumerable<PlayerObj>>(bannedGuild.GuildName, buff);
                        resultPlayerDictionary.AddOrUpdate(player, new [] { value });
                    }
                }
            }

            return resultPlayerDictionary;
        }

        public static StringBuilder GetSummary(Dictionary<PlayerObj, List<KeyValuePair<string, IEnumerable<PlayerObj>>>> resultPlayerDictionary)
        {
            StringBuilder outBuilder = new StringBuilder();

            // Перебор спалившихся игроков
            foreach (var playerPair in resultPlayerDictionary)
            {
                foreach (var keyValuePair in playerPair.Value)
                {
                    outBuilder.AppendLine(string.Format("Игрок [{0}-{1}][{2}] состоит в гильдии [{3}] Твинки в ней:",
                        playerPair.Key.PlayerName,
                        playerPair.Key.ServerName,
                        playerPair.Key.GuildName ?? "?",
                        keyValuePair.Key));

                    foreach (var player in keyValuePair.Value)
                    {
                        outBuilder.AppendLine(string.Format("     {0}-{1}", player.PlayerName, player.ServerName));
                    }
                }

                outBuilder.AppendLine();
            }

            return outBuilder;
        }
    }
}
