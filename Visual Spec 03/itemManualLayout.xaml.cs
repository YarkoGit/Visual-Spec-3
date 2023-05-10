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

namespace Visual_Spec_03
{
    /// <summary>
    /// Interaction logic for itemManualLayout.xaml
    /// </summary>
    public partial class itemManualLayout : UserControl
    {

        public string[] manualPartNO;


        public itemManualLayout()
        {
            InitializeComponent();
        }

        private void listView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImagePreview imagePreview = new ImagePreview();
                imagePreview.loadPicture(manualPartNO[listViewManual.SelectedIndex], "imageFullScreen", ".jpg", MCLsettings.Default.Division);
                imagePreview.ifManual = true;
                imagePreview.labelPointName.Content = manualPartNO[listViewManual.SelectedIndex];
                imagePreview.Show();
            }
            catch
            { }
        }

    }
}
