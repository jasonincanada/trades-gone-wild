using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TradesGoneWild.Domain;

namespace TradesGoneWild.UI
{
    public partial class TradeConsole : Form
    {
        AlertManager _alertManager;
        TickPunker _tickPunker;

        public TradeConsole()
        {
            InitializeComponent();

            _alertManager = new AlertManager();
            _alertManager.ThrowEvent += (sender, args) => OnAlertEvent(args);
                        
            var repo = new Store();

            AllInstruments.SetInstruments(repo.GetInstruments());

            var alerts = repo.GetOpenAlerts();

            foreach (var alert in alerts)
            {
                var instrument = AllInstruments.GetInstrument(alert.Instrument.Market, alert.Instrument.Symbol);

                if (instrument == null)                
                    continue;

                Alert a = new Alert
                {
                    AlertID = alert.AlertID,
                    Instrument = instrument,
                    Finished = alert.DateClosed != null,
                    OpeningDate = alert.DateOpened,
                    PriceTarget = alert.PriceTarget,
                    TargetDirection = alert.TargetDirection
                };

                _alertManager.AddAlert(a);
            }                      

            EnumeratePlans();

            _tickPunker = new TickPunker();
            _tickPunker.ReceivedTick += new ReceivedTickEventHandler(GotTick);
            _tickPunker.StartWatcher();
                        
            AddTextOutput("Started");
        }

        private void GotTick(object sender, Tick tick)
        {
            _alertManager.NewTick(tick);

            Persist();
        }
                
        private void Persist()
        {
            if (DateTime.UtcNow < _nextPersist)
                return;

            AddTextOutput("Persist");

            // Put 1 minute between persists
            _nextPersist = DateTime.UtcNow.AddMinutes(1);

            var repo = new Store();

            foreach (var alert in _alertManager.Alerts())
                repo.UpdateAlert(alert);

            repo.SaveChanges();
        }
        private DateTime _nextPersist = DateTime.UtcNow.AddMinutes(1);

        /// <summary>
        /// Actions to take when an alert fires an event.
        /// </summary>
        /// <param name="args"></param>
        private void OnAlertEvent(AlertEventArgs args)
        {
            string message;

            switch (args.EventType)
            {
                case AlertEventType.CloserToTarget:
                    message = string.Format("{0}:{1} - {2} - Nearing to price {3}",
                                            args.Instrument.Market,
                                            args.Instrument.Symbol,
                                            args.Price,
                                            args.Alert.PriceTarget);
                    break;

                case AlertEventType.TargetReached:
                    message = string.Format("{0}:{1} - {2} - Target reached at {3}!",
                                            args.Instrument.Market,
                                            args.Instrument.Symbol,
                                            args.Price,
                                            args.Alert.PriceTarget);
                    break;

                default:
                     message = "Unknown message type " + args.EventType;
                     break;
            }

            AddTextOutput(message);
        }

        private void AddTextOutput(string message)
        {
            if (string.IsNullOrEmpty(message))
                txtOutput.AppendText(string.Format("\r\n"));
            else
                txtOutput.AppendText(string.Format("\r\n{0}: {1}", DateTime.UtcNow, message));
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Return)
                return;

            string command = txtCommand.Text.ToLower().Trim();

            Regex regex = new Regex(@"^w\s+([a-z]{6})\s+([0-9\.]+)$");
            Match match = regex.Match(command);

            if (match.Success == true)
            {
                string symbol = match.Groups[1].Value.ToUpper();
                decimal price = Convert.ToDecimal(match.Groups[2].Value);

                Instrument instrument = AllInstruments.GetInstrument("Forex", symbol);

                if (instrument == null)
                {
                    AddTextOutput("Unknown instrument " + instrument);
                }
                else
                {
                    var alert = new Alert
                    {
                        Instrument = instrument,
                        OpeningDate = DateTime.UtcNow,
                        PriceTarget = price
                    };

                    AddToStore(alert);

                    _alertManager.AddAlert(alert);

                    AddTextOutput("");
                    EnumeratePlans();
                }
            }
        }

        private void AddToStore(Alert alert)
        {
            var repo = new Store();

            repo.AddAlert(alert);
        }

        /// <summary>
        /// Write each known plan to the output window, oldest first
        /// </summary>
        private void EnumeratePlans()
        {   
            foreach (var plan in _alertManager.AllOrderedPlans())
                AddTextOutput(plan.ToString());
        }
    }
}
