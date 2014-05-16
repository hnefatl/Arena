using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Arena.UI
{
    public class BotList
        : TreeView
    {
        /* Maps to:
                            Owner Name -> Bot Name -> Version -> Bot Path
        */
        protected Dictionary<string, Dictionary<string, Dictionary<string, string>>> Inner { get; set; }

        public void AddBot(string OwnerName, string BotName, string Version, string BotPath)
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
                    if ((string)((TreeViewItem)Items[x]).Header == OwnerName)
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
                    OwnerItem.ToolTip = "Versions: " + (Convert.ToInt32(Split((string)OwnerItem.ToolTip, ' ')[1]) + 1); // Add one to the current bot number (have to extract it from the string)
                    #endregion
                }
                #region Find BotItem
                TreeViewItem BotItem = null;
                for (int x = 0; x < OwnerItem.Items.Count; x++)
                {
                    if ((string)((TreeViewItem)OwnerItem.Items[x]).Header == BotName)
                    {
                        BotItem = (TreeViewItem)Items[x];
                        break;
                    }
                }
                #endregion
                TreeViewItem VersionItem = null;
                if (!Inner[OwnerName][BotName].ContainsKey(Version))    // Doesn't exist already - add it
                {
                    Inner[OwnerName][BotName].Add(Version, BotPath);
                    #region Create and Insert VersionItem
                    VersionItem = new TreeViewItem();
                    BotItem.Items.Add(VersionItem);
                    #endregion

                    // Only update tooltips if we've added a new Version
                    #region Update Category ToolTips (Version)
                    BotItem.ToolTip = "Versions: " + (Convert.ToInt32(Split((string)BotItem.ToolTip, ' ')[1]) + 1); // Add one to the current version number (have to extract it from the string)
                    #endregion
                }
                else                                                    // Does exist already - overwrite it
                {
                    Inner[OwnerName][BotName][Version] = BotPath;
                }
                #region Find VersionItem
                if (VersionItem != null)
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
                VersionItem = new TreeViewItem()
                {
                    Header = Version,
                    ToolTip = BotPath,
                };

                #endregion
            }
            else
            {
                Dispatcher.Invoke((Action<string, string, string, string>)AddBot, new object[] { OwnerName, BotName, Version, BotPath });
            }
        }

        public void RemoveOwner(string OwnerName)
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
                }
                catch
                {
                    // Don't care about any exceptions - if it doesn't exist then we can't remove it
                }
            }
            else
            {
                Dispatcher.Invoke((Action<string>)RemoveOwner, OwnerName);
            }
        }
        public void RemoveBot(string OwnerName, string BotName)
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
                }
                catch
                {
                    // Don't care about any exceptions - if it doesn't exist then we can't remove it
                }
            }
            else
            {
                Dispatcher.Invoke((Action<string, string>)RemoveBot, new object[] { OwnerName, BotName });
            }
        }
        public void RemoveBotVersion(string OwnerName, string BotName, string Version)
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
                }
                catch
                {
                    // Don't care about any exceptions - if it doesn't exist then we can't remove it
                }
            }
            else
            {
                Dispatcher.Invoke((Action<string, string, string>)RemoveBotVersion, new object[] { OwnerName, BotName, Version });
            }
        }

        public string GetBot(string OwnerName, string BotName, string Version)
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
