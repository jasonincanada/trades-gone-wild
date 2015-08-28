using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TradesGoneWild.Domain
{
    public delegate void ReceivedTickEventHandler(object sender, Tick tick);

    /// <summary>
    /// Watches for and parses *.tick files dropped by our TickDropper.mq4 expert advisor which is outputting them as files in CSV format
    /// </summary>
    public class TickPunker
    {
        //const string TickDropperPath = @"C:\Users\Jason\AppData\Local\VirtualStore\Program Files (x86)\OANDA - MetaTrader\experts\files";
        const string TickDropperPath = @"C:\Users\Jason\AppData\Roaming\MetaQuotes\Terminal\3212703ED955F10C7534BE8497B221F4\MQL4\Files";
        private FileSystemWatcher _watcher;
        private Dictionary<string, double> _lastKnownPrices;

        public event ReceivedTickEventHandler ReceivedTick;

        public TickPunker()
        {
            StartWatcher();

            _lastKnownPrices = new Dictionary<string, double>();
        }

        public void StopWatcher()
        {
            if (_watcher != null)
                _watcher.EnableRaisingEvents = false;
        }

        public void StartWatcher()
        {
            if (_watcher != null)
                return;

            _watcher = new FileSystemWatcher()
            {
                Path = TickDropperPath,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.tick"
            };

            _watcher.Changed += new FileSystemEventHandler(WatcherChanged);
            _watcher.EnableRaisingEvents = true;
        }

        void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                string contents = File.ReadAllText(e.FullPath);

                Regex regex = new Regex(@"(.+):(.+),(.+),([\d.]+)");
                Match match = regex.Match(contents);

                if (match.Success == true)
                {
                    string matchMarket = match.Groups[1].Value;
                    string matchSymbol = match.Groups[2].Value;
                    string matchDate = match.Groups[3].Value;
                    string matchPrice = match.Groups[4].Value;

                    DateTime date = new DateTime(1970, 1, 1, 0, 0, 0);
                    date = date.AddSeconds(Convert.ToInt32(matchDate));

                    string symbol = matchSymbol;
                    double price = Convert.ToDouble(matchPrice);

                    // Only report this tick value once then wait until it changes                    
                    if (!_lastKnownPrices.ContainsKey(symbol))
                        _lastKnownPrices.Add(symbol, 0.0);

                    if (_lastKnownPrices[symbol] != price)
                    {
                        _lastKnownPrices[symbol] = price;

                        Tick tick = new Tick
                        {
                            Price = Convert.ToDecimal(price),
                            Date = date,
                            Instrument = AllInstruments.GetInstrument(matchMarket, matchSymbol)
                        };

                        if (ReceivedTick != null)
                            ReceivedTick(this, tick);
                    }
                }
            }
            catch (Exception)
            {
                //Console.WriteLine("Error: {0}", ex.Message);
            }
        }
    }
}
