using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

using WebSocketSharp;

using ftxnet.Models;

namespace ftxnet.Services
{
    class FTXSocketClient
    {
        private FTXWSConfig config;
        private static WebSocket ws = new WebSocket(url: FTXWSConfig.url);

        protected static decimal currentsec { get; set; } = decimal.Truncate((decimal)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        protected static int currentcount { get; set; } = 0;

        private static ManualResetEvent exitEvent = new ManualResetEvent(false);

        public FTXSocketClient(FTXWSConfig config)
        {
            this.config = config;
        }

        // connect and setup -> run forever or until disconnect() called
        public async Task connect(bool auth = false)
        {
            //int backoff = 1;
            //TODO: add rate limit and infinite loop with exponential backoff
            while (true)
            {
                try
                {
                    using (var w = ws)
                    {
                        w.OnMessage += async (sender, e) => await OnMessage(e.Data, e);
                        w.OnError += async (sender, e) => await OnError(e.Message);
                        w.OnOpen += (sender, e) => Console.WriteLine("socket success");

                        await Task.Run(() =>
                        {
                            w.Connect();
                        });

                        //TODO: auth for private endpoints
                        if (auth) { }

                        Ping(15000); // pinger

                        exitEvent.WaitOne(); // wait till break
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine($"error: {e.Message}\nstacktrace: {e.StackTrace}");
                }
                Task.Delay(2000).Wait(); // constant backoff
            }
        }

        // interface

        /// <summary>
        /// should accept model containing...
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void SendMessage(FTXWSRequest request)
        {
            if (RateLimiter())
            {
                ws.Send(JsonConvert.SerializeObject(new
                {
                    channel = request.channel.ToString(),
                    market = request.market,
                    op = request.op.ToString(),
                }));
            }
            else return;
        }

        // privates

        /// <summary>
        /// "Send pings at regular intervals (every 15 seconds)"
        /// </summary>
        /// <returns></returns>
        private static async Task Ping(int milliseconds)
        {
            while (true)
            {
                await Task.Delay(milliseconds);
                ws.Send("{\"op\":\"ping\"}"); // will this count towards ratelimit?
                //ws.Ping();
            }
        }

        private static Task OnError(string message)
        {
            Console.Write("Message received: {0}\n", message);
            return null;
        }

        private static Task OnMessage(string message, MessageEventArgs e)
        {
            // TODO: **** gotta dig into the json parsing a bit more ****
            Console.Write("Message received: {0}\n", message);
            return Task.FromResult(0);
        }

        // currently will just block sends, but should queue(TODO)
        // returns true if valid rate check, false if exceeding limit (30 requests per second)
        private static bool RateLimiter(int ratelimit = 30)
        {
            decimal unixTimestamp = decimal.Truncate((decimal)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            if (unixTimestamp == currentsec) // iterate counter
            {
                currentcount += 1;
                if (currentcount >= ratelimit) // block send
                {
                    return false;
                }
                else // under limit
                {
                    return true;
                }
            }
            else // new second, refresh counter to 1
            {
                currentsec = unixTimestamp;
                return true;
            }
        }
    }
}