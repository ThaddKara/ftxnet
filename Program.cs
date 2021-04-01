using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using ftxnet.Models;
using ftxnet.Services;

namespace ftxnet
{
    class Program
    {

        // example implementation of ftxsocketclient
        static async Task Main(string[] args)
        {
            FTXWSConfig config = new FTXWSConfig("key", "secret");
            FTXSocketClient socket = new FTXSocketClient(config);

            FTXWSRequest btctrades = new FTXWSRequest("BTC-PERP", Channel.trades, Op.subscribe);
            FTXWSRequest btcorderbook = new FTXWSRequest("BTC-PERP", Channel.orderbook, Op.subscribe);
            FTXWSRequest btcticker = new FTXWSRequest("BTC-PERP", Channel.ticker, Op.subscribe);

            List<FTXWSRequest> unsubreqs = new List<FTXWSRequest>();

            unsubreqs.Add(FTXWSRequest.unsubscribe(btctrades));
            unsubreqs.Add(FTXWSRequest.unsubscribe(btcorderbook));
            unsubreqs.Add(FTXWSRequest.unsubscribe(btcticker));

            socket.connect();

            await Task.Delay(5000);//should tidy this up

            socket.SendMessage(btctrades);
            socket.SendMessage(btcorderbook);
            socket.SendMessage(btcticker);

            Thread.Sleep(1000);

            //unsub
            unsubreqs.ForEach(e => socket.SendMessage(e));

            Console.ReadLine();
        }
    }
}
