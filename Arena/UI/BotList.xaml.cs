using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Arena.UI
{
    public partial class BotList
        : TreeView
    {
        /* Maps to:
                            Owner Name -> Bot Name -> Version -> Bot Path
        */
        protected Dictionary<string, Dictionary<string, Dictionary<string, string>>> Inner { get; set; }

        #region ContextMenus
        public ContextMenu BaseContextMenu { get; set; }
        public ContextMenu OwnerContextMenu { get; set; }
        public ContextMenu BotContextMenu { get; set; }
        public ContextMenu VersionContextMenu { get; set; }
        #endregion

        /// <summary>
        /// NOTE: It is the user's responsibilty to fill in the ContextMenu controls
        /// </summary>
        public BotList()
        {
            InitializeComponent();

            Inner = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        }

        public void SetContextMenus(ContextMenu BaseContextMenu, ContextMenu OwnerContextMenu, ContextMenu BotContextMenu, ContextMenu VersionContextMenu)
        {
            this.BaseContextMenu = BaseContextMenu;
            this.OwnerContextMenu = OwnerContextMenu;
            this.BotContextMenu = BotContextMenu;
            this.VersionContextMenu = VersionContextMenu;

            ContextMenu = BaseContextMenu;
        }

        #region Add Methods
        /// <summary>
        /// Adds an Owner to the view
        /// </summary>
        /// <returns>True if an Owner was addded, else False</returns>
        public bool AddOwner(string OwnerName)
        {
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
                    Items.Add(new TreeViewItem()
                    {
                        Header = OwnerName,
                        ContextMenu = OwnerContextMenu,
                    });
                    return true;
                }
                return false;

                #endregion
            }
            else
            {
                return (bool)Dispatcher.Invoke((Func<string, bool>)AddOwner, new object[] { OwnerName });
            }
        }
        /// <summary>
        /// Adds a Bot to the view
        /// </summary>
        /// <returns>True if a Bot was addded, else False</returns>
        public bool AddBot(string OwnerName, string BotName)
        {
            if (Dispatcher.CheckAccess())
            {
                #region Sanity check - Arguments aren't empty
                if (OwnerName == string.Empty || BotName == string.Empty)
                {
                    throw new Exception("At least one of the arguments was empty.");
                }
                #endregion

                #region Add Bots

                if (!Inner.ContainsKey(OwnerName))
                {
                    Inner.Add(OwnerName, new Dictionary<string, Dictionary<string, string>>());
                    Items.Add(new TreeViewItem()
                    {
                        Header = OwnerName,
                    });
                }
                #region Find OwnerItem
                TreeViewItem OwnerItem = null;
                for (int x = 0; x < Items.Count; x++)
                {
                    if ((string)(((TreeViewItem)Items[x]).Header) == OwnerName)
                    {
                        OwnerItem = (TreeViewItem)Items[x];
                        break;
                    }
                }
                #endregion
                if (!Inner[OwnerName].ContainsKey(BotName))
                {
                    Inner[OwnerName].Add(BotName, new Dictionary<string, string>());
                    OwnerItem.Items.Add(new TreeViewItem()
                    {
                        Header = BotName,
                        ContextMenu = BotContextMenu,
                    });
                    // Only update tooltips if we've added a new Bot
                    #region Update Category ToolTips (Bot)
                    if (OwnerItem.ToolTip != null)
                    {
                        OwnerItem.ToolTip = "Bots: " + (Convert.ToInt32(Split((string)OwnerItem.ToolTip, ' ')[1]) + 1); // Add one to the current bot number (have to extract it from the string)
                    }
                    else
                    {
                        OwnerItem.ToolTip = "Bots: 1";
                    }
                    #endregion

                    return true;
                }
                return false;

                #endregion
            }
            else
            {
                return (bool)Dispatcher.Invoke((Func<string, string, bool>)AddBot, new object[] { OwnerName, BotName });
            }
        }
        /// <summary>
        /// Adds a Version to the view
        /// </summary>
        /// <returns>True if a Version was addded, else False</returns>
        public bool AddBotVersion(string OwnerName, string BotName, string Version, string BotPath)
        {
            if (Dispatcher.CheckAccess())
            {
                #region Sanity check - Arguments aren't empty
                if (OwnerName == string.Empty || BotName == string.Empty || Version == string.Empty || BotPath == string.Empty)
                {
                    throw new Exception("At least one of the arguments was empty.");
                }
                #endregion

                #region Add Bots

                if (!Inner.ContainsKey(OwnerName))
                {
                    Inner.Add(OwnerName, new Dictionary<string, Dictionary<string, string>>());
                    Items.Add(new TreeViewItem()
                    {
                        Header = OwnerName,
                    });
                }
                #region Find OwnerItem
                TreeViewItem OwnerItem = null;
                for (int x = 0; x < Items.Count; x++)
                {
                    if ((string)(((TreeViewItem)Items[x]).Header) == OwnerName)
                    {
                        OwnerItem = (TreeViewItem)Items[x];
                        break;
                    }
                }
                #endregion
                if (!Inner[OwnerName].ContainsKey(BotName))
                {
                    Inner[OwnerName].Add(BotName, new Dictionary<string, string>());
                    OwnerItem.Items.Add(new TreeViewItem()
                    {
                        Header = BotName,
                    });
                    // Only update tooltips if we've added a new Bot
                    #region Update Category ToolTips (Bot)
                    if (OwnerItem.ToolTip != null)
                    {
                        OwnerItem.ToolTip = "Bots: " + (Convert.ToInt32(Split((string)OwnerItem.ToolTip, ' ')[1]) + 1); // Add one to the current bot number (have to extract it from the string)
                    }
                    else
                    {
                        OwnerItem.ToolTip = "Bots: 1";
                    }
                    #endregion
                }
                #region Find BotItem
                TreeViewItem BotItem = null;
                for (int x = 0; x < OwnerItem.Items.Count; x++)
                {
                    if ((string)(((TreeViewItem)OwnerItem.Items[x]).Header) == BotName)
                    {
                        BotItem = (TreeViewItem)OwnerItem.Items[x];
                        break;
                    }
                }
                #endregion
                bool AddedVersionItem = false;
                TreeViewItem VersionItem = new TreeViewItem();
                if (!Inner[OwnerName][BotName].ContainsKey(Version))    // Doesn't exist already - add it
                {
                    Inner[OwnerName][BotName].Add(Version, BotPath);
                    #region Create and Insert VersionItem
                    VersionItem = new TreeViewItem();
                    BotItem.Items.Add(VersionItem);
                    #endregion

                    // Only update tooltips if we've added a new Version
                    #region Update Category ToolTips (Version)
                    if (BotItem.ToolTip != null)
                    {
                        BotItem.ToolTip = "Versions: " + (Convert.ToInt32(Split((string)OwnerItem.ToolTip, ' ')[1]) + 1); // Add one to the current bot number (have to extract it from the string)
                    }
                    else
                    {
                        BotItem.ToolTip = "Versions: 1";
                    }
                    #endregion

                    AddedVersionItem = true;
                }
                else                                                    // Does exist already - overwrite it
                {
                    Inner[OwnerName][BotName][Version] = BotPath;
                }
                #region Find VersionItem
                if (!AddedVersionItem)
                {
                    for (int x = 0; x < BotItem.Items.Count; x++)
                    {
                        if ((string)((TreeViewItem)BotItem.Items[x]).Header == Version)
                        {
                            VersionItem = (TreeViewItem)BotItem.Items[x];
                        }
                    }
                }
                #endregion
                VersionItem.Header = Version;
                VersionItem.ToolTip = "Path: " + BotPath;
                VersionItem.ContextMenu = VersionContextMenu;

                return AddedVersionItem;

                #endregion
            }
            else
            {
                return (bool)Dispatcher.Invoke((Func<string, string, string, string, bool>)AddBotVersion, new object[] { OwnerName, BotName, Version, BotPath });
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
