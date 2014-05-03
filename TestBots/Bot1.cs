using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bot;

namespace TestBots
{
    public class Bot1
        : Bot.Bot
    {
        public Bot1()
        {
            Name = "Bot1";
            Version = "2.0";
            Owner = "Keith";
        }

        public override void Run()
        {
            Console.WriteLine("Run");
        }
    }
}
