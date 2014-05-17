using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;

using Arena.Logic;

namespace Arena.UI
{
    #region BotTreeViewItem
    public class BotTreeViewItem
        : TreeViewItem
    {
        public Bot Inner
        {
            get
            {
                return _Inner;
            }
            set
            {
                _Inner = value;
                Header = _Inner.Version;
                ToolTip = _Inner.ToString();
            }
        }
        protected Bot _Inner;

        public BotTreeViewItem()
        {
        }
        public BotTreeViewItem(Bot Rep)
        {
            Inner = Rep;
        }
    }
    #endregion

    public partial class MainWindow : Window
    {
        /// <summary>
        /// Easy-access List of all bots in the TreeView
        /// </summary>
        public List<Bot> AllBots { get; set; }
        public string BotSaveFile { get; set; }

        public MainWindow()
        {
            BotSaveFile = "SavedBots.xml";

            Initialized += new EventHandler((object sender, EventArgs e) =>
                                            {
                                                Task.Factory.StartNew(new Action(WindowInitialised));
                                            });

            Closing += new System.ComponentModel.CancelEventHandler((object sender, System.ComponentModel.CancelEventArgs e) =>
                                                                    {
                                                                        Task.Factory.StartNew(new Action(WindowClosing));
                                                                    });

            InitializeComponent();

            #region Prepare BotList ContextMenus
            Bots.BaseContextMenu = new ContextMenu();
            Bots.BaseContextMenu.Items.Add(new MenuItem()
            {
                Header = "Add Owner",
            });
            ((MenuItem)Bots.BaseContextMenu.Items[0]).Click += AddOwnerHandler;

            #endregion
        }

        protected void AddOwnerHandler(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(sender.GetType().ToString());
        }

        protected void WindowInitialised()
        {
            LoadBots();
        }
        protected void WindowClosing()
        {
            Cover.SetVisible(true);
            Cover.ExecuteOnBar((BarExecutable)((ProgressBar Bar) =>
            {
                Bar.Minimum = 0;
                Bar.Maximum = AllBots.Count;
                Bar.Value = 0; // Reset progress - we're going to be saving the bot info to disk
            }));

            Cover.SetVisible(false);
        }

        public void LoadBots()
        {
            if (!File.Exists(BotSaveFile))
            {
                return;
            }
            try
            {
                XmlDocument LoadFile = new XmlDocument();
                LoadFile.Load(BotSaveFile);
                LoadFile.Normalize();

                XmlNodeList Owners = LoadFile.SelectSingleNode("/SavedBots").SelectNodes("Owner");
                for (int x = 0; x < Owners.Count; x++)
                {
                    string OwnerName = Owners[x].Attributes["Name"].Value;
                    XmlNodeList Bots = Owners[x].SelectNodes("Bot");
                    for (int y = 0; y < Bots.Count; y++)
                    {
                        string BotName = Bots[x].Attributes["Name"].Value;
                        XmlNodeList Versions = Bots[y].SelectNodes("Version");
                        for (int z = 0; z < Versions.Count; z++)
                        {
                            string Version = Versions[z].Attributes["Value"].Value;
                            string BotPath = Versions[z].InnerText;
                            this.Bots.AddBotVersion(OwnerName, BotName, Version, BotPath);
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Failed to load all bots.");
            }
        }

        void BotSelected(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Bot InnerBot = ((BotTreeViewItem)sender).Inner;
                MessageBox.Show("Selected");
            }
            catch
            {
                return;
            }
        }

        public void SaveBots()
        {
            lock (AllBots)
            {
                StreamWriter Writer = new StreamWriter(BotSaveFile);

                for (int x = 0; x < AllBots.Count; x++)
                {
                    Writer.WriteLine(AllBots[x].Path);
                }

                Writer.Close();
            }
        }
    }
}
