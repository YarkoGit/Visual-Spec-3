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
using System.Windows.Shapes;
using System.Data.OleDb;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Data.SqlClient;
using System.Windows.Threading;


namespace Visual_Spec_03
{
    /// <summary>
    /// Interaction logic for OptionWindow.xaml
    /// </summary>
    public partial class OptionWindow : Window
    {
        public OptionWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            MCLsettings.Default.AppID = Int16.Parse(comboBoxAppID.Text);
            MCLsettings.Default.Division = comboBoxDIV.Text;
            MCLsettings.Default.Area = comboBoxArea.Text;
            MCLsettings.Default.InspectorsNames = comboBoxInspectors.Text;

            MCLsettings.Default.Save();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            comboBoxAppID.Text = MCLsettings.Default.AppID.ToString();
            comboBoxDIV.Text = MCLsettings.Default.Division;
            comboBoxArea.Text = MCLsettings.Default.Area;
            getInspectorsNames();
            comboBoxInspectors.Text = MCLsettings.Default.InspectorsNames.ToString();

        }

        private void getInspectorsNames()
        {
            try
            {
                String connectionString = @"Data Source=server_name;Initial Catalog=catalog_name;User ID=user_name;Password=password";
                SqlConnection con = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("SELECT * FROM MCL_INSPECTORS_NAMES", con);
                con.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                comboBoxInspectors.ItemsSource = dt.DefaultView;
                comboBoxInspectors.DisplayMemberPath = "INSPECTOR_NAME";

                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
