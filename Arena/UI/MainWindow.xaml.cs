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
        public List<Bot> AllBots { get; set; }
        public string BotSaveFile { get; set; }

        public MainWindow()
        {
            BotSaveFile = "SavedBots";
            
            Initialized += new EventHandler((object sender, EventArgs e)=>
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
            SetCovererVisibility(true);
            ResetProgress(Progress);

            List<Task> Tasks = new List<Task>();
            Tasks.Add(Task.Factory.StartNew(new Action(LoadBots)));


            Task.WaitAll(Tasks.ToArray());

            SetCovererVisibility(false);
        }
        protected void WindowClosing()
        {
            SetCovererVisibility(true);
            ResetProgress(Progress);

            List<Task> Tasks = new List<Task>();
            Tasks.Add(Task.Factory.StartNew(new Action(SaveBots)));


            Task.WaitAll(Tasks.ToArray());
            
            SetCovererVisibility(false);
        }

        #region Concurrent ProgressBar access methods
        public delegate void SetCovererVisibilityDelegate(bool Visible);
        public void SetCovererVisibility(bool Visible)
        {
            if(Coverer.Dispatcher.CheckAccess())
            {
                if(Progress.Dispatcher.CheckAccess())
                {
                    if(Visible)
                    {
                        Coverer.Visibility = Visibility.Visible;
                        Progress.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Coverer.Visibility = Visibility.Hidden;
                        Progress.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    Progress.Dispatcher.BeginInvoke(new SetCovererVisibilityDelegate(SetCovererVisibility), System.Windows.Threading.DispatcherPriority.Background, Visible);
                }
            }
            else
            {
                Coverer.Dispatcher.BeginInvoke(new SetCovererVisibilityDelegate(SetCovererVisibility), System.Windows.Threading.DispatcherPriority.Background, Visible);
            }
        }

        public delegate void IncrementProgressDelegate(ProgressBar Bar);
        public void IncrementProgress(ProgressBar Bar)
        {
            if (Bar.Dispatcher.CheckAccess())
            {
                lock (Bar)
                {
                    Bar.Value++;
                }
            }
            else
            {
                Bar.Dispatcher.BeginInvoke(new IncrementProgressDelegate(IncrementProgress), System.Windows.Threading.DispatcherPriority.Background, Bar);
            }
        }

        public delegate void IncreaseProgressMaximumDelegate(ProgressBar Bar, double Maximum);
        public void IncreaseProgressMaximum(ProgressBar Bar, double Maximum)
        {
            if (Bar.Dispatcher.CheckAccess())
            {
                lock (Bar)
                {
                    Bar.Maximum += Maximum;
                }
            }
            else
            {
                Bar.Dispatcher.BeginInvoke(new IncreaseProgressMaximumDelegate(IncreaseProgressMaximum), System.Windows.Threading.DispatcherPriority.Background, new object[] { Bar, Maximum });
            }
        }

        public delegate void ResetProgressDelegate(ProgressBar Bar);
        public void ResetProgress(ProgressBar Bar)
        {
            if(Bar.Dispatcher.CheckAccess())
            {
                lock(Bar)
                {
                    Bar.Value = 0;
                    Bar.Minimum = 0;
                    Bar.Maximum = 0;
                }
            }
            else
            {
                Bar.Dispatcher.BeginInvoke(new ResetProgressDelegate(ResetProgress), System.Windows.Threading.DispatcherPriority.Background, Bar);
            }
        }
        #endregion

        public delegate void LoadBotsDelegate();
        public void LoadBots()
        {
            if (Bots.Dispatcher.CheckAccess())
            {
                if (!File.Exists(BotSaveFile))
                {
                    return;
                }
                try
                {
                    IncreaseProgressMaximum(Progress, (int)File.ReadAllLines(BotSaveFile).Length); // Increase operation count

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
                            IncrementProgress(Progress); // Increase progress
                            Thread.Sleep(5000);
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
            else
            {
                Bots.Dispatcher.BeginInvoke(new LoadBotsDelegate(LoadBots), System.Windows.Threading.DispatcherPriority.Background);
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
