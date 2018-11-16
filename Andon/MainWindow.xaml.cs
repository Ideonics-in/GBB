using ias.andonmanager;
using Printer;
using shared;
using shared.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Net;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TestBenchApp.DashBoard;
using TestBenchApp.Entity;
using ModbusTCP;
using System.Collections.Concurrent;
using System.IO.Ports;
using EasyModbus;


namespace TestBenchApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AndonManager andonManager = null;

        String _dbConnectionString = String.Empty;
        String _testdbConnectionString = String.Empty;
        DataAccess dataAccess = null;
        Queue<int> deviceQ = null;
        String[] comLayers;
        AndonManager.MODE Mode = AndonManager.MODE.NONE;
        Users Users;
        User CurrentUser;

        PrinterManager PrinterManager;

        SerialPort TestJigPort;
        string TestJigComPort;

        List<Plan> FramePlans, BodyPlans;
        ConcurrentDictionary<String, Plan> FramePlanDictionary = new ConcurrentDictionary<string, Plan>();
        ConcurrentDictionary<String, Plan> BodyPlanDictionary = new ConcurrentDictionary<string, Plan>();

        ObservableCollection<Plan> FramePlanList = new ObservableCollection<Plan>();
        ObservableCollection<Plan> BodyPlanList = new ObservableCollection<Plan>();

        ObservableCollection<LogRecord> Log = new ObservableCollection<LogRecord>();
        ObservableCollection<TestRecord> TestLog = new ObservableCollection<TestRecord>();


        ConcurrentQueue<Boolean> FSerialQ, MSerialQ;
        ConcurrentQueue<LogRecord> LogQ;
        ConcurrentQueue<TestRecord> TestRecordQ;

        ObservableCollection<Model> Models;
        Dictionary<String,Model> ModelDictionary;

        Plan CurrentFramePlan = null;
        Plan CurrentBodyPlan = null;


        Timer PerformanceTestTimer;
        Timer tickTimer, modbusTimer;
        Timer F2Error1, F2Error2;
        Timer IError;
        Timer FGSuccess;
        Timer FGFailure;
        Timer FGProcess;
        Timer MainFrameFunctionalTestFailure;
        Timer MainFrameFunctionalTestSuccess;
        Timer LogTimer;

        bool FBypass = false;
        bool CBypass = false;

        Queue<String> FCodeQ;
        Queue<String> BCodeQ;
        Queue<String> ICodeQ;
        Queue<String> CCodeQ;
        Queue<String> ACodeQ;

        bool PrinterSimulation = false;

        bool ScannerSimulation = false;
        bool ControllerSimulation = false;

        bool Startup = true;

        DashBoardView dbView;

        List<UnitAssociation> Associations;
        int AssociationTimeout;

        int f1PrintCount = 0;
        int m1PrintCount = 0;

        string F1BarcodeFile = String.Empty;
        string M1BarcodeFile = String.Empty;
        string IntegratedBarcodeFile = String.Empty;

        string DummyF1BarcodeFile = String.Empty;
        string DummyM1BarcodeFile = String.Empty;
        string DummyIntegratedBarcodeFile = String.Empty;

        string templatePath = String.Empty;

        string CSDataFile = String.Empty;

        bool waitingforRO = false;
        String latestCombinationCode = String.Empty;

        Master modbusMaster = null;

        bool F1pressed = false;
        int F1Checkcount = 0;

        bool M1pressed = false;
        int M1Checkcount = 0;

        private Object FGLock = new object();

        private Object F2Lock = new object();
        private Object F3Lock = new object();
        System.Threading.AutoResetEvent TestOverEvent;

        BackgroundWorker TestWorker;
        int TEST_TIMEOUT;

        event EventHandler<PerformanceTestEventArgs> PerformanceTestEvent;
        event EventHandler<PerformanceTestEventArgs> PerformanceTestTimeoutEvent;

        public MainWindow()
        {
            InitializeComponent();

            

          

            _dbConnectionString = ConfigurationSettings.AppSettings["DBConStr"];
            _testdbConnectionString = ConfigurationSettings.AppSettings["TestDBConStr"];

            DataAccess.conStr = _dbConnectionString;
            DataAccess.TestDBConnectionString = _testdbConnectionString;
            dataAccess = new DataAccess();

            String mode = ConfigurationSettings.AppSettings["MODE"];
            Mode = (mode == "MASTER") ? AndonManager.MODE.MASTER : AndonManager.MODE.SLAVE;

            comLayers = ConfigurationSettings.AppSettings["COM_LAYERS"].Split(',');

            String combPrinterName = ConfigurationSettings.AppSettings["COMBINATION_BARCODE_PRINTER_NAME"];

            deviceQ = dataAccess.getDeviceQ();
            andonManager = new AndonManager(deviceQ, null, Mode);
            andonManager.andonAlertEvent += andonManager_andonAlertEvent;

            //Code added on 11 Nov
            andonManager.barcodeAlertEvent += andonManager_barcodeAlertEvent;
            andonManager.combStickerAlertEvent += andonManager_combStickerAlertEvent;
            andonManager.actQtyAlertEvent += andonManager_actQtyAlertEvent;


            TestJigComPort = ConfigurationSettings.AppSettings["TESTJIGPORT"];
            TestJigPort = new SerialPort(TestJigComPort);

            int port = Convert.ToInt32(ConfigurationSettings.AppSettings["PRINTER_PORT"]);
            IPAddress F1PrinterIPAddr = IPAddress.Parse(ConfigurationSettings.AppSettings["F1_PRINTER_IP"]);
            IPAddress M1PrinterIPAddr = IPAddress.Parse(ConfigurationSettings.AppSettings["M1_PRINTER_IP"]);
            IPAddress TOKPrinterIPAddr = IPAddress.Parse(ConfigurationSettings.AppSettings["TOK_PRINTER_IP"]);
            String PLCIPAddr = ConfigurationSettings.AppSettings["PLC_IP"];



            F1BarcodeFile = ConfigurationSettings.AppSettings["F1_BARCODE_TEMPLATE"];
            M1BarcodeFile = ConfigurationSettings.AppSettings["M1_BARCODE_TEMPLATE"];
            IntegratedBarcodeFile = ConfigurationSettings.AppSettings["INTEGRATED_BARCODE_TEMPLATE"];

            DummyF1BarcodeFile = ConfigurationSettings.AppSettings["DUMMY_F1_BARCODE_TEMPLATE"];
            DummyM1BarcodeFile = ConfigurationSettings.AppSettings["DUMMY_M1_BARCODE_TEMPLATE"];
            DummyIntegratedBarcodeFile = ConfigurationSettings.AppSettings["DUMMY_INTEGRATED_BARCODE_TEMPLATE"];

            templatePath = ConfigurationSettings.AppSettings["TEMPLATE_PATH"];

            CSDataFile = ConfigurationSettings.AppSettings["CS_BARCODE_TEMPLATE"];


            Log = dataAccess.GetTodayLog();
            LogGrid.DataContext = Log;

            TestRecordQ = new ConcurrentQueue<TestRecord>();
            TestLog = dataAccess.GetTodayTestRecords();
            TestLogGrid.DataContext = TestLog;

            Models = dataAccess.GetModels();
            ModelDictionary = new Dictionary<string,Model>();
            foreach (Model m in Models)
            {
                if (!ModelDictionary.ContainsKey(m.Code))
                    ModelDictionary.Add(m.Code, m);
            }

            tickTimer = new Timer(500);
            tickTimer.AutoReset = false;
            tickTimer.Elapsed += tickTimer_Elapsed;

            modbusTimer = new Timer(750);
            modbusTimer.AutoReset = false;
            modbusTimer.Elapsed += modbusTimer_Elapsed;

            F2Error1 = new Timer(2000);
            F2Error1.AutoReset = false;
            F2Error1.Elapsed += F2Error1_Elapsed;

            F2Error2 = new Timer(1000);
            F2Error2.AutoReset = false;
            F2Error2.Elapsed += F2Error2_Elapsed;


            IError = new Timer(1000);
            IError.AutoReset = false;
            IError.Elapsed += IError_Elapsed;

            FGSuccess = new Timer(1000);
            FGSuccess.AutoReset = false;
            FGSuccess.Elapsed += FGSuccess_Elapsed;

            FGFailure = new Timer(1000);
            FGFailure.AutoReset = false;
            FGFailure.Elapsed += FGFailure_Elapsed;


            FGProcess = new Timer(30000);
            FGProcess.AutoReset = false;
            FGProcess.Elapsed += FGProcess_Elapsed;


            MainFrameFunctionalTestFailure = new Timer(3000);
            MainFrameFunctionalTestFailure.AutoReset = false;
            MainFrameFunctionalTestFailure.Elapsed += MainFrameFunctionalTestFailure_Elapsed;

            MainFrameFunctionalTestSuccess = new Timer(1000);
            MainFrameFunctionalTestSuccess.AutoReset = false;
            MainFrameFunctionalTestSuccess.Elapsed += MainFrameFunctionalTestSuccess_Elapsed;

            LogQ = new ConcurrentQueue<LogRecord>();
            LogTimer = new Timer(1000);
            LogTimer.AutoReset = false;
            LogTimer.Elapsed += LogTimer_Elapsed;


            PerformanceTestTimer = new Timer(50000);
            PerformanceTestTimer.AutoReset = false;
            PerformanceTestTimer.Elapsed += PerformanceTestTimer_Elapsed;


            Associations = new List<UnitAssociation>();

            if (ConfigurationSettings.AppSettings["CONTROLLER_SIMULATION"] == "Yes")
            {
                ControllerSimulation = true;
            }

            if (ConfigurationSettings.AppSettings["SCANNER_SIMULATION"] == "Yes")
            {
                ScannerSimulation = true;
                FCodeQ = new Queue<string>();
                BCodeQ = new Queue<string>();
                ICodeQ = new Queue<string>();
                CCodeQ = new Queue<string>();
                ACodeQ = new Queue<string>();

            }

            if (ConfigurationSettings.AppSettings["PRINTER_SIMULATION"] == "Yes")
            {
                PrinterSimulation = true;

            }

            if (PrinterSimulation || ScannerSimulation || ControllerSimulation)
            {
                BaseWindow.KeyDown += Window_KeyDown;
            }

            if (!PrinterSimulation)
            {

                PrinterManager = new Printer.PrinterManager(templatePath);
                PrinterManager.SetupDriver("S11", F1PrinterIPAddr, port, F1BarcodeFile);
                PrinterManager.SetupDriver("M1Printer", M1PrinterIPAddr, port, M1BarcodeFile);
                PrinterManager.SetupDriver("F2Printer", TOKPrinterIPAddr, port, IntegratedBarcodeFile);
                PrinterManager.CombinationPrinterName = combPrinterName;
                PrinterManager.CombinationTemplate = CSDataFile;

            }

         

            AssociationTimeout = Convert.ToInt32(ConfigurationSettings.AppSettings["ASSOCIATION_TIMEOUT"]);

           
            FSerialQ = new ConcurrentQueue<Boolean>();
            MSerialQ = new ConcurrentQueue<Boolean>();

            SetupPlan();

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                  new Action(() =>
                                  {
                                      MainFramePlanGrid.DataContext = FramePlanList;
                                  }));


            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                             new Action(() =>
                             {
                                 MainBodyPlanGrid.DataContext = BodyPlanList;
                             }));

            if (!ScannerSimulation)
            {
                andonManager.OpenBCScanner();
                andonManager.OpenCSScanner();
            }

            if (!ControllerSimulation)
            {
                andonManager.start();
                modbusMaster = new Master(PLCIPAddr, (ushort)502);
                modbusMaster.OnResponseData += new ModbusTCP.Master.ResponseData(MBmaster_OnResponseData);
                modbusMaster.OnException += new ModbusTCP.Master.ExceptionData(MBmaster_OnException);
                modbusTimer.Start();

            }

            TestOverEvent = new System.Threading.AutoResetEvent(false);
            TestWorker = new BackgroundWorker();
            TestWorker.WorkerSupportsCancellation = true;
            TestWorker.DoWork += TestWorker_DoWork;
            TestWorker.RunWorkerCompleted += TestWorker_RunWorkerCompleted;

            PerformanceTestEvent += MainWindow_PerformanceTestEvent;
            PerformanceTestTimeoutEvent += MainWindow_PerformanceTestTimeoutEvent;

            TEST_TIMEOUT = Convert.ToInt32(ConfigurationSettings.AppSettings["TEST_TIMEOUT"]);

            LogTimer.Start();
            tickTimer.Start();
           
        }

        private void MainWindow_PerformanceTestTimeoutEvent(object sender, PerformanceTestEventArgs e)
        {
            
        }

        private void PerformanceTestTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TestWorker.CancelAsync(); 
        }

        private void MainWindow_PerformanceTestEvent(object sender, PerformanceTestEventArgs e)
        {
            UnitAssociation u = e.Unit;
            /*
            TestJigPort = new SerialPort(TestJigComPort);
            TestJigPort.Open();
            byte[] ReadPacket = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x1E, 0xC5, 0xC2 };
            TestResponse ts = new TestResponse();

            TestJigPort.Write(ReadPacket, 0, 8);

            TestJigPort.Read(ts.ResponsePacket, 0, 65);

            TestJigPort.Close();

            int TestResult = ts.GetStatus();

            if (TestResult == 3)
            {
                LogMessage("F2", "Peformance Test Passed", u.FCode);
                dataAccess.InsertUnitAssociation(u.Model, u.FCode, Model.Type.FRAME);
                if (!ControllerSimulation)
                    IndicateMainFrameFunctionalTestSuccess();
            }

            else
            {

                LogMessage("F2", "Perfomance Test Failed", u.FCode);
                if (!ControllerSimulation)
                    IndicateMainFrameFunctionalTestFailure();
            }

            TestRecord tr = ts.ParseResponse(u.FCode, u.Model);
            TestRecordQ.Enqueue(tr);
            dataAccess.InsertTestRecord(tr);
            */

        }

        private void TestWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            { 
                PerformanceTestTimeoutEvent?.Invoke(this, new PerformanceTestEventArgs(null));
                return;
            }


            PerformanceTestEvent?.Invoke(this, new PerformanceTestEventArgs((UnitAssociation)e.Result));



        }

        private void TestWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int[] result = { 0 };
            UnitAssociation u = (UnitAssociation)e.Argument;
            ModbusClient modbusClient = new ModbusClient("172.20.241.201", 502);
            modbusClient.Connect();

            List<TestResponse> ResponseList = new List<TestResponse>();

            BackgroundWorker w = (BackgroundWorker)sender;
            TestJigPort = new SerialPort(TestJigComPort);
            TestJigPort.Open();
            byte[] ReadPacket = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x1E, 0xC5, 0xC2 };

            int res = 0;

            
            TestJigPort.ReadTimeout = 5000;
            do
            {
                if (w.CancellationPending)
                {
                    e.Result = null;
                    modbusClient.Disconnect();
                    TestJigPort.Close();
                    LogMessage("S2", "Test Timed Out", u.FCode);
                    return;
                }

                TestJigPort.Write(ReadPacket, 0, 8);

                TestResponse ts = new TestResponse();
                try
                {
                    TestJigPort.Read(ts.ResponsePacket, 0, 65);
                   
                }
                catch(System.TimeoutException ex)
                {
                    continue;
                }

                
                ResponseList.Add(ts);

                res = ts.GetStatus();

                result = modbusClient.ReadHoldingRegisters(5, 1);
                System.Threading.Thread.Sleep(1000);
           // } while (res < 2);
           } while (result[0] != 1);

            LogMessage("S2", "Peformance Test Completed", u.FCode);

            TestJigPort.Close();
            modbusClient.Disconnect();
            PerformanceTestTimer.Stop();

            float Imax =0, Imin=0, Pmax=0, Pmin=0;

            foreach(Model m in Models)
            {
                if(m.Name == u.Model)
                {
                    Imax = (float)m.Imax;
                    Imin = (float)m.Imin;
                    Pmax = (float)m.Pmax;
                    Pmin = (float)m.Pmin;
                }
            }

            foreach(TestResponse r in ResponseList)
            {
                TestRecord rs = r.ParseResponse(u.FCode, u.Model, Imax,Imin,Pmax,Pmin);

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                       new Action(() =>
                                       {
                                           TestJigTextBox.Text += rs.ToString();
                                           
                                       }));


                if (rs.Status == "PASS")
                {
                    LogMessage("S2", "Peformance Test Passed", u.FCode);
                    rs.Timestamp = DateTime.Now;
                    TestRecordQ.Enqueue(rs);
                    dataAccess.InsertTestRecord(rs);


                    if (!ControllerSimulation)
                        IndicateMainFrameFunctionalTestSuccess();
                    e.Result = u;
                    return;
                }

            }
            TestRecord f = ResponseList[ResponseList.Count - 1].ParseResponse(u.FCode,u.Model,Imax,Imin,Pmax,Pmin);
            f.Timestamp = DateTime.Now;
            TestRecordQ.Enqueue(f);
            dataAccess.InsertTestRecord(f);
            LogMessage("S2", "Perfomance Test Failed", u.FCode);
            if (!ControllerSimulation)
                IndicateMainFrameFunctionalTestFailure();
            e.Result = u;
           

        }

        void LogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            LogRecord lr = new LogRecord();
            if (LogQ.TryDequeue(out lr))
            {
                dataAccess.InsertLogRecord(lr);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                         new Action(() =>
                                         {
                                             Log.Insert(0, lr);
                                         }));
            }

            TestRecord tr = new TestRecord();
            if (TestRecordQ.TryDequeue(out tr))
            {
                //dataAccess.InsertLogRecord(lr);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                         new Action(() =>
                                         {
                                             TestLog.Insert(0, tr);
                                         }));
            }
            LogTimer.Start();

        }

        void MainFrameFunctionalTestSuccess_Elapsed(object sender, ElapsedEventArgs e)
        {
            MainFrameFunctionalTestFailure.Stop();
            byte[] values = { 0, 0 };
            modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)7, values);
        }

        void MainFrameFunctionalTestFailure_Elapsed(object sender, ElapsedEventArgs e)
        {
            MainFrameFunctionalTestFailure.Stop();
            byte[] values = { 0, 0 };
            modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)8, values);

            LogMessage("S2", "Failure Indication Stopped", "");
        }

        void FGProcess_Elapsed(object sender, ElapsedEventArgs e)
        {
            FGProcess.Stop();
            byte[] values = { 0, 0 };
            waitingforRO = false;
            latestCombinationCode = "";

            modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)5, values);
        }

        void FGFailure_Elapsed(object sender, ElapsedEventArgs e)
        {
            FGFailure.Stop();
            byte[] values = { 0, 0 };
            modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)6, values);
        }

        void FGSuccess_Elapsed(object sender, ElapsedEventArgs e)
        {
            FGSuccess.Stop();
            byte[] values = { 0, 0 };
            modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)4, values);
        }

        void IError_Elapsed(object sender, ElapsedEventArgs e)
        {
            IError.Stop();
            byte[] values = { 0, 0 };
            modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)3, values);
        }

        void F2Error2_Elapsed(object sender, ElapsedEventArgs e)
        {
            F2Error2.Stop();
            byte[] values = { 0, 0 };
            modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)2, values);
        }

        void F2Error1_Elapsed(object sender, ElapsedEventArgs e)
        {
            F2Error1.Stop();
            byte[] values = { 0, 0 };
            modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)2, values);
        }

        ModbusClient PLCModbus = new ModbusClient("172.20.241.201", 502);

        void modbusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //byte[] values = new byte[8];
            modbusTimer.Stop();
            if (!PLCModbus.Connected)
            {
                PLCModbus = new ModbusClient("172.20.241.201", 502);    //Ip-Address and Port of Modbus-TCP-Server
                PLCModbus.Connect();
            }
            var values = PLCModbus.ReadHoldingRegisters(4, 1);
            if(values[0] == 1)
            {
                if (CurrentFramePlan == null)
                {
                    MessageBox.Show(" Please Select Plan to continue",
                        "Application Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }




                FSerialQ.Enqueue(true);

            }
           // modbusClient.Disconnect();
            //modbusMaster.ReadHoldingRegister(1, 0, 0, 1,ref values);
            modbusTimer.Start();
        }

        void tickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            tickTimer.Stop();

            if (Startup == true)
            {
                int[] values = { 1, 1 };
                //ModbusClient modbusClient = new ModbusClient("172.20.241.201", 502);    //Ip-Address and Port of Modbus-TCP-Server
                //modbusClient.Connect();
                //modbusClient.WriteMultipleRegisters(0, values);
                //modbusClient.Disconnect();

               // modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)2, values);
                /*
                                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)3, values);
                                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)4, values);
                                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)5, values);
                                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)6, values);
                                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)7, values);
                                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)8, values);
                 */
                Startup = false;
            }
                 


            updatePlan();
         //   dataAccess.DeleteAssociationTimeouts(AssociationTimeout);
            updateUnitData();

            processSerialQs();
            tickTimer.Start();
        }


        private void AddToQ(string qName )
        {
            switch(qName)
            {
                case "FSerial":
                   
                    FSerialQ.Enqueue(true);
                    break;

                case "FCode":
                    CurrentFramePlan.FSerialNo++;

                    String fcode = CurrentFramePlan.ModelCode + DateTime.Now.ToString("yyMMdd")
                       + CurrentFramePlan.FSerialNo.ToString("D4");



                    
                        FCodeQ.Enqueue(fcode);
                    break;

                case "CCode":
                    CurrentFramePlan.FSerialNo++;

                    String ccode = CurrentFramePlan.ModelCode + DateTime.Now.ToString("yyMMdd")
                       + CurrentFramePlan.FSerialNo.ToString("D4");




                    CCodeQ.Enqueue(ccode);
                    break;


            }
        }

        private void processSerialQs()
        {
            Boolean temp;
            if (FSerialQ.TryDequeue(out temp) == true)
            {
                //process F1 Station

                if (CurrentFramePlan.FSerialNo >= CurrentFramePlan.Quantity)
                {
                    MessageBox.Show("Current Plan Completed. Please Modify plan or Select another plan to continue",
                        "Application Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                else
                {
                    CurrentFramePlan.FSerialNo++;

                    String fcode = CurrentFramePlan.ModelCode + DateTime.Now.ToString("yyMMdd")
                       + CurrentFramePlan.FSerialNo.ToString("D4");



                    if (ScannerSimulation)
                        FCodeQ.Enqueue(fcode);

                    int printCount = f1PrintCount;
                    do
                    {
                        String template = CurrentFramePlan.ModelName.Contains("Dummy") ? DummyF1BarcodeFile : F1BarcodeFile;
                        if (!PrinterSimulation)
                        {
                            bool result = false;
                            int count = 0;
                            do
                            {

                                result =
                                    PrinterManager.PrintBarcode("S11", CurrentFramePlan.ModelName, CurrentFramePlan.ModelCode,
                                DateTime.Now.ToString("yyMMdd"), CurrentFramePlan.FSerialNo.ToString("D4"), template);
                                count++;
                            } while ((result == false) && (count < 3));

                        }
                    } while (--printCount > 0);
                    dataAccess.InsertUnit(CurrentFramePlan.ModelCode, Model.Type.FRAME,
                        CurrentFramePlan.FSerialNo);
                    dataAccess.UpdateFSerial(CurrentFramePlan);

                    

                    F1pressed = true;
                    F1Checkcount = 0;
                   

                    String logMsg = String.Empty;

                   
                    LogMessage("S1", "Barcode Printed",fcode);

                    Model m = FindModel(CurrentFramePlan.ModelCode);
                    if(m.ByPassPerformanceTest)
                    {
                        dataAccess.InsertUnitAssociation(m.Code, fcode, Model.Type.FRAME);
                        if (ScannerSimulation)
                            CCodeQ.Enqueue(fcode);
                    }

                   

                }

            }


            



        }

        private void updateUnitData()
        {
            DataTable dt = dataAccess.GetReportData(DateTime.Now, DateTime.Now);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                             new Action(() =>
                             {
                                 AssociationGrid.DataContext = dt;
                             }));
        }
  
     


        private void SetupPlan()
        {
            FramePlans = dataAccess.GetPlans(Model.Type.FRAME);

            foreach (Plan p in FramePlans)
            {

                p.SetCurrentEvent += P_SetCurrentEvent;
                p.SetPrintCountEvent += P_SetPrintCountEvent;
                FramePlanDictionary.TryAdd(p.ModelCode, p);
                FramePlanList.Add(p);
                if (p.FStatus == true)
                    CurrentFramePlan = p;
            }
            if (CurrentFramePlan != null)
            {
                FramePlanList.Remove(CurrentFramePlan);
                FramePlanList.Insert(0, CurrentFramePlan);
            }


            BodyPlans = dataAccess.GetPlans(Model.Type.BODY);

            foreach (Plan p in BodyPlans)
            {

                p.SetCurrentEvent += P_SetCurrentEvent;
                p.SetPrintCountEvent += P_SetPrintCountEvent;
                BodyPlanDictionary.TryAdd(p.ModelCode, p);
                BodyPlanList.Add(p);
                if (p.BStatus == true)
                    CurrentBodyPlan = p;
            }
            if (CurrentBodyPlan != null)
            {
                BodyPlanList.Remove(CurrentBodyPlan);
                BodyPlanList.Insert(0, CurrentBodyPlan);
            }

        }

        private void updatePlan()
        {
            FramePlans = dataAccess.GetPlans(Model.Type.FRAME);

            foreach (Plan p in FramePlans)
            {

                if (FramePlanDictionary.ContainsKey(p.ModelCode))
                    continue;
                p.SetCurrentEvent += P_SetCurrentEvent;
                p.SetPrintCountEvent += P_SetPrintCountEvent;
                FramePlanDictionary.TryAdd(p.ModelCode, p);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              FramePlanList.Add(p);

                          }));
            }


            BodyPlans = dataAccess.GetPlans(Model.Type.BODY);

            foreach (Plan p in BodyPlans)
            {

                if (BodyPlanDictionary.ContainsKey(p.ModelCode))
                    continue;
                p.SetCurrentEvent += P_SetCurrentEvent;
                p.SetPrintCountEvent += P_SetPrintCountEvent;
                BodyPlanDictionary.TryAdd(p.ModelCode, p);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              BodyPlanList.Add(p);

                          }));
            }




            int FrameTotal = 0;
            int FrameActual = 0;
            int FSerial = 0;
            int ISerial = 0;
            int CSerial = 0;
            foreach (Plan p in FramePlans)
            {
                FrameTotal += p.Quantity;
                FrameActual += p.Actual;
                FSerial += p.FSerialNo;
                ISerial += p.IntegratedSerialNo;
                CSerial += p.CombinationSerialNo;
            }
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                             new Action(() =>
                             {
                                 FrametbTotalPlan.Text = FrameTotal.ToString();
                                 FrametbTotalAct.Text = FrameActual.ToString();
                                 FrametbTotalFserial.Text = FSerial.ToString();
                                 FrametbTotalIserial.Text = ISerial.ToString();
                                 FrametbTotalCserial.Text = CSerial.ToString();
                             }));

            int BodyTotal = 0;

            int BSerial = 0;
            foreach (Plan p in BodyPlans)
            {
                BodyTotal += p.Quantity;

                BSerial += p.BSerialNo;
            }
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                             new Action(() =>
                             {
                                 BodyTotalPlan.Text = BodyTotal.ToString();
                                 BodyTotalAct.Text = BSerial.ToString();
                             }));

        }

        private void P_SetPrintCountEvent(object sender, EventArgs e)
        {

            Plan p = sender as Plan;
            switch (p.UnitType)
            {
                case Model.Type.FRAME:
                    if (CurrentFramePlan != null && CurrentFramePlan.ModelCode == p.ModelCode)
                        f1PrintCount = p.F1Quantity;
                    break;
                case Model.Type.BODY:
                    if (CurrentBodyPlan != null && CurrentBodyPlan.ModelCode == p.ModelCode)
                        m1PrintCount = p.M1Quantity;
                    break;

            }
        }

        private void P_SetCurrentEvent(object sender, EventArgs e)
        {
            int index = 0;
            Plan p = sender as Plan;
            switch (p.UnitType)
            {

                case Model.Type.FRAME:
                    if (FramePlanDictionary.ContainsKey(p.ModelCode))
                    {

                        CurrentFramePlan = p;
                        dataAccess.UpdateFPlanStatus(CurrentFramePlan);
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                    new Action(() =>
                                    {
                                        FramePlanList.Remove(CurrentFramePlan);

                                        FramePlanList.Insert(0, CurrentFramePlan);
                                        for (int i = 1; i < FramePlanList.Count; i++)
                                        {
                                            FramePlanList[i].FStatus = false;
                                            dataAccess.UpdateFPlanStatus(FramePlanList[i]);
                                        }

                                    }));
                    }

                    if (p.F1Quantity <= 0)
                        p.F1Quantity = 1;
                    break;

                case Model.Type.BODY:
                    if (BodyPlanDictionary.ContainsKey(p.ModelCode))
                    {

                        CurrentBodyPlan = p;
                        dataAccess.UpdateBPlanStatus(CurrentBodyPlan);
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                    new Action(() =>
                                    {
                                        BodyPlanList.Remove(CurrentBodyPlan);

                                        BodyPlanList.Insert(0, CurrentBodyPlan);
                                        for (int i = 1; i < BodyPlanList.Count; i++)
                                        {
                                            BodyPlanList[i].BStatus = false;
                                            dataAccess.UpdateBPlanStatus(BodyPlanList[i]);
                                        }

                                    }));
                    }

                    if (p.M1Quantity <= 0)
                        p.M1Quantity = 1;
                    break;
            }
        }


        //Code added on 11 Nov
        private void andonManager_actQtyAlertEvent(object sender, actQtyScannerEventArgs e)
        {
            String ROTag = String.Empty;

            String modelCode = String.Empty;

            String FCode = String.Empty;
            String ICode = String.Empty;
            Model model = null;
            bool ModelInPlan = false;
            lock (FGLock)
            {

                if (waitingforRO == false)
                {
                    ModelInPlan = false;
                    LogMessage("FG", "CS Scanned", e.Barcode);

                    modelCode = e.Barcode.Substring(0, 4);
                    model = FindModel(modelCode);
                    if (model == null)     //model code not found , CS invalid
                    {

                        IndicateFGFailure();
                        LogMessage("FG", "Barcode InValid", e.Barcode);
                        latestCombinationCode = String.Empty;
                        return;
                    }
                    foreach (Plan p in FramePlans)
                    {
                        if (p.ModelCode == model.Code)
                        {
                            ModelInPlan = true;
                            break;
                        }
                    }
                    if (ModelInPlan == false)
                    {
                        IndicateFGFailure();
                        LogMessage("FG", "Unit Not In Plan", e.Barcode);
                        latestCombinationCode = String.Empty;
                        return;
                    }
                    if (dataAccess.CheckAssociationStatus(modelCode))       //CS already associated
                    {
                        LogMessage("FG", "CS Already Processed", e.Barcode);
                        IndicateFGFailure();
                        latestCombinationCode = String.Empty;
                        return;
                    }
                    if ((model.ByPassFunctionalTest==false) &&   (model.ByPassPerformanceTest==false))
                    {
                        switch (dataAccess.HasPassedFunctional_PerformanceTest(e.Barcode,
                            model.ByPassFunctionalTest, model.ByPassPerformanceTest, out FCode, out ICode))
                        {
                            case 0:

                                LogMessage("FG", "CS Invalid", e.Barcode);
                                latestCombinationCode = String.Empty;
                                return;
                            case 1:

                                LogMessage("FG", "Functional Test Failed", FCode);
                                latestCombinationCode = String.Empty;
                                return;
                            case 2:
                                LogMessage("FG", "Performance Test Failed", ICode);
                                latestCombinationCode = String.Empty;
                                return;
                            case 3:
                                LogMessage("FG", "Functional Test Passed", FCode);
                                LogMessage("FG", "Performance Test Passed", ICode);
                                break;
                        }
                    }

                    if (!ControllerSimulation)
                    {
                        byte[] processvalues = { 0, 1 };
                        modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)5, processvalues);   //process indication
                        FGProcess.Start();
                    }

                    latestCombinationCode = e.Barcode;


                    dataAccess.UpdateWaitingStatus(latestCombinationCode);
                    waitingforRO = true;

                    if (PrinterSimulation)
                        ACodeQ.Enqueue(ModelDictionary[modelCode].ROTag + DateTime.Now.ToString("yyMMdd"));


                    return;

                }
                else if (waitingforRO == true)
                {
                    LogMessage("FG", "RO Scanned", e.Barcode);
                    if (latestCombinationCode != "")
                    {
                        modelCode = latestCombinationCode.Substring(0, 4);
                    }
                    else
                        return;

                    if (!e.Barcode.StartsWith(ModelDictionary[modelCode].ROTag))
                    {
                        IndicateFGFailure();
                        waitingforRO = false;
                        latestCombinationCode = string.Empty;
                        LogMessage("FG", "RO Tag Mismatch", e.Barcode);
                        return;
                    }

                    if (dataAccess.CheckROStatus(e.Barcode) && model.Name != "Z1" && (model.Name != "A1"))
                    {
                        IndicateFGFailure();
                        waitingforRO = false;
                        latestCombinationCode = string.Empty;
                        LogMessage("FG", "RO Tag Already Processed", e.Barcode);
                        return;
                    }


                    foreach (Plan p in FramePlanList)
                    {
                        if (p.ModelCode == modelCode)
                        {
                            p.Actual++;
                            dataAccess.UpdateActual(p.Actual, p.slNumber);
                            dataAccess.UpdateAsscociationStatus(e.Barcode);
                            LogMessage("FG", "Actual Updated", p.Actual.ToString() + "/" + p.Quantity.ToString());
                            break;
                        }
                    }
                    waitingforRO = false;
                    latestCombinationCode = string.Empty;
                    if (!ControllerSimulation)
                        IndicateFGSuccess();
                    // }
                    // else
                    // {
                    //     IndicateFGFailure();
                    //    waitingforRO = false;
                    //      latestCombinationCode = string.Empty;
                    //      LogMessage("FG", "CS Association Error", e.Barcode);
                    //  }
                }
            }
        }

        private void IndicateFGSuccess()
        {
            if (!ControllerSimulation)
            {
                byte[] processvalues = { 0, 0 };
                byte[] successvalues = { 0, 1 };

                FGProcess.Stop();
                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)5, processvalues); //stop processing
                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)4, successvalues); //indicate success

                FGSuccess.Start();
            }
        }

        private void IndicateFGFailure()
        {
            if (!ControllerSimulation)
            {
                byte[] processvalues = { 0, 0 };
                byte[] failurevalues = { 0, 1 };

                FGProcess.Stop();

                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)5, processvalues); //stop processing
                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)6, failurevalues); //indicate failure

                FGFailure.Start();
            }
        }

        private void IndicateMainFrameFunctionalTestSuccess()
        {
            if (!ControllerSimulation)
            {
                byte[] values = { 0, 1 };


                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)7, values); //stop processing


                MainFrameFunctionalTestSuccess.Start();
            }
        }

        private void IndicateMainFrameFunctionalTestFailure()
        {
            if (!ControllerSimulation)
            {
                byte[] values = { 0, 1 };
               

                modbusMaster.WriteMultipleRegister((ushort)3, (byte)0, (ushort)8, values); //stop processing
                

                MainFrameFunctionalTestFailure.Start();
                LogMessage("S2", "Failure Indication Started", "");
            }
        }

        private Model FindModel(string barcode)
        {
            if (!ModelDictionary.ContainsKey(barcode)) return null;
            return ModelDictionary[barcode];
        }

        private void andonManager_combStickerAlertEvent(object sender, CSScannerEventArgs e)
        {
            lock (F3Lock)
            {
                String iCode = e.ModelNumber + e.Timestamp + e.SerialNo.ToString("D4");
                Model model = FindModel(e.ModelNumber);

                if (model == null)
                {
                    LogMessage("S3", "Invalid Barcode", iCode);
                    return;
                }
                bool exists = false; string ccode = string.Empty;
                dataAccess.CheckIntegrationStatus(iCode, out exists, out ccode);
                if(ccode != String.Empty)
                {
                    LogMessage("S3", "CS Already Printed ", iCode);
                    return;
                }
                

                Plan plan = null;

                //LogMessage("F3:Integrated Scanned\t\t" + iCode);
                LogMessage("S3", "Unit Scanned", iCode);

                if (model.ByPassPerformanceTest == false)
                {
                    if (dataAccess.PerformanceTestCompleted(iCode) == false)
                    {
                        LogMessage("S3", "Test Result Not found", iCode);
                        latestCombinationCode = String.Empty;
                        return;
                    }
                    if (dataAccess.HasPassedPerformanceTest(iCode))
                    {


                        LogMessage("S3", "Performance Test Passed", iCode);
                        latestCombinationCode = String.Empty;


                    }
                    else
                    {
                        LogMessage("S3", "Performance Test Failed", iCode);
                        return;
                    }

                    foreach (Plan p in FramePlans)
                    {
                        if (p.ModelCode == model.Code)
                        {
                            plan = p;
                            break;
                        }
                    }
                    plan.CombinationSerialNo++;
                    String csCode = e.ModelNumber + DateTime.Now.ToString("yyMMdd") + plan.CombinationSerialNo.ToString("D4");
                    dataAccess.UpdateAssociation(csCode, Model.Type.COMBINED, "", iCode);

                    


                    dataAccess.UpdateCSerial(plan);

                   
                    if (!PrinterSimulation)
                    {

                        PrinterManager.PrintCombSticker(model, csCode, templatePath + model.Name + ".prn");

                        LogMessage("S3", "CS Printed", csCode);

                    }
                    else LogMessage("S3", "CS Printed", csCode);

                    foreach (Plan p in FramePlanList)
                    {
                        if (p.ModelCode == model.Code)
                        {
                            p.Actual++;
                            dataAccess.UpdateActual(p.Actual, p.slNumber);
                            dataAccess.UpdateAsscociationStatus(csCode);
                            LogMessage("S3", "Actual Updated", p.Actual.ToString() + "/" + p.Quantity.ToString());
                            break;
                        }
                    }

                }
                else
                {
                    foreach (Plan p in FramePlans)
                    {
                        if (p.ModelCode == model.Code)
                        {
                            plan = p;
                            break;
                        }
                    }
                    plan.CombinationSerialNo++;
                    String csCode = e.ModelNumber + DateTime.Now.ToString("yyMMdd") + plan.CombinationSerialNo.ToString("D4");
                    dataAccess.UpdateAssociation(csCode, Model.Type.COMBINED, "", iCode);

                    


                    dataAccess.UpdateCSerial(plan);

                    

                    if (!PrinterSimulation)
                    {

                        PrinterManager.PrintCombSticker(model, csCode, templatePath + model.Name + ".prn");

                        LogMessage("S3", "CS Printed", csCode);

                    }


                    else LogMessage("S3", "CS Printed", csCode);

                    if(!dataAccess.PerformanceTestCompleted(iCode))
                    {
                        TestRecord tr = new TestRecord();
                        tr.UpdateStatus(csCode, model.Name, "Bypassed");
                        TestRecordQ.Enqueue(tr);
                    }

                    

                    foreach (Plan p in FramePlanList)
                    {
                        if (p.ModelCode == model.Code)
                        {
                            p.Actual++;
                            dataAccess.UpdateActual(p.Actual, p.slNumber);
                            dataAccess.UpdateAsscociationStatus(csCode);
                            LogMessage("S3", "Actual Updated", p.Actual.ToString() + "/" + p.Quantity.ToString());
                            break;
                        }
                    }
                }
                

                if (!ControllerSimulation)
                    IndicateMainFrameFunctionalTestSuccess();

                
            }
        }

        async void andonManager_barcodeAlertEvent(object sender, BCScannerEventArgs e)
        {
            lock (F2Lock)
            {
                String barcode = e.ModelNumber + e.Timestamp + e.SerialNo.ToString("D4");

                String assocationBarcode = String.Empty;
                String template = String.Empty;
                String modelName = String.Empty;
                String modelCode = String.Empty;

                bool IsBody = false;

 
                modelCode = e.ModelNumber;
                Model model = FindModel(modelCode);

                if (model == null)
                {
                    if (!ControllerSimulation)
                        IndicateMainFrameFunctionalTestFailure();

                    LogMessage("S2", " Invalid Barcode", barcode);
                    return;
                }
                Plan plan = null;

                if (dataAccess.UnitExists(barcode) == false)        //unit does not exist
                {
                    if (!ControllerSimulation)
                        IndicateMainFrameFunctionalTestFailure();

                    LogMessage("S2", " Invalid Barcode", barcode);
                    return;
                }

                if (dataAccess.UnitProcessed(barcode) == true)      //unit has already been processed
                {
                    if (!ControllerSimulation)
                        IndicateMainFrameFunctionalTestFailure();

                    LogMessage("S2", " Unit Already Processed", barcode);

                    return;
                }





             





                   

                LogMessage("S2", "Unit Scanned", barcode);

                if (!ControllerSimulation)
                {
                    ModbusClient modbusClient = new ModbusClient("172.20.241.201", 502);
                    modbusClient.Connect();
                    modbusClient.WriteSingleRegister(0, 1);
                    modbusClient.WriteSingleRegister(0, 0);
                    modbusClient.Disconnect();

                    LogMessage("S2", "Peformance Test Started", barcode);




                        int TestResult = 0;

                        UnitAssociation unit = new UnitAssociation(65535);
                        unit.Model = model.Name;
                        unit.FCode = barcode;
                        PerformanceTestTimer.Start();
                        TestWorker.RunWorkerAsync(unit);
                    

                    //LogMessage("S2", "Peformance Test ByPassed", barcode);
                    //    dataAccess.InsertUnitAssociation(e.ModelNumber, barcode, Model.Type.FRAME);

                    //    foreach (Plan p in FramePlans)
                    //    {
                    //        if (p.ModelCode == model.Code)
                    //        {
                    //            plan = p;
                    //            break;
                    //        }
                    //    }
                    //    plan.CombinationSerialNo++;
                    //    String csCode = e.ModelNumber + DateTime.Now.ToString("yyMMdd") + plan.CombinationSerialNo.ToString("D4");

                    //    dataAccess.UpdateAssociation(csCode, Model.Type.COMBINED, "", barcode);
                    /*
                                                TestRecord tr = new TestRecord();
                                                tr.UpdateStatus(csCode,model.Name, "Bypassed");
                                                TestRecordQ.Enqueue(tr);
                    */
                }
                else
                {




                   
                }

            }
            
        }

       
  

        void andonManager_andonAlertEvent(object sender, AndonAlertEventArgs e)
        {
            try
            {
                foreach (LogEntry lg in e.StationLog)
                {

                    DeviceAssociation deviceAssociation = dataAccess.getDeviceAssociation(e.StationId);

                    if (deviceAssociation == null) return;

                    String logMsg = String.Empty;

                    logMsg += deviceAssociation.Header + "";
                    String lineName = deviceAssociation.LineName;
                    String stationName = deviceAssociation.StationName;

                    logMsg += lineName + ":" + stationName;

                    logMsg += "--Request Raised" ;

                    if (e.StationId == 2)
                    {

                        if (CurrentBodyPlan == null)
                        {
                            MessageBox.Show(" Please Select Plan to continue",
                                "Application Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (CurrentBodyPlan.BSerialNo > CurrentBodyPlan.Quantity)
                        {
                            MessageBox.Show("Current Plan Completed. Please Modify plan or Select another plan to continue",
                                "Application Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            CurrentBodyPlan.BSerialNo++;
                            String bcode = CurrentBodyPlan.ModelCode + "A" + DateTime.Now.ToString("yyMMdd")
                             + CurrentBodyPlan.BSerialNo.ToString("D4");

                            if (ScannerSimulation)
                                BCodeQ.Enqueue(bcode);

                            int printCount = m1PrintCount;
                            do
                            {

                                if (!PrinterSimulation)
                                {
                                    String template = CurrentBodyPlan.ModelName.Contains("Dummy") ? DummyM1BarcodeFile : M1BarcodeFile;
                                    bool result = false;
                                    int count = 0;
                                    do
                                    {
                                        result =
                                        PrinterManager.PrintBarcode("M1Printer", CurrentBodyPlan.ModelName,
                                        CurrentBodyPlan.ModelCode + "A", DateTime.Now.ToString("yyMMdd"),
                                         CurrentBodyPlan.BSerialNo.ToString("D4"), template);
                                        count++;
                                    } while ((result == false) && (count < 3));

                                }
                            } while (--printCount > 0);

                            dataAccess.InsertUnit(CurrentBodyPlan.ModelCode, Model.Type.BODY,
                                CurrentBodyPlan.BSerialNo);

                            LogMessage("M1", "MB Printed", bcode);

                            dataAccess.UpdateBSerial(CurrentBodyPlan);


                        }
                    }
                    else if (e.StationId == 1)
                    {

                        if (CurrentFramePlan == null)
                        {
                            MessageBox.Show(" Please Select Plan to continue",
                                "Application Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (CurrentFramePlan.FSerialNo > CurrentFramePlan.Quantity)
                        {
                            MessageBox.Show("Current Plan Completed. Please Modify plan or Select another plan to continue",
                                "Application Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {

                            CurrentFramePlan.FSerialNo++;
                            String fcode = CurrentFramePlan.ModelCode + DateTime.Now.ToString("yyMMdd")
                               + CurrentFramePlan.FSerialNo.ToString("D4");



                            if (ScannerSimulation)
                                FCodeQ.Enqueue(fcode);

                            int printCount = f1PrintCount;
                            do
                            {
                                String template = CurrentFramePlan.ModelName.Contains("Dummy") ? DummyF1BarcodeFile : F1BarcodeFile;
                                if (!PrinterSimulation)
                                {
                                    bool result = false;
                                    int count = 0;
                                    do
                                    {

                                        result =
                                            PrinterManager.PrintBarcode("S11", CurrentFramePlan.ModelName, CurrentFramePlan.ModelCode,
                                        DateTime.Now.ToString("yyMMdd"), CurrentFramePlan.FSerialNo.ToString("D4"), template);
                                        count++;
                                    } while ((result == false) && (count < 3));

                                }
                            } while (--printCount > 0);
                            dataAccess.InsertUnit(CurrentFramePlan.ModelCode, Model.Type.FRAME,
                                CurrentFramePlan.FSerialNo);

                            LogMessage("S1", "Barcode Printed", fcode);

                            dataAccess.UpdateFSerial(CurrentFramePlan);



                        }
                    }

                   // LogMessage(logMsg);
                    //LogMessage(stationName , (e.StationId==1 )? "MF Printed" : "MB Printed", (e.StationId==1 )?

                  
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Error", MessageBoxButton.OK);
            }
        }

        void LogMessage(String stage, String message, String barcode)
        {
            try
            {
                LogRecord lr = new LogRecord
                {
                    Timestamp = DateTime.Now,
                    Stage = stage,
                    Remarks = message,
                    Barcode = barcode
                };

                LogQ.Enqueue(lr);
             
            }
            
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private void tabPlan_Loaded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            Users = dataAccess.GetUsers();
            tabPlan.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                            new Action(() =>
                                            {
                                                BaseGrid.Children.Clear();
                                                LoginPage lp = new LoginPage(Users);
                                                lp.LoginEvent += lp_LoginEvent;
                                                BaseGrid.Children.Add(lp);
                                            }));

        }

        void lp_LoginEvent(object sender, LoginEventArgs e)
        {

            CurrentUser = e.User;

            BaseGrid.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                            new Action(() =>
                                            {
                                                BaseGrid.Children.Clear();
                                                dbView = new DashBoardView(Users, CurrentUser.Name, PrinterManager);
                                                BaseGrid.Children.Add(dbView);
                                            }));


        }



        private void FrameLabelQuantitySelectionChanged(object sender, RoutedEventArgs e)
        {
            f1PrintCount = ((ComboBox)sender).SelectedIndex + 1;
        }

        private void BodyLabelQuantitySelectionChanged(object sender, RoutedEventArgs e)
        {
            m1PrintCount = ((ComboBox)sender).SelectedIndex + 1;
        }



        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (this.IsLoaded)
                {

                    if (BaseTabControl.SelectedIndex == 1)
                    {
                        Users = dataAccess.GetUsers();
                        tabPlan.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                        new Action(() =>
                                                        {
                                                            BaseGrid.Children.Clear();
                                                            LoginPage lp = new LoginPage(Users);
                                                            lp.LoginEvent += lp_LoginEvent;
                                                            BaseGrid.Children.Add(lp);
                                                        }));
                    }
                    else if (BaseTabControl.SelectedIndex == 3)
                    {

                    }
                }
            }


        }


        void WindowClosing(object sender, CancelEventArgs e)
        {
            if (andonManager != null)
                andonManager.stop();
        }


        private void MBmaster_OnException(ushort id, byte unit, byte function, byte exception)
        {
            string exc = "Modbus says error: ";
            switch (exception)
            {
                case Master.excIllegalFunction: exc += "Illegal function!"; break;
                case Master.excIllegalDataAdr: exc += "Illegal data adress!"; break;
                case Master.excIllegalDataVal: exc += "Illegal data value!"; break;
                case Master.excSlaveDeviceFailure: exc += "Slave device failure!"; break;
                case Master.excAck: exc += "Acknoledge!"; break;
                case Master.excGatePathUnavailable: exc += "Gateway path unavailbale!"; break;
                case Master.excExceptionTimeout: exc += "Slave timed out!"; break;
                case Master.excExceptionConnectionLost: exc += "Connection is lost!"; break;
                case Master.excExceptionNotConnected: exc += "Not connected!"; break;
            }

            MessageBox.Show(exc, "Modbus slave exception");
        }

        // ------------------------------------------------------------------------
        // Event for response data
        // ------------------------------------------------------------------------
        private void MBmaster_OnResponseData(ushort ID, byte unit, byte function, byte[] values)
        {
            // ------------------------------------------------------------------------
            // Identify requested data
            switch (ID)
            {
                case 1:

                    break;
                case 2:
                    if (values[1] == 1)
                    {
                        if (CurrentFramePlan == null)
                        {
                            MessageBox.Show(" Please Select Plan to continue",
                                "Application Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }




                        FSerialQ.Enqueue(true);


                    }


                    //if (values[3] == 1)
                    //{
                    //    if (CurrentBodyPlan == null)
                    //    {
                    //        MessageBox.Show(" Please Select Plan to continue",
                    //            "Application Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //        return;
                    //    }


                    //    MSerialQ.Enqueue(true);

                    //}


                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:

                    break;
                case 6:

                    break;
                case 7:

                    break;
                case 8:

                    break;
            }
        }


       

      
    }
}
