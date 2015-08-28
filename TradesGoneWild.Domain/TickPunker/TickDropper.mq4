//+------------------------------------------------------------------+
//|                                                  TickDropper.mq4 |
//|                                     Copyright 2013, Jason Hooper |
//+------------------------------------------------------------------+
#property copyright "Copyright 2013, Jason Hooper"
#property link      ""

string Symbols[] = {
   "EURUSD", 
   "GBPUSD",
   "USDCAD",
   "XAUUSD",
   "AUDUSD",
   "AUDJPY",
   "EURJPY",
   "GBPJPY",
   "USDJPY",
   "USDCHF"
};

int init()
{   
}

int deinit()
{
}

int start()
{
   for (int i = 0; i < ArraySize(Symbols); i++)
   {      
      double close = iClose(Symbols[i], PERIOD_M1, 0);
      datetime time = iTime(Symbols[i], PERIOD_M1, 0);      
      string fileName = Symbols[i] + ".tick";
            
      int fh = FileOpen(fileName, FILE_CSV | FILE_WRITE, ',');
      
      if (fh > 0) {
         FileWrite(fh, "Forex:" + Symbols[i], time, close);
         FileClose(fh);      
      } else {
         Print("Error: ", GetLastError());
      }
   }   
   
   return(0);
}

