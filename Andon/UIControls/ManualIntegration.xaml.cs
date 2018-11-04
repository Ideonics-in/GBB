using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using TestBenchApp.Entity;

namespace TestBenchApp.UIControls
{
    /// <summary>
    /// Interaction logic for ManualIntegration.xaml
    /// </summary>
    public partial class ManualIntegration : UserControl
    {
        DataAccess da;

        ObservableCollection<ReportRecord> Records;
        List<ReportRecord> ModifiedRecords;
        public ManualIntegration()
        {
            InitializeComponent();
            da = new DataAccess();
            Records = new ObservableCollection<ReportRecord>();

            dgRecordGrid.DataContext = Records;
        }

        private void ReportUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //da.UpdateRecords(ModifiedRecords);
            MessageBox.Show("Records Updated", "Records Update Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReportGenerateButton_Click(object sender, RoutedEventArgs e)
        {


            // Records = da.GetRecords(dpFrom.SelectedDate.Value, dpTo.SelectedDate.Value);
            // dgReportGrid.DataContext = Records;
            // dgReportGrid.Visibility = System.Windows.Visibility.Visible;
        }

        private void dgReportGrid_RowEditEnding(object sender, Microsoft.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            ModifiedRecords.Add(e.Row.DataContext as ReportRecord);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Records.Clear();
            ReportRecord r = da.GetAssociationData(MFTextBox.Text, MBTextBox.Text, ISTextBox.Text, CSTextBox.Text, ROTextBox.Text);
            if (r == null)
            {
                MessageBox.Show("No Record Found", "Search Info", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Records.Add(r);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            int result = da.CheckRecordAssociation( Records[0]);
            MessageBoxResult mresult = new MessageBoxResult();
            switch(result)
            {
                case 0:
                    mresult = MessageBox.Show("All Clear -- Update?", 
                        "Check Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    break;

                case 1:
                    mresult = MessageBox.Show("Main Frame Barcode Error -- Override ?", 
                        "Check Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    break;

                    case 2:
                    mresult = MessageBox.Show("Main Body Barcode Error -- Override ?", 
                        "Check Info", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    break;

                    case 3:
                    mresult = MessageBox.Show("Integrated Barcode Error -- Override ?",
                      "Check Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    break;
                    case 4:
                    mresult = MessageBox.Show("Combination Barcode Error -- Override ?",
                        "Check Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    break;
                    case 5:
                    mresult = MessageBox.Show("ROFilter Barcode Error -- Override ?",
                        "Check Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    break;

                    case 6:
                    mresult = MessageBox.Show("Unknown Error -- Override ?",
                        "Check Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    break;
                  
            }
            if (mresult == MessageBoxResult.Yes)
            {
                if (da.UpdateAssociation(Records[0]))
                    MessageBox.Show("Record Updated", "Update Info", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Error Updating", "Update Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }

               
        }
    }
}
