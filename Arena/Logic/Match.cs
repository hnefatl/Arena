using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arena.Logic
{
    public abstract class Match
    {
        protected List<Bot> Bots { get; set; }

        public Match()
        {
            Bots = new List<Bot>();
        }

        public abstract BotInfo Run();

        public virtual void LoadBot(string Filename)
        {
            Bots.Add(new Bot());
            Bots[Bots.Count - 1].Initialise(Filename);
        }
        public void LoadBots(List<string> Filenames)
        {
            foreach (string Filename in Filenames)
            {
                LoadBot(Filename);
            }
        }

        public virtual void StartBots()
        {
            for (int x = 0; x < Bots.Count; x++)
            {
                Bots[x].Start();
            }
        }
    }
}
