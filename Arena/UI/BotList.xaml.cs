using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
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

using Arena.UI.Dialogs;

namespace Arena.UI
{
    public class OwnerTreeViewItem
        : TreeViewItem
    {
        public new BotList Parent
        {
            get
            {
                return (BotList)base.Parent;
            }
        }

        public OwnerTreeViewItem(string Header)
        {
            ToolTip = null;
            ContextMenu = new ContextMenu();
            this.Header = Header;
            #region AddBot
            MenuItem AddBot = new MenuItem();
            AddBot.Width = double.NaN;
            AddBot.Height = double.NaN;
            AddBot.Header = "Add Bot";
            AddBot.Click += AddBot_Handler;
            ContextMenu.Items.Add(AddBot);
            #endregion
            #region Delete
            MenuItem Delete = new MenuItem();
            Delete.Width = double.NaN;
            Delete.Height = double.NaN;
            Delete.Header = "Delete";
            Delete.Click += Delete_Handler;
            #endregion
        }

        protected void AddBot_Handler(object sender, RoutedEventArgs e)
        {
            StringInputDialog Dialog = new StringInputDialog();
            Dialog.Prompt = "Bot Name:";
            bool? Result = Dialog.ShowDialog();
            if (Result.HasValue && Result.Value) // Pressed Ok
            {
                BotTreeViewItem Temp;
                if (!Parent.AddBot((string)this.Header, Dialog.Input, out Temp)) // Add the owner
                {
                    MessageBox.Show("A bot with that name already exists under this owner.", "Error");
                }
            }
            else
            {
                return;
            }
        }
        protected void Delete_Handler(object sender, EventArgs e)
        {
            Parent.RemoveOwner((string)this.Header);
        }
    }

    public class BotTreeViewItem
        : TreeViewItem
    {
        public new OwnerTreeViewItem Parent
        {
            get
            {
                return (OwnerTreeViewItem)base.Parent;
            }
        }

        public BotTreeViewItem(string Header)
        {
            this.Header = Header;
            ToolTip = null;
            ContextMenu = new ContextMenu();
        }
    }

    public class VersionTreeViewItem
        : TreeViewItem
    {
        public new BotTreeViewItem Parent
        {
            get
            {
                return (BotTreeViewItem)base.Parent;
            }
        }

        public VersionTreeViewItem(string Header)
        {
            this.Header = Header;
            ToolTip = null;
            ContextMenu = new ContextMenu();
        }
    }

