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
            BotSaveFile = "SavedBots";

            Initialized += new EventHandler((object sender, EventArgs e) =>
                                            {
                                                Task.Factory.StartNew(new Action(WindowInitialised));
                                            });

            Closing += new System.ComponentModel.CancelEventHandler((object sender, System.ComponentModel.CancelEventArgs e) =>
                                                                    {
                                                                        Task.Factory.StartNew(new Action(WindowClosing));
                                                                    });

            InitializeComponent();
        }

        protected void WindowInitialised()
        {
            /*
            if (Bars.Dispatcher.CheckAccess())
            {
                SetCovererVisibility(true);

                Bars.AddProgressBar("LoadBots");
                
                int CompletedTasks=0;
                BackgroundWorker LoadBotsWorker=new BackgroundWorker();
                LoadBotsWorker.DoWork+= (Action)(()=>
                                                 {
                                                 	LoadBots(Bars.GetProgressBar("LoadBots"));
                                                 });
                LoadBotsWorker.ProgressChanged+= (Action)(()=>
                                                          {
                                                          	Bars.GetProgressBar("LoadBots").Value++;
                                                          });
                LoadBotsWorker.RunWorkerCompleted+=(Action)(()=>
                                                            {
                                                            	lock(CompletedTasks)
                                                            	{
                                                            		CompletedTasks++;
                                                            		if(CompletedTasks>=1)
                                                            		{
                                                            			SetCovererVisibility(false);
                                                            		}
                                                            	}
                                                            });
            }
            else
            {
                Bars.Dispatcher.Invoke((Action)WindowInitialised);
            }
            */
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

        public void LoadBots(ProgressBar Bar)
        {
            if (!File.Exists(BotSaveFile))
            {
                return;
            }
            try
            {
                Bar.Maximum = (double)File.ReadAllLines(BotSaveFile).Length; // Increase operation count

                AllBots = new List<Bot>();
                StreamReader Reader = new StreamReader(BotSaveFile);

                while (true)
                {
                    string Filename = Reader.ReadLine();
                    if (Filename == null)
                        break;

                    Bot New = new Bot();
                    if (New.Initialise(Filename))
                    {
                        AllBots.Add(New);
                        Bar.Value++;
                        Thread.Sleep(1000);
                    }
                }
                Reader.Close();

                // Loaded all bots
                #region Postprocessing
                lock (Bots)
                {
                    Bots.Items.Clear();
                    for (int x = 0; x < AllBots.Count; x++)
                    {
                        #region Find owner
                        TreeViewItem Owner = null;
                        for (int y = 0; y < Bots.Items.Count; y++) // Search for owner in tree
                        {
                            if ((string)((TreeViewItem)Bots.Items[y]).Header == AllBots[x].Owner) // Same owner
                            {
                                Owner = (TreeViewItem)Bots.Items[y];
                                break;
                            }
                        }
                        if (Owner == null) // Create new owner
                        {
                            Owner = new TreeViewItem();
                            Owner.Header = AllBots[x].Owner;
                            Bots.Items.Add(Owner);
                        }
                        #endregion
                        #region Find name
                        TreeViewItem Name = null;
                        foreach (TreeViewItem n in Owner.Items)
                        {
                            if ((string)n.Header == AllBots[x].Name)
                            {
                                Name = n;
                            }
                        }
                        if (Name == null)
                        {
                            Name = new TreeViewItem();
                            Name.Header = AllBots[x].Name;
                            Owner.Items.Add(Name);
                        }
                        #endregion
                        BotTreeViewItem NewBot = new BotTreeViewItem(AllBots[x]);
                        NewBot.MouseDoubleClick += BotSelected;
                        Name.Items.Add(NewBot);
                    }
                    #region Add ToolTips
                    for (int x = 0; x < Bots.Items.Count; x++)
                    {
                        (Bots.Items[x] as TreeViewItem).ToolTip = "Bots: " + (Bots.Items[x] as TreeViewItem).Items.Count;
                        for (int y = 0; y < ((TreeViewItem)Bots.Items[x]).Items.Count; y++)
                        {
                            ((TreeViewItem)(((TreeViewItem)Bots.Items[x]).Items[y])).ToolTip = "Versions: " + ((TreeViewItem)(((TreeViewItem)Bots.Items[x]).Items[y])).Items.Count;
                        }
                    }
                    #endregion
                }
                #endregion
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
