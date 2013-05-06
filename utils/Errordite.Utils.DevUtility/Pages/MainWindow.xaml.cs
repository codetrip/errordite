using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Errordite.Utils.DevUtility.Pages
{
    /// <summary>
    /// Interaction logic for BuildManagerWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //UseSystemTray();
            Icon = new BitmapImage(new Uri(string.Format("Images/Life-Saver.ico"), UriKind.Relative));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItemClicked = sender as MenuItem;

            if (menuItemClicked != null)
            {
                switch(menuItemClicked.Name)
                {
                    case "Exit":
                        Application.Current.Shutdown(0);
                        break;
                    default:
                        MainFrame.Navigate(new Uri(string.Format("Pages/{0}.xaml", menuItemClicked.Name), UriKind.Relative));
                        break;
                }
            }
        }

        private void UseSystemTray()
        {
            var notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Images/life-saver.ico"),
                Visible = true,
            };

            notifyIcon.DoubleClick += (sender, args) =>
            {
                Show();
                WindowState = WindowState.Normal;
            };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            //if (WindowState == WindowState.Minimized)
            //    Hide();
            base.OnStateChanged(e);
        }
    }
}