    public partial class BotList
        : TreeView
    {
        /* Maps to:
                            Owner Name -> Bot Name -> Version -> Bot Path
        */
        protected Dictionary<string, Dictionary<string, Dictionary<string, string>>> Inner { get; set; }

        public BotList()
        {
            InitializeComponent();

            Inner = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        }

        #region Add Methods
        private delegate bool AddOwnerDelegate(string OwnerName, out OwnerTreeViewItem New);
        /// <summary>
        /// Adds an Owner to the view
        /// </summary>
        /// <returns>True if an Owner was addded, else False</returns>
        public bool AddOwner(string OwnerName, out OwnerTreeViewItem New)
        {
            New = null; // Temp value
            if (Dispatcher.CheckAccess())
            {
                #region Sanity check - Arguments aren't empty
                if (OwnerName == string.Empty)
                {
                    throw new Exception("At least one of the arguments was empty.");
                }
                #endregion

                #region Add Owner

                if (!Inner.ContainsKey(OwnerName))
                {
                    Inner.Add(OwnerName, new Dictionary<string, Dictionary<string, string>>());
                    New = new OwnerTreeViewItem(OwnerName);
                    Items.Add(New);
                    return true;
                }
                foreach (TreeViewItem Item in Items)
                {
                    if ((string)Item.Header == OwnerName) // Find the Owner
                    {
                        New = (OwnerTreeViewItem)Item;
                        break;
                    }
                }

                return false;

                #endregion
            }
            else
            {
                return (bool)Dispatcher.Invoke((AddOwnerDelegate)AddOwner, new object[] { OwnerName, New });
            }
        }

        private delegate bool AddBotDelegate(string OwnerName, string BotName, out BotTreeViewItem New);
        /// <summary>
        /// Adds a Bot to the view
        /// </summary>
        /// <returns>True if a Bot was addded, else False</returns>
        public bool AddBot(string OwnerName, string BotName, out BotTreeViewItem New)
        {
            New = null; // Temp value
            if (Dispatcher.CheckAccess())
            {
                #region Sanity check - Arguments aren't empty
                if (OwnerName == string.Empty || BotName == string.Empty)
                {
                    throw new Exception("At least one of the arguments was empty.");
                }
                #endregion

                #region Add Bots

                OwnerTreeViewItem Owner;
                AddOwner(OwnerName, out Owner);
                if (!Inner[OwnerName].ContainsKey(BotName))
                {
                    Inner[OwnerName].Add(BotName, new Dictionary<string, string>());
                    New = new BotTreeViewItem(BotName);
                    Owner.Items.Add(New);
                    // Only update tooltips if we've added a new Bot
                    #region Update Category ToolTips (Bot)
                    try
                    {
                        Owner.ToolTip = "Bots: " + (Convert.ToInt32(Split((string)Owner.ToolTip, ' ')[1]) + 1); // Add one to the current bot number (have to extract it from the string)
                    }
                    catch
                    {
                        Owner.ToolTip = "Bots: 1";
                    }
                    #endregion

                    return true;
                }
                foreach (BotTreeViewItem Item in Owner.Items)
                {
                    if ((string)Item.Header == BotName)
                    {
                        New = Item;
                        break;
                    }
                }
                return false;

                #endregion
            }
            else
            {
                return (bool)Dispatcher.Invoke((AddBotDelegate)AddBot, new object[] { OwnerName, BotName, New });
            }
        }

        private delegate bool AddVersionDelegate(string OwnerName, string BotName, string Version, string BotPath, out VersionTreeViewItem New);
        /// <summary>
        /// Adds a Version to the view
        /// </summary>
        /// <returns>True if a Version was addded, else False</returns>
        public bool AddVersion(string OwnerName, string BotName, string Version, string BotPath, out VersionTreeViewItem New)
        {
            New = null;
            if (Dispatcher.CheckAccess())
            {
                #region Sanity check - Arguments aren't empty
                if (OwnerName == string.Empty || BotName == string.Empty || Version == string.Empty || BotPath == string.Empty)
                {
                    throw new Exception("At least one of the arguments was empty.");
                }
                #endregion

                #region Add Bots

                BotTreeViewItem BotParent;
                AddBot(OwnerName, BotName, out BotParent);

                bool AddedVersionItem = false;
                VersionTreeViewItem VersionItem = new VersionTreeViewItem(Version);
                if (!Inner[OwnerName][BotName].ContainsKey(Version))    // Doesn't exist already - add it
                {
                    Inner[OwnerName][BotName].Add(Version, BotPath);
                    #region Create and Insert VersionItem
                    BotParent.Items.Add(VersionItem);
                    #endregion

                    // Only update tooltips if we've added a new Version
                    #region Update Category ToolTips (Version)
                    try
                    {
                        BotParent.ToolTip = "Versions: " + (Convert.ToInt32(Split((string)BotParent.ToolTip, ' ')[1]) + 1); // Add one to the current bot number (have to extract it from the string)
                    }
                    catch
                    {
                        BotParent.ToolTip = "Versions: 1";
                    }
                    #endregion

                    AddedVersionItem = true;
                }
                else                                                    // Does exist already - overwrite it
                {
                    Inner[OwnerName][BotName][Version] = BotPath;
                    foreach (VersionTreeViewItem Item in BotParent.Items)
                    {
                        if ((string)Item.Header == Version)
                        {
                            VersionItem = Item;
                            break;
                        }
                    }
                }
                VersionItem.ToolTip = "Path: " + BotPath;

                New = VersionItem;

                return AddedVersionItem;

                #endregion
            }
            else
            {
                return (bool)Dispatcher.Invoke((AddVersionDelegate)AddVersion, new object[] { OwnerName, BotName, Version, BotPath, New });
            }
        }
        #endregion

        #region Remove Methods
        /// <summary>
        /// Removes an Owner from the view
        /// </summary>
        /// <returns>True if an Owner was added, else False</returns>
        public bool RemoveOwner(string OwnerName)
        {
            if (Dispatcher.CheckAccess())
            {
                try
                {
                    #region Remove UI
                    for (int x = 0; x < Items.Count; x++)
                    {
                        if ((string)((TreeViewItem)Items[x]).Header == OwnerName)
                        {
                            Items.RemoveAt(x);
                        }
                    }
                    #endregion
                    #region Remove Virtual
                    Inner.Remove(OwnerName);
                    #endregion
                    return true;
                }
                catch
                {
                    // Don't care about any exceptions - if it doesn't exist then we can't remove it
                    return false;
                }
            }
            else
            {
                return (bool)Dispatcher.Invoke((Func<string, bool>)RemoveOwner, OwnerName);
            }
        }
        /// <summary>
        /// Removes a Bot from the view
        /// </summary>
        /// <returns>True if a Bot was added, else False</returns>
        public bool RemoveBot(string OwnerName, string BotName)
        {
            if (Dispatcher.CheckAccess())
            {
                try
                {
                    #region Remove UI
                    for (int x = 0; x < Items.Count; x++)
                    {
                        if ((string)((TreeViewItem)Items[x]).Header == OwnerName)
                        {
                            for (int y = 0; y < ((TreeViewItem)Items[x]).Items.Count; y++)
                            {
                                if ((string)((TreeViewItem)((TreeViewItem)Items[x]).Items[y]).Header == BotName)
                                {
                                    ((TreeViewItem)Items[x]).Items.RemoveAt(y);
                                }
                            }
                        }
                    }
                    #endregion
                    #region Remove Virtual
                    Inner[OwnerName].Remove(BotName);
                    #endregion

                    // Means we actually removed something
                    #region Update ToolTips
                    for (int x = 0; x < Items.Count; x++)
                    {
                        if ((string)((TreeViewItem)Items[x]).Header == OwnerName)
                        {
                            ((TreeViewItem)Items[x]).ToolTip = "Bots: " + (Convert.ToInt32(Split((string)((TreeViewItem)Items[x]).ToolTip, ' ')[1]) + 1);
                        }
                    }
                    #endregion

                    return true;
                }
                catch
                {
                    // Don't care about any exceptions - if it doesn't exist then we can't remove it
                    return false;
                }
            }
            else
            {
                return (bool)Dispatcher.Invoke((Func<string, string, bool>)RemoveBot, new object[] { OwnerName, BotName });
            }
        }
        /// <summary>
        /// Removes a Version from the view
        /// </summary>
        /// <returns>True if a Version was added, else False</returns>
        public bool RemoveBotVersion(string OwnerName, string BotName, string Version)
        {
            if (Dispatcher.CheckAccess())
            {
                try
                {
                    #region Remove UI
                    for (int x = 0; x < Items.Count; x++)
                    {
                        if ((string)((TreeViewItem)Items[x]).Header == OwnerName)
                        {
                            for (int y = 0; y < ((TreeViewItem)Items[x]).Items.Count; y++)
                            {
                                if ((string)((TreeViewItem)((TreeViewItem)Items[x]).Items[y]).Header == BotName)
                                {
                                    for (int z = 0; z < ((TreeViewItem)((TreeViewItem)Items[x]).Items[y]).Items.Count; z++)
                                    {
                                        if ((string)((TreeViewItem)((TreeViewItem)((TreeViewItem)Items[x]).Items[y])).Items[z] == Version)
                                        {
                                            ((TreeViewItem)((TreeViewItem)((TreeViewItem)Items[x]).Items[y])).Items.RemoveAt(z);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    #region Remove Virtual
                    Inner[OwnerName][BotName].Remove(Version);
                    #endregion

                    // Means we actually removed something
                    #region Update ToolTips
                    for (int x = 0; x < Items.Count; x++)
                    {
                        if ((string)((TreeViewItem)Items[x]).Header == OwnerName)
                        {
                            for (int y = 0; y < ((TreeViewItem)Items[x]).Items.Count; y++)
                            {
                                if ((string)((TreeViewItem)((TreeViewItem)Items[x]).Items[y]).Header == BotName)
                                {
                                    ((TreeViewItem)((TreeViewItem)Items[x]).Items[y]).ToolTip = "Versions: " + (Convert.ToInt32(Split((string)((TreeViewItem)((TreeViewItem)Items[x]).Items[y]).ToolTip, ' ')[1]) + 1);
                                }
                            }
                        }
                    }
                    #endregion
                    return true;
                }
                catch
                {
                    // Don't care about any exceptions - if it doesn't exist then we can't remove it
                    return false;
                }
            }
            else
            {
                return (bool)Dispatcher.Invoke((Func<string, string, string, bool>)RemoveBotVersion, new object[] { OwnerName, BotName, Version });
            }
        }
        #endregion

        public string GetBotPath(string OwnerName, string BotName, string Version)
        {
            return Inner[OwnerName][BotName][Version];
        }

        private static List<string> Split(string Input, char Delimiter)
        {
            List<string> Results = new List<string>();

            string Temp = string.Empty;
            for (int x = 0; x < Input.Length; x++)
            {
                if (Input[x] == Delimiter)
                {
                    if (Temp.Length != 0)
                    {
                        Results.Add(Temp);
                        Temp = string.Empty;
                    }
                }
                else
                {
                    Temp += Input[x];
                }
            }
            if (Temp.Length != 0)
            {
                Results.Add(Temp);
            }

            return Results;
        }
    }
}
