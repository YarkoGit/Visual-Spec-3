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
using System.IO;

namespace Visual_Spec_03
{
    /// <summary>
    /// Interaction logic for itemEnergyLayout.xaml
    /// </summary>
    public partial class itemEnergyLayoutWM : UserControl
    {
        public itemEnergyLayoutWM()
        {
            InitializeComponent();
        }

        public void loadPicture(string pictureName, string imageFieldName, string fileType, string division)
        {
            //string link = @"C:\VSI_01\";
            string link = @"C:\LGEWR_IMAGES\MCL\" + division + @"\";

            var imageField = (Image)this.FindName(imageFieldName);

            if (File.Exists(link + pictureName + fileType))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(link + pictureName + fileType);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imageField.Source = bitmap;
            }
            else
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(link + "coming_soon.png");
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imageField.Source = bitmap;

                //imageField.Source = null;
                MainWindow maniForm = new MainWindow();
                maniForm.zapiszLog("ERROR" + "\t" + "itemEnergyLayoutWM: " + "brak zdjecia " + pictureName);
                //publishMessage("ERROR" + "  " + "loadPictureMainForm: " + "brak zdjecia " + pictureName + " model: " + model);
            }
        }

        public void EnergyLabelsLayoutVisibility(bool visible)
        {
            if (visible == true)
            {
                labelBuyerModel.Visibility = Visibility.Visible;
                labelEnergyGrade.Visibility = Visibility.Visible;
                labelEnergyCons.Visibility = Visibility.Visible;
                labelKWh.Visibility = Visibility.Visible;
                labelWaterCons.Visibility = Visibility.Visible;
                labelLannum.Visibility = Visibility.Visible;
                labelWeightCapa.Visibility = Visibility.Visible;
                labelKg.Visibility = Visibility.Visible;
                labelSpinGrade.Visibility = Visibility.Visible;
                labelNoiseWash.Visibility = Visibility.Visible;
                labelNoiseSpin.Visibility = Visibility.Visible;
                imageEnergyArrov.Visibility = Visibility.Visible;
            }
            else
            {
                labelBuyerModel.Visibility = Visibility.Hidden;
                labelEnergyGrade.Visibility = Visibility.Hidden;
                labelEnergyCons.Visibility = Visibility.Hidden;
                labelKWh.Visibility = Visibility.Hidden;
                labelWaterCons.Visibility = Visibility.Hidden;
                labelLannum.Visibility = Visibility.Hidden;
                labelWeightCapa.Visibility = Visibility.Hidden;
                labelKg.Visibility = Visibility.Hidden;
                labelSpinGrade.Visibility = Visibility.Hidden;
                labelNoiseWash.Visibility = Visibility.Hidden;
                labelNoiseSpin.Visibility = Visibility.Hidden;
                imageEnergyArrov.Visibility = Visibility.Hidden;
            }


        }
    }
}
