using System;
using System.Collections.Generic;
using System.Threading;
using ChartIQ.Finsemble;
using Newtonsoft.Json.Linq;

namespace ConsoleAppExample
{
    class Program
    {
        private static readonly System.Timers.Timer timer = new System.Timers.Timer();
        private static readonly AutoResetEvent autoEvent = new AutoResetEvent(false);
        private static readonly object lockObj = new object();

        private static Finsemble FSBL = null;

        static void Main(string[] args)
        {
            //string json = "[{ 'Name': 'Jon Smith', 'City': 'New York', 'Age': 42 }]";
            //var myjson = JToken.Parse(json);
            // Initialize Finsemble
            FSBL = new Finsemble(args, null);
            FSBL.Connect();
            FSBL.Connected += OnConnected;
            FSBL.Disconnected += OnShutdown;
            StartPushingMessage();
            
            
            // Block main thread until worker is finished.
            autoEvent.WaitOne();
        }

        private static void StartPushingMessage()
        {
            // Send log message every 5 seconds
            timer.Interval = 5 * 1000;
            timer.AutoReset = true;
            string json = @"[{ 'Name': 'Jon Smith', 'City': 'New York', 'Age': 42 }]";
            timer.Elapsed += (s1, e1) => FSBL.RouterClient.Transmit("Testing", JToken.Parse(json));
            Console.WriteLine("Message fired");
            timer.Start();

        }

        private static void OnConnected(object sender, EventArgs e)
        {
            //FSBL.RPC("Logger.log", new List<JToken> { "Windowless example connected to Finsemble." });

            // Send log message every 5 seconds
            timer.Interval = 5 * 1000;
            timer.AutoReset = true;
            //timer.Elapsed += (s1, e1) => FSBL.RPC("Logger.log", new List<JToken> { string.Format("Windowless example elapsed event was raised at {0}", e1.SignalTime) });
            string json = @"[{ 'Name': 'Jon Smith', 'City': 'New York', 'Age': 42 }]";
            timer.Elapsed += (s1, e1) => FSBL.RouterClient.Transmit("Testing", JToken.Parse(json));
            timer.Start();
            
            //FSBL.LinkerClient.PublishToChannel("pratik",JObject)
            //FSBL.RouterClient.Publish


        }

        private static void OnShutdown(object sender, EventArgs e)
        {
            if (FSBL != null)
            {
                lock (lockObj)
                {
                    // Disable log timer
                    timer.Stop();

                    if (FSBL != null)
                    {
                        try
                        {
                            // Dispose of Finsemble.
                            FSBL.Dispose();
                        }
                        catch { }
                        finally
                        {
                            FSBL = null;
                        }
                    }

                }
            }

            // Release main thread so application can exit.
            autoEvent.Set();
        }
    }
}