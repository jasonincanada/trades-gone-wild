using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradesGoneWild.Domain;

namespace TradesGoneWild.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Instrument loonie = new Instrument
            {
                Market = "Forex",
                Symbol = "USDCAD"
            };

            Plan usdcadto4h21 = new Plan
            {
                Instrument = loonie,
                OpeningDate = DateTime.Parse("8/25/2015 23:50"),
                //ClosestPrice = 1.33105M,
                //OpeningPrice = 1.33105M,
                PriceTarget = 1.32309M, // TODO: this changes!
                TargetIsBelow = true
            };

            PlanManager mgr = new PlanManager();

            mgr.AddPlan(usdcadto4h21);
        }
    }
}
