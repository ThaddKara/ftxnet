using System;

namespace ftxnet.Models
{
    public class FTXWSConfig
    {
        public FTXWSConfig(string ftxkey, string ftxsecret)
        {
            this.ftxkey = ftxkey;
            this.ftxsecret = ftxsecret;
        }

        public string ftxkey { get; set; }
        public string ftxsecret { get; set; }

        public static string url { get; set; } = "wss://ftx.com/ws/";
    }
}