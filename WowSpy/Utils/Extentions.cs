using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowSpy.Serialization;

namespace WowSpy.Utils
{
    public static class Extentions
    {
        public static void AddOrUpdate(this Dictionary<Player, List<KeyValuePair<string, IEnumerable<Player>>>> dictionary,
            Player key, IEnumerable<KeyValuePair<string, IEnumerable<Player>>> value)
        {
            var itemKeys = dictionary.Where(
                item =>
                    string.Equals(item.Key.PlayerName, key.PlayerName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.Key.ServerName, key.ServerName, StringComparison.OrdinalIgnoreCase));

            if (itemKeys.Any())
            {
                var item = itemKeys.First();
                dictionary[item.Key] = dictionary[item.Key].Union(value).ToList();
            }
            else
            {
                dictionary[key] = value.ToList();
            }
        }

        public const string EmptyGuildName = "NoName";

        public static string GetGuild(this Player player)
        {
            return player.GuildName == null ? EmptyGuildName : string.Concat(player.GuildName, "-", player.ServerName);
        }
    }
}
