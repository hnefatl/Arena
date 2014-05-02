using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Arena.Logic
{
    public class BotInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Owner { get; set; }
    }

    public class Bot
        : BotInfo
    {
        protected Process Sub { get; set; }

        public Bot()
        {
        }

        public bool Initialise(string Filename)
        {
            if (!File.Exists(Filename))
            {
                return false;
            }

            Sub = new Process();
            Sub.StartInfo.FileName = Filename;
            Sub.StartInfo.RedirectStandardInput = true;
            Sub.StartInfo.RedirectStandardOutput = true;
            Sub.StartInfo.CreateNoWindow = true;

            return true;
        }

        public void Start()
        {
            Sub.Start();

            Name = Read();
            Version = Read();
            Owner = Read();
        }

        public void Write(string Data)
        {
            try
            {
                Sub.StandardInput.WriteLine(Data);
            }
            catch (NullReferenceException)
            {
                throw new Exception("Must call Bot.Initialise before reading or writing.");
            }
            catch
            {
                throw new Exception("Failed to write data.");
            }
        }
        public string Read()
        {
            try
            {
                return Sub.StandardOutput.ReadLine();
            }
            catch (NullReferenceException)
            {
                throw new Exception("Must call Bot.Initialise before reading or writing.");
            }
            catch
            {
                throw new Exception("Failed to read data.");
            }
        }
    }
}
