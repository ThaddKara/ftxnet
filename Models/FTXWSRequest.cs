using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ftxnet.Models
{
    public class FTXWSRequest
    {
        [JsonProperty]
        public string market { get; set; }
        [JsonProperty]
        public Channel channel { get; set; }
        [JsonProperty]
        public Op op { get; set; }

        public FTXWSRequest(string market, Channel channel, Op op)
        {
            this.market = market;
            this.channel = channel;
            this.op = op;
        }

        // just flip the Op
        public static FTXWSRequest unsubscribe(FTXWSRequest request)
        {
            return new FTXWSRequest(request.market, request.channel, Op.unsubscribe);
        }
    }

    public enum Channel
    {
        orderbook,
        trades,
        ticker
    }

    public enum Op
    {
        subscribe,
        unsubscribe
    }
}
