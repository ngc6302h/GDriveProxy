using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GDriveProxy
{
    class Program
    {
        static int filesize;
        static Dictionary<string, int> links = new Dictionary<string, int>();
        static void Main(string[] args)
        {
            if (args.Length  < 3)
            {
                Console.WriteLine("arguments: first filesize in megabytes, then all links separated by a space\nPress any key to continue...");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            filesize = int.Parse(args[0]);
            foreach (string link in args.Skip(1).Take(args.Length-1))
            {
                links.Add(link, 50000);
            }

            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://*:15001/");
            httpListener.Start();
            while (true)
            {
                var context = httpListener.GetContext();
                Task.Run(() => HandleIncomingRequest(context));
            }
        }

        static void HandleIncomingRequest(HttpListenerContext context)
        {
            string chosen = null;
            foreach (KeyValuePair<string, int> pair in links)
            {
                if (pair.Value-filesize > 1000) // 1000 mb as safe margin?
                {
                    chosen = pair.Key;
                    links[pair.Key] -= filesize;
                    break;
                }
            }
            if (chosen == null)
            {
                chosen = "error.link";
            }
            context.Response.Redirect(chosen);
            context.Response.Close();
            Console.WriteLine($"Forwarded request to {chosen}   --- estimated quota left on link={links[chosen]}");
        }
    }
}
