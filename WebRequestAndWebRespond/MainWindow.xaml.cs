using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebRequestAndWebRespond
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtTure_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebRequest myWebRequest = WebRequest.Create(TbUri.Text);
                WebResponse myWebResponse = myWebRequest.GetResponse();
                Stream myStream = myWebResponse.GetResponseStream();
                byte[] mybyte = new byte[1024];
                string pagecontent = "";
                int readl = 0;
                do
                {
                    readl = myStream.Read(mybyte, 0, mybyte.Length);
                    pagecontent += Encoding.Default.GetString(mybyte, 0, 1024);
                } while (readl > 0);
                TbPrint.Text = pagecontent;
            }
            catch(Exception e1)
            {
                MessageBox.Show(e1.Message , "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
