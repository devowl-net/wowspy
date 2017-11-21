using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowSpy.Utils
{
    public static class Extentions
    {
        private static readonly Dictionary<string, string> RuEuMapping = new Dictionary<string, string>()
        {
            { "Гордунни",           "Gordunni" },
            { "Гром",               "Grom" },
            { "Корольлич",          "Lich King" },
            { "Пиратскаябухта",     "Booty Bay" },
            { "Подземье",           "Deephome" },
            { "Разувий",            "Razuvious" },
            { "Ревущийфьорд",       "Howling Fjord" },
            { "СвежевательДуш",     "Soulflayer" },
            { "Седогрив",           "Greymane" },
            { "СтражСмерти",        "Deathguard" },
            { "Термоштепсель",      "Thermaplugg" },
            { "ТкачСмерти",         "Deathweaver" },
            { "ЧерныйШрам",         "Blackscar" },
            { "Ясеневыйлес",        "Ashenvale" },
            { "Азурегос",           "Azuregos" },
            { "Борейскаятундра",    "Borean Tundra" },
            { "ВечнаяПесня",        "Eversong" },
            { "Дракономор",         "Fordragon" },
            { "Галакронд",          "Galakrond" },
            { "Голдринн",           "Goldrinn" },
        };

        public static string RussianToEnglishRealm(string realmName)
        {
            return RuEuMapping[new string(realmName.Where(char.IsLetter).ToArray())];
        }

        //public static void AddOrUpdate(this Dictionary<PlayerObj, List<KeyValuePair<string, IEnumerable<PlayerObj>>>> dictionary,
        //    PlayerObj key, IEnumerable<KeyValuePair<string, IEnumerable<PlayerObj>>> value)
        //{
        //    var itemKeys = dictionary.Where(
        //        item =>
        //            string.Equals(item.Key.PlayerName, key.PlayerName, StringComparison.OrdinalIgnoreCase) &&
        //            string.Equals(item.Key.ServerName, key.ServerName, StringComparison.OrdinalIgnoreCase));

        //    if (itemKeys.Any())
        //    {
        //        var item = itemKeys.First();
        //        dictionary[item.Key] = dictionary[item.Key].Union(value).ToList();
        //    }
        //    else
        //    {
        //        dictionary[key] = value.ToList();
        //    }
        //}

        //public const string EmptyGuildName = "NoName";

        //public static string GetGuild(this PlayerObj player)
        //{
        //    return player.GuildName == null ? EmptyGuildName : string.Concat(player.GuildName, "-", player.ServerName);
        //}
    }
}
