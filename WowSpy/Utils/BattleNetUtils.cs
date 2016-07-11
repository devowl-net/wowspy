using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using WowSpy.Serialization;

namespace WowSpy.Utils
{
    public class BattleNetUtils
    {
        private const string PageNotFound = "<h3>Страница не найдена</h3>";
        public static bool TryUpdateGuildPlayers(string guildName, string serverName, ref Guild guild, out string error)
        {
            error = string.Empty;
            try
            {
                guild = guild ?? new Guild { GuildName = guildName, ServerName = serverName };
                guild.Players = guild.Players ?? new List<Player>();
                const string url = "http://eu.battle.net/wow/ru/guild/{0}/{1}/roster";
                string resultUrl = string.Format(url, serverName, guildName);
                string pageSource;
                if (TryReadPage(resultUrl, out pageSource))
                {
                    HtmlDocument htmlDoc = new HtmlDocument
                    {
                        OptionFixNestedTags = true
                    };

                    htmlDoc.LoadHtml(pageSource);

                    if (htmlDoc.DocumentNode != null)
                    {
                        if (htmlDoc.DocumentNode.InnerHtml.Contains(PageNotFound))
                        {
                            error = "Данной гильдии не существует";
                            return false;
                        }

                        HtmlNodeCollection pageCountNode =
                            htmlDoc.DocumentNode.SelectNodes(
                                "//ul[@class='ui-pagination']//li//a//span");

                        HashSet<int> hashSetPages = new HashSet<int>();
                        if (pageCountNode != null)
                        {
                            foreach (var pageNode in pageCountNode)
                            {
                                int buf;
                                if (int.TryParse(pageNode.InnerText, out buf))
                                {
                                    hashSetPages.Add(buf);
                                }
                            }
                        }

                        // page=1
                        ParsePage(pageSource, guild);
                        if (hashSetPages.Count > 1)
                        {
                            foreach (var pageId in hashSetPages.OrderBy(val => val).Skip(1))
                            {
                                var pageUrl = string.Format("{0}?page={1}", resultUrl, pageId);
                                if (TryReadPage(pageUrl, out pageSource))
                                {
                                    //page 2,3...
                                    ParsePage(pageSource, guild);
                                }
                            }
                        }
                        guild.LastUpdateTime = DateTime.Now;
                    }
                }
                else
                {
                    error = string.Format("Гильдия {0}-{1} не найдена", guildName, serverName);
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

            return true;
        }

        private static void ParsePage(string pageSource, Guild guild)
        {
            HtmlDocument htmlDoc = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            htmlDoc.LoadHtml(pageSource);

            if (htmlDoc.DocumentNode != null)
            {
                // Получаем список игроков на странице
                HtmlNodeCollection playerNodes =
                    htmlDoc.DocumentNode.SelectNodes(
                        "//tr//td[@class='name']//strong//a");

                foreach (var node in playerNodes)
                {
                    var playerInGuild =guild.Players.FirstOrDefault(
                        p => string.Equals(p.PlayerName, node.InnerHtml, StringComparison.OrdinalIgnoreCase));
                    if (playerInGuild == null)
                    {
                        var player = new Player
                        {
                            GuildName = guild.GuildName, 
                            ServerName = guild.ServerName,
                            PlayerName = node.InnerHtml,
                        };

                        guild.Players.Add(player);
                        TryUpdatePlayerInfo(player);
                    }
                }
            }
        }

        public static bool TryUpdatePlayerInfo(Player player)
        {
            const string url = "http://eu.battle.net/wow/ru/character/{0}/{1}/pet";
            string resultUrl = string.Format(url, player.ServerName, player.PlayerName);
            string pageSource;

            return TryReadPage(resultUrl, out pageSource) && InternalProcessPage(player, pageSource);
        }

        private static bool InternalProcessPage(Player player, string pageSource)
        {
            player.Pets = player.Pets ?? new List<Pet>();
            player.Pets.Clear();
            /*
             *   <span xmlns="http://www.w3.org/1999/xhtml" class="name color-q3">
             *       <span class="level">25</span>
             *       Щенок гончей Недр
             *   </span>
             */
            HtmlDocument htmlDoc = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            // filePath is a path to a file containing the html
            htmlDoc.LoadHtml(pageSource);

            if (htmlDoc.DocumentNode != null)
            {
                HtmlNodeCollection levelNodes =
                    htmlDoc.DocumentNode.SelectNodes("//div[@class='grid-item is-collected']//a[@class='preview']//span[@class='info']");

                if (levelNodes == null)
                {
                    // http://eu.battle.net/wow/en/character/deathguard/Горящийпуть/pet
                    // Петов у человека нету
                    return false;
                }

                
                foreach (HtmlNode node in levelNodes)
                {
                    // <span class="name color-q3"><span class="level">25</span>Предвестник пламени</span>
                    Regex regex = new Regex("(?<=<span class=\"level\">)(.*)(?=<\\/span>)");
                    var levelPetName = regex.Match(node.InnerHtml).Value;
                    var splits = levelPetName.Split(new string[] { "</span>" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length != 2)
                    {
                        // "Гильдейский паж" не будет записан
                        Debug.WriteLine(levelPetName);
                        continue;
                    }

                    int level = int.Parse(splits[0]);
                    string petName = splits[1];
                    

                    var existingPet = player.Pets.FirstOrDefault(pet => pet.Name == petName);
                    if (existingPet != null)
                    {
                        // У дубликатов берётся максимальный уровень
                        existingPet.Level = Math.Max(level, existingPet.Level);
                    }
                    else
                    {
                        player.Pets.Add(new Pet {Level = level, Name = petName});
                    }
                }

                player.TotalPetsCount = levelNodes.Count;
            }

            return true;
        }

        private static bool TryReadPage(string uri, out string page)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    using (Stream data = client.OpenRead(uri))
                    {
                        if (data != null)
                        {
                            using (StreamReader reader = new StreamReader(data))
                            {
                                page = reader.ReadToEnd();
                                return true;
                            }
                        }
                    }
                }
            }
            catch { }

            page = string.Empty;
            return false;
        }
    }
}
