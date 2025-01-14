using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Ari_Player
{
    public partial class Welcome : Window
    {
        public Welcome()
        {
            InitializeComponent();
            
        }

        private void Settings_click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Show();
            this.Close();
        }


    }
}
