using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 网络传输
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
        public delegate void newdelegate();
        int i;
        string fmess,str;
        IPAddress add1 ;
        public void threadxianshi()
        {
            //TbClient.Text = TbClient.Text + add1 + "\r\n";
            TbClient.Text = TbClient.Text +str1+ "\r\n";
        }
        public void threadxianshione()
        {
            TbClient.Text = TbClient.Text +fmess+  "\r\n";
        }
        string str1;
        public void threadclient()
        {
            try
            {
                IPHostEntry ipety = Dns.GetHostEntry(str);
                str1=ipety.HostName;
                //IPAddress[] myIPAddress = Dns.GetHostAddresses(str);
                //foreach (IPAddress add in myIPAddress)
                //{
                   // add1 = add;
                    TbClient.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new newdelegate(threadxianshi));
                //}
            }
            catch(SocketException e)
            {
                fmess = e.Message;
                TbClient.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new newdelegate(threadxianshione));
            }
                    
           
              
                    
               
        }

        private void BtTest_Click(object sender, RoutedEventArgs e)
        {
            str = TbName.Text;
            Thread newthread = new Thread(new ThreadStart(threadclient));
            newthread.Start();

        }
    }

}
