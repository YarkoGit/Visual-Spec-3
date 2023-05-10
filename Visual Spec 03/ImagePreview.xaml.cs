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
using System.IO;


namespace Visual_Spec_03
{
    /// <summary>
    /// Interaction logic for ImagePreview.xaml
    /// </summary>
    public partial class ImagePreview : Window
    {
        public int ActualPoint;
        public bool ifManual;

        public ImagePreview()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            viewBoxImagePreview.Child = null;
            imageFullScreen.Source = null;
            labelPointName.Content = null;

            if (ifManual == false)
            {
                ((MainWindow)this.Owner).loadLayout(ActualPoint, false);
            }

            ifManual = false;

            this.Hide();
        }

        public void loadPicture(string pictureName, string imageFieldName, string fileType, string division)
        {
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

                MainWindow maniForm = new MainWindow();
                maniForm.zapiszLog("ERROR" + "\t" + "ImagePreviewFullScreen: " + "brak zdjecia " + pictureName);
            }
        }
    }
}
