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

using Arena.Logic;

namespace Arena.UI
{
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

    public partial class MainWindow : Window
    {
        public List<Bot> AllBots { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LoadBots("SavedBots");
        }

        public void LoadBots(string File)
        {
            if (!System.IO.File.Exists(File))
            {
                return;
            }
            try
            {
                AllBots = new List<Bot>();
                StreamReader Reader = new StreamReader(File);

                while (true)
                {
                    string Filename = Reader.ReadLine();
                    if (Filename == null)
                        break;

                    Bot New = new Bot();
                    if (New.Initialise(Filename))
                    {
                        AllBots.Add(New);
                    }
                }
                Reader.Close();

                // Loaded all bots
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
                    for(int y=0; y<((TreeViewItem)Bots.Items[x]).Items.Count; y++)
                    {
                        ((TreeViewItem)(((TreeViewItem)Bots.Items[x]).Items[y])).ToolTip = "Versions: " + ((TreeViewItem)(((TreeViewItem)Bots.Items[x]).Items[y])).Items.Count;
                    }
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
            Bot InnerBot = ((BotTreeViewItem)sender).Inner;
            MessageBox.Show("Selected");
        }

        public void SaveBots()
        {

        }
    }
}
