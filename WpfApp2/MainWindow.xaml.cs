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
using System.Threading;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Selenya selenium;
        private bool isWork = false;

        public static MainWindow window;
        public MainWindow()
        {
            
            InitializeComponent();
            
        }

        public static MainWindow Instance()
        {
            if (window == null)
                window = new MainWindow();
            return window;
        }

        private void ChangeState()
        {
            isWork = false;
            ShowMessage("Работа завершена");

            //Dispatcher.Invoke(() => Input.Background = brush);
            
        }

        
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!isWork)
            {
                selenium = new Selenya(login.Text, password.Text);
                Selenya.IsWork = true;
                selenium.endwork += ChangeState;
                selenium.changer += ModifyLabel;
                Thread start = new Thread(selenium.Start);
                isWork = true;
                start.Start();
            }
            else
                ShowMessage("Программа запущена");

        }

        public static void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        public void ModifyLabel(string tem)
        {
            Dispatcher.Invoke(() => Label0.Content += tem + "\n");
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Selenya.IsWork = false;
        }
    }
}
