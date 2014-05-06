using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public abstract class Bot
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Owner { get; set; }

        public Bot()
        {

        }

        public void Launch(string[] args)
        {
            if(args.Contains("OutputInfo"))
            {
                Console.WriteLine(Name);
                Console.WriteLine(Version);
                Console.WriteLine(Owner);
                Environment.Exit(0);
            }
            else
            {
                Run();
            }
        }
        public abstract void Run();
    }
}
