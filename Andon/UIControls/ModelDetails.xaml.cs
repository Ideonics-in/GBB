using shared.Entity;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ModelDetails.xaml
    /// </summary>
    public partial class ModelDetails : UserControl
    {
        Model currentModel;
        public Model CurrentModel
        {
            get { return currentModel; }

            set
            {
                currentModel = value;
                this.DataContext = currentModel;
                this.Visibility = System.Windows.Visibility.Visible;
            }
        }
        public ModelDetails()
        {
            InitializeComponent();

        }



        internal Model GetModel()
        {
            double volume = 0;
            double power = 0, stdgloss = 0;
            int mrp = 0;

            if ((Double.TryParse(PowerTextBox.Text, out power) == false)
                || (Double.TryParse(VolumeTextBox.Text, out volume) == false)
                || (int.TryParse(MRPTextBox.Text, out mrp) == false)
                || (Double.TryParse(StdgLossTextBox.Text, out stdgloss) == false)
               )
            {
                MessageBox.Show("Invalid Data!! Please Verify ", "Application Info", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                return null;
            }

            CurrentModel.Product = ProductTextBox.Text;
            CurrentModel.ProductNumber = ProductNoTextBox.Text;
            CurrentModel.Name = ModelNameTextBox.Text;
            CurrentModel.Volume = volume;
            CurrentModel.Power = power;
            CurrentModel.StdgLoss = stdgloss;
            CurrentModel.Code = CodeTextBox.Text;
            CurrentModel.MRP = mrp;
            //CurrentModel.CustomerCare = CustomerCareNoTextBox.Text;
            //CurrentModel.Email = StdgLossTextBox.Text;
            //CurrentModel.Width = width;
            //CurrentModel.Height = height;
            //CurrentModel.Depth = depth;

            return CurrentModel;


        }

        private void ByPassCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
