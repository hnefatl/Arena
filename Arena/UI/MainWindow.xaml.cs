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
using Arena.UI.Dialogs;

namespace Arena.UI
{
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
            AllBots = new List<Bot>();

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            #region Prepare BotList ContextMenus
            #region BaseContextMenu
            ContextMenu BaseContextMenu = new ContextMenu();
            #region AddOwner
            MenuItem AddOwner = new MenuItem();
            AddOwner.Width = double.NaN;
            AddOwner.Height = double.NaN;
            AddOwner.Header = "Add Owner";
            AddOwner.Click += AddOwnerHandler;
            BaseContextMenu.Items.Add(AddOwner);
            #endregion
            #endregion

            #region OwnerContextMenu
            ContextMenu OwnerContextMenu = new ContextMenu();
            #region AddBot
            MenuItem AddBot = new MenuItem();
            AddBot.Width = double.NaN;
            AddBot.Height = double.NaN;
            AddBot.Header = "Add Bot";
            AddBot.Click += AddBotEventHandler;
            OwnerContextMenu.Items.Add(AddBot);
            #endregion
            #endregion

            ContextMenu BotContextMenu = new ContextMenu();

            ContextMenu VersionContextMenu = new ContextMenu();


            Bots.SetContextMenus(BaseContextMenu, OwnerContextMenu, BotContextMenu, VersionContextMenu);
            #endregion

            WindowInitialised();

            base.OnInitialized(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Task.Factory.StartNew(new Action(WindowClosing));

            base.OnClosing(e);
        }

        protected void AddOwnerHandler(object sender, RoutedEventArgs e)
        {
            StringInputDialog Dialog = new StringInputDialog();
            Dialog.Prompt = "Owner Name:";
            bool? Result = Dialog.ShowDialog();
            if (Result.HasValue && Result.Value) // Pressed Ok
            {
                if (!Bots.AddOwner(Dialog.Input)) // Add the owner
                {
                    MessageBox.Show("An Owner with that name already exists.", "Error");
                }
            }
            else
            {
                return;
            }
        }
        protected void AddBotEventHandler(object sender, RoutedEventArgs e)
        {
            StringInputDialog Dialog = new StringInputDialog();
            Dialog.Prompt = "Bot Name:";
            bool? Result = Dialog.ShowDialog();
            if (Result.HasValue && Result.Value) // Pressed Ok
            {
                if (!Bots.AddOwner(Dialog.Input)) // Add the owner
                {
                    MessageBox.Show("A bot with that name already exists under this owner.", "Error");
                }
            }
            else
            {
                return;
            }
        }

        protected void WindowInitialised()
        {
            Task.Factory.StartNew(LoadBots);
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
                Cover.SetVisible(true);
                XmlDocument LoadFile = new XmlDocument();
                LoadFile.Load(BotSaveFile);
                LoadFile.Normalize();

                Cover.ExecuteOnBar((BarExecutable)((ProgressBar Bar) =>
                {
                    Bar.Minimum = 0;
                    Bar.Maximum = LoadFile.SelectNodes("//Version").Count;
                    Bar.Value = 0;
                }));

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

                            Cover.ExecuteOnBar((BarExecutable)((ProgressBar Bar) =>
                            {
                                Bar.Value++;
                            }));
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Failed to load all bots.");
            }
            finally
            {
                Cover.SetVisible(false);
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
