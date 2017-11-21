using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using WowDotNetAPI;
using WowDotNetAPI.Models;
using WowSpy.Utils;

namespace WowSpy
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string BannedGuildsFileName = "BannedGuilds.txt";

        private const string BannedPlayersFileName = "BannedPlayers.txt";

        private const string GuildsForCheckingFileName = "GuildsForChecking.txt";

        private readonly string _apiKey = "kznr78d4pb23p7cm4kxj53nqvgrh8vh3";
        private WowExplorer _explorer;

        public MainWindow()
        {
            Initialize();
            DataContext = this;
            InitializeComponent();
        }

        public ObservableCollection<Guild> BannedGuilds { get; set; }

        public ObservableCollection<Character> BannedPlayers { get; set; }

        public ObservableCollection<Guild> GuildsForChecking { get; set; }

        public List<string> ServerNames { get; set; }

        public Guild SelectedGuildForChecking { get; set; }

        public string SelectedPlayerServerName { get; set; }

        public string SelectedGuildServerName { get; set; }

        public string SelectedGuildCheckingServerName { get; set; }

        private TType GetObjectFromFile<TType>(string path)
        {
            if (File.Exists(path))
            {
                using (var fileStream = new StreamReader(path, Encoding.Unicode))
                {
                    using (var textReader = XmlReader.Create(fileStream))
                    {
                        var serializer = new DataContractSerializer(typeof (TType));
                        return (TType) serializer.ReadObject(textReader);
                    }
                }
            }

            return default(TType);
        }

        private void Initialize()
        {
            _explorer = new WowExplorer(Region.EU, Locale.ru_RU, _apiKey);

            GuildsForChecking =
                new ObservableCollection<Guild>(GetObjectFromFile<List<Guild>>(GuildsForCheckingFileName) ??
                                                new List<Guild>());
            BannedGuilds =
                new ObservableCollection<Guild>(GetObjectFromFile<List<Guild>>(BannedGuildsFileName) ??
                                                new List<Guild>());
            BannedPlayers =
                new ObservableCollection<Character>(GetObjectFromFile<List<Character>>(BannedPlayersFileName) ??
                                                    new List<Character>());

            ServerNames = new List<string>
            {
                "Страж Смерти",
                "Черный Шрам",
                "Свежеватель душ",
                "Ревущий фьорд",
                "Азурегос",
                "Борейская тундра",
                "Вечная Песня",
                "Галакронд",
                "Голдринн",
                "Гордунни",
                "Гром",
                "Термоштепсель",
                "Дракономор",
                "Пиратская бухта",
                "Ткач Смерти",
                "Король-лич",
                "Седогрив",
                "Подземье",
                "Разувий",
                "Ясеневый лес"
            };
        }

        private void AddGuildToBan(object sender, RoutedEventArgs e)
        {
            var bannedGuildName = GuildNameTextBox.Text;

            var bGuild = BannedGuilds.FirstOrDefault(g =>
                string.Equals(g.Name, bannedGuildName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(g.Realm, SelectedGuildServerName, StringComparison.OrdinalIgnoreCase));

            if (bGuild != null)
            {
                MessageBox.Show("Данная гильдия уже добавлена");
                return;
            }

            var newGuild = new Guild {Name = bannedGuildName, Realm = SelectedGuildServerName};

            BannedGuilds.Add(newGuild);
        }

        private void AddPlayerToBan(object sender, RoutedEventArgs e)
        {
            var player = BannedPlayers.FirstOrDefault(
                p => string.Equals(p.Name, PlayerNameTextBox.Text, StringComparison.OrdinalIgnoreCase));

            if (player != null)
            {
                if (string.Equals(player.Guild.Name, SelectedPlayerServerName, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Данный игрок уже добавлен");
                    return;
                }
            }

            var bannedPlayer = _explorer.GetCharacter(SelectedPlayerServerName, PlayerNameTextBox.Text,
                CharacterOptions.GetPetSlots);

            BannedPlayers.Add(bannedPlayer);
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (BannedGuilds.Any())
            {
                WriteObjects(BannedGuildsFileName, BannedGuilds.ToList());
            }

            if (BannedPlayers.Any())
            {
                WriteObjects(BannedPlayersFileName, BannedPlayers.ToList());
            }

            if (GuildsForChecking.Any())
            {
                WriteObjects(GuildsForCheckingFileName, GuildsForChecking.ToList());
            }
        }

        private void WriteObjects(string path, object obj)
        {
            using (XmlWriter textWriter = new XmlTextWriter(path, Encoding.Unicode))
            {
                var serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(textWriter, obj);
            }
        }

        private void AddGuildToChecking(object sender, RoutedEventArgs e)
        {
            var guildName = CheckingGuildNameTextBox.Text;
            if (string.IsNullOrEmpty(guildName))
            {
                MessageBox.Show("Введите значение");
                return;
            }

            var newGuild = _explorer.GetGuild(Region.EU, SelectedGuildCheckingServerName, guildName);
            GuildsForChecking.Add(newGuild);
        }

        private void UpdateBannedGuildsAndPeoples()
        {
            foreach (var bannedGuild in BannedGuilds.ToArray())
            {
                var guild = _explorer.GetGuild(Region.EU, bannedGuild.Realm, bannedGuild.Name,
                    GuildOptions.GetMembers);

                Parallel.ForEach(guild.Members, member =>
                {
                    member.FullCharactor = _explorer.GetCharacter(Region.EU, member.GuildCharacter.Realm,
                        member.GuildCharacter.Name, CharacterOptions.GetPetSlots);
                });

                BannedGuilds.Remove(bannedGuild);
                BannedGuilds.Add(guild);
            }

            foreach(var bannedPlayer in BannedPlayers.ToArray())
            {
                var updated = _explorer.GetCharacter(bannedPlayer.Realm, bannedPlayer.Name, CharacterOptions.GetPetSlots);
                BannedPlayers.Remove(bannedPlayer);
                BannedPlayers.Add(updated);
            }
        }

        private void CheckPlayersList(object sender, RoutedEventArgs e)
        {
            if (!BannedGuilds.Any())
            {
                MessageBox.Show("Нет забаненых гильдий");
                return;
            }

            var reportError = new StringBuilder();

            // Будем их чекать
            var checkPlayers = new ConcurrentBag<Character>();
            var splittedNames = TbRaidPlayersForCheck.Text.Split(new[] {Environment.NewLine},
                StringSplitOptions.RemoveEmptyEntries);

            Parallel.ForEach(splittedNames, player =>
            {
                var info = player.Split('-');
                if (info.Length != 2)
                {
                    reportError.AppendLine("Не удалось проверить " + player);
                    return;
                }

                var playerName = info[0];
                var serverName = info[1];

                try
                {
                    var @char = _explorer.GetCharacter(Region.EU, serverName, playerName, CharacterOptions.GetPetSlots);
                    checkPlayers.Add(@char);
                }
                catch (Exception ex)
                {
                    reportError.AppendLine("Не удалось проверить:" + player + Environment.NewLine + ex.Message);
                }
            });

            var outBuilder = new StringBuilder();

            foreach (var checkPlayer in checkPlayers)
            {
                foreach (var bannedGuild in BannedGuilds)
                {
                    foreach (var bannedMember in bannedGuild.Members)
                    {
                        if (bannedMember.FullCharactor == null || string.IsNullOrEmpty(bannedMember.FullCharactor.Name))
                        {
                            Debug.WriteLine("Missing char");
                            continue;
                        }

                        if (bannedMember.FullCharactor.PetSlots.All(slot => slot.IsEmpty))
                        {
                            outBuilder.AppendLine($"[!] bannedMember {bannedMember} no pets!");
                            continue;
                        }

                        if (checkPlayer.PetSlots.All(slot => slot.IsEmpty))
                        {
                            outBuilder.AppendLine($"[!] {checkPlayer} no pets!");
                            continue;
                        }

                        if (BattleNetUtils.IsEqualCharactors(checkPlayer, bannedMember.FullCharactor))
                        {
                            outBuilder.AppendLine(
                                $"[!] <{checkPlayer}> is twink of <{bannedMember.FullCharactor}> in guild [{bannedGuild.Name}]");
                        }
                    }
                }

                foreach (var bannedPlayer in BannedPlayers)
                {
                    if (BattleNetUtils.IsEqualCharactors(checkPlayer, bannedPlayer))
                    {
                        outBuilder.AppendLine(
                            $"[!] <{checkPlayer}> is banned player twink of <{bannedPlayer}>");
                    }
                }
            }
            
            if (outBuilder.Length == 0)
            {
                MessageBox.Show("Проверка завершена. Игроки чистые :)");
            }
            else
            {
                var resultWindow = new ResultWindow(outBuilder.ToString());
                resultWindow.ShowDialog();
            }
        }


        private void CheckGuildList(object sender, RoutedEventArgs e)
        {
            //if (!GuildsForChecking.Any())
            //{
            //    MessageBox.Show("Нет гильдий на проверку");
            //    return;
            //}

            //var outBuilder = new StringBuilder();
            //foreach (var guild in GuildsForChecking)
            //{
            //    var resultPlayerDictionary = PlayersUtils.Check(BannedPlayers, BannedGuilds, guild.Players);

            //    if (resultPlayerDictionary.Any())
            //    {
            //        var buff = PlayersUtils.GetSummary(resultPlayerDictionary);
            //        outBuilder.AppendLine(buff.ToString());
            //    }
            //}

            //if (outBuilder.Length == 0)
            //{
            //    MessageBox.Show("Проверка завершена. Игроки чистые :)");
            //}
            //else
            //{
            //    var resultWindow = new ResultWindow(outBuilder.ToString());
            //    resultWindow.ShowDialog();
            //}
            throw new NotSupportedException("!!!");
        }

        private void RemoveGuildToBan(object sender, RoutedEventArgs e)
        {
            var guildForRemove =
                BannedGuilds.FirstOrDefault(
                    guild =>
                        guild.Name.ToLower() == GuildNameTextBox.Text.ToLower() &&
                        guild.Name.ToLower() == SelectedGuildServerName.ToLower());

            if (guildForRemove == null)
            {
                MessageBox.Show("Гильдия не найдена");
                return;
            }

            BannedGuilds.Remove(guildForRemove);
        }

        private void RemovePlayerFromBan(object sender, RoutedEventArgs e)
        {
            var playerForRemove =
                BannedPlayers.FirstOrDefault(
                    player =>
                        player.Name.ToLower() == PlayerNameTextBox.Text.ToLower() &&
                        player.Realm.ToLower() == SelectedPlayerServerName.ToLower());

            if (playerForRemove == null)
            {
                MessageBox.Show("Игрок не найден");
                return;
            }

            BannedPlayers.Remove(playerForRemove);
        }

        private void RemoveGuildToChecking(object sender, RoutedEventArgs e)
        {
            GuildsForChecking.Remove(SelectedGuildForChecking);
        }

        private void CheckingGuildStartScan(object sender, RoutedEventArgs e)
        {
            //foreach (var guild in GuildsForChecking.ToList())
            //{
            //    string error;
            //    GuildObject newGuild = null;
            //    if (!BattleNetUtils.TryUpdateGuildPlayers(guild.GuildName, guild.ServerName, ref newGuild, out error))
            //    {
            //        MessageBox.Show(error);
            //        continue;
            //    }

            //    newGuild.LastUpdateTime = DateTime.Now;
            //    GuildsForChecking.Remove(guild);
            //    GuildsForChecking.Add(newGuild);
            //}
        }

        private void UpdateBannedGuilds(object sender, RoutedEventArgs e)
        {
            var dNow = DateTime.Now;
            UpdateBannedGuildsAndPeoples();
            MessageBox.Show("Данные по игрокам обновленны за " + (DateTime.Now - dNow));
        }
    }
}