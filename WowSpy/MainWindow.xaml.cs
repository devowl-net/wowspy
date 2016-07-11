using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Xml.Serialization;
using WowDotNetAPI;
using WowSpy.Serialization;
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
        
        private string _apiKey = "pzdz2hfp2efzgkaf4khy2ubsp8ke4x7c";
        private WowExplorer _explorer;
        public MainWindow()
        {
            Initialize();
            DataContext = this;
            InitializeComponent();
        }

        public ObservableCollection<GuildObject> BannedGuilds { get; set; }

        public ObservableCollection<PlayerObj> BannedPlayers { get; set; }

        public ObservableCollection<GuildObject> GuildsForChecking { get; set; }

        public List<string> ServerNames { get; set; }

        public GuildObject SelectedGuildForChecking { get; set; }

        public string SelectedPlayerServerName { get; set; }

        public string SelectedGuildServerName { get; set; }

        public string SelectedGuildCheckingServerName { get; set; }

        private void Initialize()
        {
            _explorer = new WowExplorer(Region.EU, Locale.en_US, _apiKey);
            if (File.Exists(BannedGuildsFileName))
            {
                try
                {
                    using (TextReader textReader = new StreamReader(GuildsForCheckingFileName))
                    {
                        var serializer = new XmlSerializer(typeof (List<GuildObject>));
                        GuildsForChecking =
                            new ObservableCollection<GuildObject>((List<GuildObject>) serializer.Deserialize(textReader));
                    }
                }
                catch
                {
                }
            }

            GuildsForChecking = GuildsForChecking ?? new ObservableCollection<GuildObject>();

            if (File.Exists(BannedGuildsFileName))
            {
                try
                {
                    using (TextReader textReader = new StreamReader(BannedGuildsFileName))
                    {
                        var serializer = new XmlSerializer(typeof (List<GuildObject>));
                        BannedGuilds =
                            new ObservableCollection<GuildObject>((List<GuildObject>) serializer.Deserialize(textReader));
                    }
                }
                catch
                {
                }
            }

            BannedGuilds = BannedGuilds ?? new ObservableCollection<GuildObject>();


            if (File.Exists(BannedPlayersFileName))
            {
                try
                {
                    using (TextReader textReader = new StreamReader(BannedPlayersFileName))
                    {
                        var serializer = new XmlSerializer(typeof (List<PlayerObj>));
                        BannedPlayers =
                            new ObservableCollection<PlayerObj>((List<PlayerObj>) serializer.Deserialize(textReader));
                    }
                }
                catch
                {
                }
            }

            BannedPlayers = BannedPlayers ?? new ObservableCollection<PlayerObj>();

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
                string.Equals(g.GuildName, bannedGuildName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(g.ServerName, SelectedGuildServerName, StringComparison.OrdinalIgnoreCase));

            if (bGuild != null)
            {
                MessageBox.Show("Данная гильдия уже добавлена");
                return;
            }

            string error;
            GuildObject newGuild = null;
            if (!BattleNetUtils.TryUpdateGuildPlayers(bannedGuildName, SelectedGuildServerName, ref newGuild, out error))
            {
                FlashThisWindow();
                MessageBox.Show(error);
                return;
            }

            FlashThisWindow();
            BannedGuilds.Add(newGuild);
        }

        private void AddPlayerToBan(object sender, RoutedEventArgs e)
        {
            var player = BannedPlayers.FirstOrDefault(
                p => string.Equals(p.PlayerName, PlayerNameTextBox.Text, StringComparison.OrdinalIgnoreCase));

            if (player != null)
            {
                if (string.Equals(player.GuildName, SelectedPlayerServerName, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Данный игрок уже добавлен");
                    return;
                }
            }

            var bannedPlayer = new PlayerObj
            {
                PlayerName = PlayerNameTextBox.Text,
                ServerName = SelectedPlayerServerName
            };
            if (!BattleNetUtils.TryUpdatePlayerInfo(bannedPlayer))
            {
                FlashThisWindow();
                MessageBox.Show("Не удалось обновить инфу");
            }

            FlashThisWindow();
            BannedPlayers.Add(bannedPlayer);
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            using (TextWriter textWriter = new StreamWriter(BannedGuildsFileName))
            {
                var serializer = new XmlSerializer(typeof (List<GuildObject>));
                serializer.Serialize(textWriter, BannedGuilds.ToList());
            }

            using (TextWriter textWriter = new StreamWriter(BannedPlayersFileName))
            {
                var serializer = new XmlSerializer(typeof (List<PlayerObj>));
                serializer.Serialize(textWriter, BannedPlayers.ToList());
            }

            using (TextWriter textWriter = new StreamWriter(GuildsForCheckingFileName))
            {
                var serializer = new XmlSerializer(typeof (List<GuildObject>));
                serializer.Serialize(textWriter, GuildsForChecking.ToList());
            }
        }

        private void StartScan(object sender, RoutedEventArgs e)
        {
            if (ChGuildFullCheck.IsChecked.GetValueOrDefault())
            {
                foreach (var bannedGuild in BannedGuilds.ToList())
                {
                    string error;
                    GuildObject newGuild = null;
                    if (
                        !BattleNetUtils.TryUpdateGuildPlayers(bannedGuild.GuildName, bannedGuild.ServerName,
                            ref newGuild, out error))
                    {
                        MessageBox.Show(error);
                        continue;
                    }

                    BannedGuilds.Remove(bannedGuild);
                    BannedGuilds.Add(newGuild);
                }
            }
            else
            {
                foreach (var bannedGuild in BannedGuilds)
                {
                    foreach (var player in bannedGuild.Players)
                    {
                        BattleNetUtils.TryUpdatePlayerInfo(player);
                    }
                }
            }

            foreach (var bannedPlayer in BannedPlayers)
            {
                BattleNetUtils.TryUpdatePlayerInfo(bannedPlayer);
            }

            FlashThisWindow();
        }

        private void Processor(object obj)
        {
        }

        private void AddGuildToChecking(object sender, RoutedEventArgs e)
        {
            var guildName = CheckingGuildNameTextBox.Text;
            if (string.IsNullOrEmpty(guildName))
            {
                MessageBox.Show("Введите значение");
                return;
            }

            string error;
            GuildObject newGuild = null;
            if (
                !BattleNetUtils.TryUpdateGuildPlayers(guildName, SelectedGuildCheckingServerName, ref newGuild,
                    out error))
            {
                FlashThisWindow();
                MessageBox.Show(error);
                return;
            }
            FlashThisWindow();
            GuildsForChecking.Add(newGuild);
        }


        private void CheckPlayersList(object sender, RoutedEventArgs e)
        {
            var checkPlayerNames = new List<PlayerObj>();
            var playersList = TbRaidPlayersForCheck.Text;
            var playersSplits = playersList.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var player in playersSplits)
            {
                var info = player.Split('-');
                if (info.Length != 2)
                {
                    MessageBox.Show("Не удалось проверить " + player);
                    return;
                }

                var newPlayer = new PlayerObj
                {
                    PlayerName = info[0],
                    ServerName = info[1]
                };

                if (!BattleNetUtils.TryUpdatePlayerInfo(newPlayer))
                {
                    MessageBox.Show("Не удалось получить иформацию об игроке: " + player);
                    return;
                }

                checkPlayerNames.Add(newPlayer);
            }

            var resultPlayerDictionary = PlayersUtils.Check(BannedPlayers, BannedGuilds, checkPlayerNames);


            var outBuilder = PlayersUtils.GetSummary(resultPlayerDictionary);

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


        protected override void OnActivated(EventArgs e)
        {
            StopFlashingWindow();
            base.OnActivated(e);
        }


        private void CheckGuildList(object sender, RoutedEventArgs e)
        {
            if (!GuildsForChecking.Any())
            {
                MessageBox.Show("Нет гильдий на проверку");
                return;
            }

            var outBuilder = new StringBuilder();
            foreach (var guild in GuildsForChecking)
            {
                var resultPlayerDictionary = PlayersUtils.Check(BannedPlayers, BannedGuilds, guild.Players);

                if (resultPlayerDictionary.Any())
                {
                    var buff = PlayersUtils.GetSummary(resultPlayerDictionary);
                    outBuilder.AppendLine(buff.ToString());
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

        private void RemoveGuildToBan(object sender, RoutedEventArgs e)
        {
            var guildForRemove =
                BannedGuilds.FirstOrDefault(
                    guild =>
                        guild.GuildName.ToLower() == GuildNameTextBox.Text.ToLower() &&
                        guild.ServerName.ToLower() == SelectedGuildServerName.ToLower());

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
                        player.GuildName.ToLower() == PlayerNameTextBox.Text.ToLower() &&
                        player.ServerName.ToLower() == SelectedPlayerServerName.ToLower());

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
            foreach (var guild in GuildsForChecking.ToList())
            {
                string error;
                GuildObject newGuild = null;
                if (!BattleNetUtils.TryUpdateGuildPlayers(guild.GuildName, guild.ServerName, ref newGuild, out error))
                {
                    MessageBox.Show(error);
                    continue;
                }

                newGuild.LastUpdateTime = DateTime.Now;
                GuildsForChecking.Remove(guild);
                GuildsForChecking.Add(newGuild);
            }

            FlashThisWindow();
        }

        #region FlashWindow

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }


        /// <summary>
        ///     Flashes a window
        /// </summary>
        /// <param name="hWnd">The handle to the window to flash</param>
        /// <returns>whether or not the window needed flashing</returns>
        public bool FlashThisWindow()
        {
            var fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = new WindowInteropHelper(this).Handle;
            fInfo.dwFlags = 3;
            fInfo.uCount = uint.MaxValue;
            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }

        public bool StopFlashingWindow()
        {
            var hWnd = new WindowInteropHelper(this).Handle;
            if (IntPtr.Zero != hWnd)
            {
                var fi = new FLASHWINFO();
                fi.cbSize = (uint) Marshal.SizeOf(typeof (FLASHWINFO));
                fi.dwFlags = 0;
                fi.hwnd = hWnd;

                return FlashWindowEx(ref fi);
            }
            return false;
        }

        #endregion
    }
}