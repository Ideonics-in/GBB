using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Windows.Threading;
using DisplayManager;

namespace AndonDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<LineSummary> Summary;
        Timer UpdateTimer;

        CP5200Andon AndonDisplay;
        public MainWindow()
        {
            InitializeComponent();

            AndonDisplay = new CP5200Andon("COM9", 9600, 500);

            Summary = new ObservableCollection<LineSummary>();
            Summary.Add(new LineSummary { Name = "GBB", Actual = 0, Plan = 0,  WindowID = 5,
                DBConnection = "Data Source=.\\GBB;database=AOSmith_GBB;User id=sa; Password=ide123$%^;" , Cumulative = false });

            Summary.Add(new LineSummary { Name = "MJ", Actual = 0, Plan = 0, Cumulative = false, WindowID = 4,
            DBConnection = "Data Source=AOS-IND-AND-MV;database=AOSmith_MJ;User id=sa; Password=TestBenchData;"
            });

            //Summary.Add(new LineSummary { Name = "INST", Actual = 0, Plan = 0,  Cumulative = false , WindowID = 4 });

            Summary.Add(new LineSummary { Name = "WT", Actual = 0, Plan = 0,  Cumulative = false, WindowID = 3,
                DBConnection = "Data Source=AOS-IND-AND-WT\\SQLEXPRESS;database=AOSmith_WT;User id=sa; Password=IDE123$%^;" });

            Summary.Add(new LineSummary { Name = "Total", Actual = 0, Plan = 0,  Cumulative = true , WindowID = 2 });


            SummaryDataGrid.DataContext = Summary;
            
            UpdateTimer = new Timer(5 * 1000);
            UpdateTimer.AutoReset = false;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;

            UpdateTimer.Start();
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            String PlanStr = string.Empty;
            String CSStr = string.Empty;
            String FGStr = String.Empty;
            UpdateTimer.Stop();
            foreach(LineSummary ls in Summary)
            {
                if (ls.Cumulative == false)
                    ls.Update();
                else ls.UpdateCumulative(Summary);

            }

            foreach (LineSummary ls in Summary)
            {
                string updateStr = ls.GetUpdateString();
                AndonDisplay.UpdateWindow(ls.WindowID, updateStr);
            }
            
            

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                 new Action(() =>
                                 {
                                     SummaryDataGrid.DataContext = null;
                                     SummaryDataGrid.DataContext = Summary;
                                 }));

            UpdateTimer.Start();
        }
    }


    public class LineSummary
    {
        public String Name { get; set; }
        public int Plan { get; set; }
       
        public int Actual { get; set; }
        public bool Cumulative { get; set; }
        public int WindowID { get; set; }
        public String DBConnection { get; set; }

        public void UpdateCumulative (ObservableCollection<LineSummary> summary)
        {
            Plan = Actual = 0;
            foreach(LineSummary ls in summary)
            {

                this.Plan += (ls.Cumulative == false) ? ls.Plan : 0;
                this.Actual += (ls.Cumulative == false) ? ls.Actual : 0;
                
            }
        }

        public string  GetUpdateString()
        {
            
            return Plan.ToString().PadLeft(4) + " " + Actual.ToString().PadLeft(5) ;
        }

        public void Update()
        {
            if (DBConnection == null || DBConnection == String.Empty)
            {
                return;
            }
                
            try
            {
                SqlConnection con = new SqlConnection(DBConnection);
                con.Open();

                String qry = String.Empty;
                qry = @"SELECT SUM(Quantity) as [Plan], 
                         Sum(Plans.Actual) as [Actual]
                        from Plans where [timestamp] >= '{0}' and [Timestamp] <= '{1}' and UnitType = 2";

                qry = String.Format(qry, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), 2);


                SqlCommand cmd = new SqlCommand(qry, con);
                SqlDataReader dr = cmd.ExecuteReader();
                DataTable SummaryTable = new DataTable();
                SummaryTable.Load(dr);
                dr.Close();
                cmd.Dispose();
                Plan = 0;
                Actual = 0;
                

                if(SummaryTable.Rows.Count > 0)
                {
                    Plan = (SummaryTable.Rows[0]["Plan"] == DBNull.Value) ? 0 : (int)SummaryTable.Rows[0]["Plan"];
                    Actual = (SummaryTable.Rows[0]["Actual"] == DBNull.Value) ? 0 : (int)SummaryTable.Rows[0]["Actual"];
                    
                }
                     
                
            }
            catch (Exception E)
            {

            }
            



        }

        
    }
}
