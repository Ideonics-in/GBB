using ias.andonmanager;
using Printer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TestBenchApp.DashBoard;
using TestBenchApp.Entity;



namespace TestBenchApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            List<LogEntry> log;
            LogEntry le;
            if (ControllerSimulation)
            {
                switch (e.Key)
                {
                    case Key.F1:
                        AddToQ("FSerial");
                        break;
                   
                }
            }

            if (ScannerSimulation)
            {
                switch (e.Key)
                {
                    case Key.F2:
                        if (FCodeQ.Count > 0)
                        {
                            String code = FCodeQ.Dequeue();
                            andonManager_barcodeAlertEvent(this, new BCScannerEventArgs(code));
                            
                            
                        }
                        break;

                    case Key.F3:

                        if (CCodeQ.Count > 0)
                        {

                            String code = CCodeQ.Dequeue();
                            andonManager_combStickerAlertEvent(this, new CSScannerEventArgs(code));
                        }

                        break;

        
                }
            }





        }

    }
}
