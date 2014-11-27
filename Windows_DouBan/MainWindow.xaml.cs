using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net;
using Chengf;
using System.Collections;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Security.Policy;
using System.Web;
using System.Runtime.InteropServices;

namespace Windows_DouBan
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region//创建需要的对象
        DispatcherTimer g_timer = new DispatcherTimer();
        ArrayList list_3 = new ArrayList();
        int i = 0;//红心模式下记录当前播放的歌曲索引
        int ii = 0;//随机模式下记录当前播放的歌曲索引
        string DouBan_Check;
        string DouBan_Id;
        string DouBan_User;
        string DouBan_PassWord;
        string DouBan_YanZhengMa;
        string DouBan_redchannel;
        string DouBan_blackchannel;
        ArrayList list_cookie = new ArrayList();
        //红心模式变量
        List<string> DouBan_SongHttpList = new List<string>();
        List<string> DouBan_SongNameList = new List<string>();
        List<string> DouBan_SongLike = new List<string>();
        List<string> DouBan_SongSid = new List<string>();
        List<string> DouBan_SongGeShouNameList = new List<string>();
        //随机模式变量
        bool SuiJi = false;//随机模式全局标识
        List<string> SuiJi_SongHttpList = new List<string>();
        List<string> SuiJi_SongNameList = new List<string>();
        List<string> SuiJi_SongGeShouNameList = new List<string>();
        List<string> SuiJi_SongSid = new List<string>();
        List<string> SuiJi_SongLike = new List<string>();
        Cf_HttpWeb myHttpWeb = new Cf_HttpWeb();
        //声明一个委托
        public delegate void myDelegate();
        #endregion
        public MainWindow()
        {
            InitializeComponent();

        }//主窗口,定义了计时器
        void Progress_timer_Tick(object sender, EventArgs e)
        {
            if (melt.HasAudio)
            {
                SdPosition.Value = melt.Position.Ticks;
                SdPosition.ToolTip = ((SdPosition.Value / SdPosition.Maximum) * 100).ToString("0") + "%";
                LbSongJinDu.Content = ((SdPosition.Value / SdPosition.Maximum) * 100).ToString("0") + "%";
                PbXiaZaiJinDu.Value = melt.DownloadProgress;
            }
        }//后台计时器，用以更新前台的Slider及PrassWord
        private void Window_Loaded(object sender, RoutedEventArgs e)//窗体初始化
        {
            if (!Cf_Web.IsConnectedToInternet()) { MessageBox.Show("无网络连接，无法使用本应用", "网络连接错误！", MessageBoxButton.OK, MessageBoxImage.Error); this.Close(); }
            if (Directory.Exists(System.Windows.Forms.Application.StartupPath + "\\Logo"))
            {
                string jpgstring = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\Logo")[0];
                ImJieMian.Source = new BitmapImage(new Uri(jpgstring));
            }
            //设置计时器
            g_timer.Interval = new System.TimeSpan(0, 0, 1);
            g_timer.Tick += new EventHandler(Progress_timer_Tick);
            g_timer.IsEnabled = false;
            //界面元素设置
            PbXiaZaiJinDu.Maximum = 1;
            SdVolume.Maximum = 1;
            SdVolume.Value = melt.Volume;
            BtLogin.Visibility = Visibility.Visible;
            BtKaiShi.Visibility = Visibility.Hidden;
            BtZanTing.Visibility = Visibility.Hidden;
            BtXiaZai.Visibility = Visibility.Hidden;
            LbSongContent.Visibility = Visibility.Hidden;
            LbSongJinDu.Visibility = Visibility.Hidden;
            BtHuoQu.Visibility = Visibility.Hidden;
            BtSuiJi.Visibility = Visibility.Hidden;
            TbHuoQuCiShu.Visibility = Visibility.Hidden;
            BtQieGe.Visibility = Visibility.Hidden;
            SdPosition.Visibility = Visibility.Hidden;
            BtDeleteAll.Visibility = Visibility.Hidden;
            #region//如果存在本地帐号密码和存在本地歌曲缓存则加载
            if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\Cookie.Chengf"))
            {
                baiducookie = Cf_PassWordClass.Decrypt(File.ReadAllText(System.Windows.Forms.Application.StartupPath + "\\Cookie.Chengf"));
            }
            if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\DouBanUser.Chengf"))
            {
                string strtest = Cf_PassWordClass.Decrypt(File.ReadAllText(System.Windows.Forms.Application.StartupPath + "\\DouBanUser.Chengf", Encoding.Unicode));
                TbUser.Text = strtest.Remove(strtest.IndexOf("|"));
                Pb1.Password = strtest.Remove(0, strtest.IndexOf("|") + 1);
            }
            DirectoryInfo dtif = new DirectoryInfo(System.Windows.Forms.Application.StartupPath + "\\Cache");
            if (!Directory.Exists(System.Windows.Forms.Application.StartupPath + "\\Cache"))
            {
                Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath + "\\Cache");
                dtif.Attributes = FileAttributes.Hidden;
            }
            else { MessageBox.Show("检测到本地已经存在缓存文件夹，可能会出现不可预知的错误！", "提示", MessageBoxButton.OK, MessageBoxImage.Error); }
            #endregion
            //获取频道channel值
            //Thread thread_channel = new Thread(DouBan_Channel);
            //thread_channel.Start();
            //获取验证码
            Thread thread1 = new Thread(thread_1);
            thread1.Start();
        }
        #region//完成从初始界面验证码的生成到主界面红心歌曲播发
        private void BtLogin_Click(object sender, RoutedEventArgs e)
        {
            if (TbUser.Text == "" || Pb1.Password == "" || TbCheck.Text == "") { MessageBox.Show("输入错误！", "错误！", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            g_timer.IsEnabled = true;
            DouBan_User = TbUser.Text;
            DouBan_PassWord = Pb1.Password;
            DouBan_YanZhengMa = TbCheck.Text;
            Thread thread2 = new Thread(thread_2);
            thread2.Start();
        }//初始界面的登录按钮
        public void thread_2()//另开线程获取到红心歌曲列表，并播放
        {
            try
            {
                myHttpWeb.ContentType = "application/x-www-form-urlencoded";
                myHttpWeb.EncodingSet = "utf-8";
                myHttpWeb.Referer = "http://douban.fm/";
                myHttpWeb.HeaderSet = "X-Requested-With,XMLHttpRequest";
                myHttpWeb.UserDate = "source=radio&alias=" + DouBan_User + "&form_password=" + DouBan_PassWord + "&captcha_solution=" + DouBan_YanZhengMa + "&captcha_id=" + DouBan_Id + "&task=sync_channel_list";
                list_cookie = myHttpWeb.PostOrGet("http://douban.fm/j/login", HttpMethod.POST, (CookieContainer)list_3[0]);
                if (((string)list_cookie[1]).IndexOf("帐号和密码不匹配") != -1)//判断密码是否正确
                {
                    MessageBox.Show("密码错误！", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    thread_1();
                    return;
                }
                if (((string)list_cookie[1]).IndexOf("验证码不正确") != -1)//判断验证码是否正确
                {
                    MessageBox.Show("验证码错误！请重新输入！", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    thread_1();
                    return;
                }
                //开始获取红心列表
                myHttpWeb.EncodingSet = "utf-8";
                ArrayList list_5 = myHttpWeb.PostOrGet("http://douban.fm/j/mine/playlist?type=n&sid=280187&pt=0.4&channel=-3&pb=64&from=mainsite&r=70af77b33f", HttpMethod.GET, (CookieContainer)list_cookie[0]);
                //MessageBox.Show("成功生成！即将播放音乐", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                string test1 = ((string)list_5[1]).Replace("\\/", "/");
                DouBan_SongHttpList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"url\":\"", "\",\""), 0, "\"url\":\"".Length), 0, "\",\"".Length);
                DouBan_SongNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"title\"", "\",\""), 0, "\"title\"".Length + 2), 0, "\",\"".Length);
                DouBan_SongGeShouNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"artist\"", "\",\""), 0, "\"artist\"".Length + 2), 0, "\",\"".Length);
                DouBan_SongLike = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"like\":", "}"), 0, "\"like\":".Length), 0, 1);
                DouBan_SongSid = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"sid\":\"", "\""), 0, "\"sid\":\"".Length), 0, 1);
                while (DouBan_SongGeShouNameList.IndexOf("") != -1)//以下两行用于处理出现链接为空
                {
                    DouBan_SongNameList.RemoveAt(DouBan_SongGeShouNameList.IndexOf(""));
                    DouBan_SongLike.RemoveAt(DouBan_SongGeShouNameList.IndexOf(""));
                    DouBan_SongSid.RemoveAt(DouBan_SongGeShouNameList.IndexOf(""));
                    DouBan_SongGeShouNameList.Remove("");
                }
                for (int k = 0; k < DouBan_SongHttpList.Count; k++)//空白和非.mp3/.mp4链接地址抹除
                {
                    if (DouBan_SongHttpList[k].IndexOf(".mp3") == -1 && DouBan_SongHttpList[k].IndexOf(".mp4") == -1)
                    {
                        DouBan_SongHttpList.RemoveAt(k);
                        k--;
                    }
                }
                melt.Dispatcher.BeginInvoke(new myDelegate(delegate_3), System.Windows.Threading.DispatcherPriority.Normal);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        public void thread_1()//获取验证码线程
        {
            Lbyanzhengma.Dispatcher.BeginInvoke(new myDelegate(delegate_2), System.Windows.Threading.DispatcherPriority.Normal);
            ArrayList list_1 = myHttpWeb.PostOrGet("http://douban.fm/j/new_captcha", HttpMethod.GET, new CookieContainer());
            DouBan_Check = "http://douban.fm/misc/captcha?size=m&id=" + ((string)list_1[1]).Split(new char[] { '\"' })[1];
            DouBan_Id = ((string)list_1[1]).Split(new char[] { '\"' })[1];
            list_3 = myHttpWeb.PostOrGetCheck(DouBan_Check, HttpMethod.GET, (CookieContainer)list_1[0]);
            ImCheck.Dispatcher.BeginInvoke(new myDelegate(delegate_1), System.Windows.Threading.DispatcherPriority.Normal);
        }
        void delegate_1()
        {
            ImCheck.Source = new BitmapImage(new Uri(DouBan_Check));
            Lbyanzhengma.Visibility = Visibility.Hidden;
        }
        void delegate_2()
        {
            Lbyanzhengma.Visibility = Visibility.Visible;
        }
        void delegate_3()
        {
            File.WriteAllText(System.Windows.Forms.Application.StartupPath + "\\DouBanUser.Chengf", Cf_PassWordClass.Encrypt(TbUser.Text + "|" + Pb1.Password), Encoding.Unicode);
            melt.Source = new Uri(DouBan_SongHttpList[0]);
            melt.LoadedBehavior = MediaState.Manual;
            melt.Play();
            //为了避免开始界面加载速度过慢，跳过本地缓存检查
            //DouBan_SongCache();
            //使主面板上登录控件失效
            LbUser.IsEnabled = false;
            TbUser.IsEnabled = false;
            LbYanzhengma.IsEnabled = false;
            TbCheck.IsEnabled = false;
            Pb1.IsEnabled = false;
            ImCheck.IsEnabled = false;
            LbPassword.IsEnabled = false;
            BtZanTing.Visibility = Visibility.Visible;
            BtXiaZai.Visibility = Visibility.Visible;
            BtDeleteAll.Visibility = Visibility.Visible;
            BtLogin.Visibility = Visibility.Hidden;
            LbSongJinDu.Visibility = Visibility.Visible;
            LbSongContent.Visibility = Visibility.Visible;
            BtHuoQu.Visibility = Visibility.Visible;
            BtSuiJi.Visibility = Visibility.Visible;
            TbHuoQuCiShu.Visibility = Visibility.Visible;
            BtQieGe.Visibility = Visibility.Visible;
            SdVolume.Visibility = Visibility.Visible;
            SdPosition.Visibility = Visibility.Visible;
            RtGeQuJinDu.Visibility = Visibility.Visible;
            LbVolume.Visibility = Visibility.Visible;
            LbSongContent.Content = "总歌曲：" + DouBan_SongHttpList.Count.ToString() + "首，" + "当前播放的歌曲为：" + (string)DouBan_SongNameList[0];
            ListBoxSongName.Visibility = Visibility.Visible;
            List<string> DouBan_SongNameList_Copy = new List<string>();
            int i_1 = 1;//用来为ListBox中歌曲加标记数字
            foreach (string str in DouBan_SongNameList) { DouBan_SongNameList_Copy.Add(i_1.ToString() + "," + str + "（" + DouBan_SongGeShouNameList[i_1 - 1] + "）"); i_1++; }
            ListBoxSongName.ItemsSource = DouBan_SongNameList_Copy;
        }
        #endregion
        #region//实现界面无标题栏拖动，界面关闭、界面最小化、主线程退出、直接敲击Enter键实现登录或搜索按钮事件
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }//用于使鼠标能拖动Grid

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }//主窗口关闭
        private void BtMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }//主窗口最小化

        private void Window_Closed(object sender, EventArgs e)
        {
            if (melt.HasAudio)
            {
                melt.Close();
            }
            Directory.Delete(System.Windows.Forms.Application.StartupPath + "\\Cache", true);
            Environment.Exit(0);
        }//主线程彻底退出
        private void TbCheck_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtLogin_Click(null, null);
            }
        }

        private void TbSearch_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                BtSearch_Click(null, null);
            }
        }
        #endregion
        #region//播放器开始、结束事件及ListBox界面点击换歌

        private void melt_MediaEnded(object sender, RoutedEventArgs e)//实现歌曲播放完换歌
        {
            try
            {
                if (SuiJi)
                {
                    if (ii + 1 < SuiJi_SongNameList.Count)
                    {
                        ii++;
                        //melt.Source = new Uri(SuiJi_SongHttpList[ii]);
                        //melt.Play();
                        DouBan_SongCache();
                        LbSongContent.Content = "当前播放的歌曲为：" + (string)SuiJi_SongNameList[ii];
                    }
                    else
                    {
                        ii++;
                        Thread thread_SuiJi = new Thread(threadSuiJi);
                        thread_SuiJi.Start();
                    }
                    return;
                }
                if (i + 1 >= DouBan_SongHttpList.Count) { MessageBox.Show("红心列表歌曲已经播放完毕！想继续播放请点击获取更多按钮！", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
                i++;
                //melt.Source = new Uri(DouBan_SongHttpList[i]);
                //melt.Play();
                DouBan_SongCache();
                LbSongContent.Content = "总歌曲：" + DouBan_SongHttpList.Count.ToString() + "首，" + "当前播放的歌曲为：" + (string)DouBan_SongNameList[i];
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private void melt_MediaOpened(object sender, RoutedEventArgs e)//播放器打开时将歌曲总时长传递到slider控件
        {
            SdPosition.Maximum = melt.NaturalDuration.TimeSpan.Ticks;
            if (ListBoxSousuo.Visibility == Visibility.Visible) { return; }
            if (SuiJi)
            {
                LbSongContent.Content = "当前播放的歌曲为：" + (string)SuiJi_SongNameList[ii];
                if (SuiJi_SongLike[ii] == "1") { BtHongXing.Visibility = Visibility.Visible; BtHeiXing.Visibility = Visibility.Hidden; }
                else { BtHongXing.Visibility = Visibility.Hidden; BtHeiXing.Visibility = Visibility.Visible; }
            }
            else
            {
                LbSongContent.Content = "当前播放的歌曲为：" + (string)DouBan_SongNameList[i];
                if (DouBan_SongLike[i] == "1") { BtHongXing.Visibility = Visibility.Visible; BtHeiXing.Visibility = Visibility.Hidden; }
                else { BtHongXing.Visibility = Visibility.Hidden; BtHeiXing.Visibility = Visibility.Visible; }
            }

        }
        private void ListBoxSongName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (SuiJi)
                {
                    ii = ListBoxSongName.SelectedIndex;
                    DouBan_SongCache();
                    ListBoxSongName.SelectedItem = ii + 1 + "," + SuiJi_SongNameList[ii] + "（" + SuiJi_SongGeShouNameList[ii] + "）" + "...↓";
                    LbSongContent.Content = "当前播放的歌曲为：" + (string)SuiJi_SongNameList[ListBoxSongName.SelectedIndex];
                    return;
                }
                i = ListBoxSongName.SelectedIndex;
                //melt.Source = new Uri(DouBan_SongHttpList[ListBoxSongName.SelectedIndex]);
                DouBan_SongCache();
                //melt.Play();
                LbSongContent.Content = "总歌曲：" + DouBan_SongHttpList.Count.ToString() + "首，" + "当前播放的歌曲为：" + (string)DouBan_SongNameList[i];
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }//ListBox点击换歌
        #endregion
        #region//Slider控件拖动实现歌曲定位、音量控制以及界面实现界面聚焦
        private void SdVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)//控制媒体音量
        {
            melt.Volume = SdVolume.Value;
            SdVolume.ToolTip = (melt.Volume * 100).ToString("0");
            LbVolume.Content = (melt.Volume * 100).ToString("0");
        }
        private void SdPosition_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            g_timer.IsEnabled = false;
        }

        private void SdPosition_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            melt.Position = new TimeSpan((long)SdPosition.Value);
            g_timer.IsEnabled = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Tbfouse.Focus();
        }//实现界面聚焦
        #endregion
        #region//下载按钮、删除按钮、ListBoxSongName的GetFouce和LostFouce事件实现
        private void BtDeleteAll_Click(object sender, RoutedEventArgs e)//删除按钮
        {
            if (ListBoxSongName.ItemsSource == null) { MessageBoxResult mbr = MessageBox.Show("没有要删除的歌曲存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
            if (ListBoxSongName.SelectedIndex != -1)
            {
                if (SuiJi)
                {
                    SuiJi_SongGeShouNameList.RemoveAt(ListBoxSongName.SelectedIndex);
                    SuiJi_SongHttpList.RemoveAt(ListBoxSongName.SelectedIndex);
                    SuiJi_SongNameList.RemoveAt(ListBoxSongName.SelectedIndex);
                    SuiJi_SongLike.RemoveAt(ListBoxSongName.SelectedIndex);
                    SuiJi_SongSid.RemoveAt(ListBoxSongName.SelectedIndex);
                    listboxitem(SuiJi_SongNameList, SuiJi_SongGeShouNameList);
                    //List<string> SuiJi_SongHttpList_Copy = new List<string>();
                    //for (int k = 0; k < SuiJi_SongNameList.Count; k++)
                    //{
                    //    SuiJi_SongHttpList_Copy.Add(k + 1 + "," + SuiJi_SongNameList[k] + "（" + SuiJi_SongGeShouNameList[k] + "）");
                    //}
                    //ListBoxSongName.ItemsSource = SuiJi_SongHttpList_Copy;
                    ii--;
                }
                else
                {
                    DouBan_SongGeShouNameList.RemoveAt(ListBoxSongName.SelectedIndex);
                    DouBan_SongHttpList.RemoveAt(ListBoxSongName.SelectedIndex);
                    DouBan_SongNameList.RemoveAt(ListBoxSongName.SelectedIndex);
                    DouBan_SongLike.RemoveAt(ListBoxSongName.SelectedIndex);
                    DouBan_SongSid.RemoveAt(ListBoxSongName.SelectedIndex);
                    listboxitem(DouBan_SongNameList, DouBan_SongGeShouNameList);
                    //List<string> DouBan_SongHttpList_Copy = new List<string>();
                    //for (int k = 0; k < DouBan_SongNameList.Count; k++)
                    //{
                    //    DouBan_SongHttpList_Copy.Add(k + 1 + "," + DouBan_SongNameList[k] + "（" + DouBan_SongGeShouNameList[k] + "）");
                    //}
                    //ListBoxSongName.ItemsSource = DouBan_SongHttpList_Copy;
                    i--;
                }
                return;
            }
            if (SuiJi)
            {
                MessageBoxResult mbr = MessageBox.Show("您确定要清空所有现有歌曲吗？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
                if (mbr == MessageBoxResult.OK)
                {
                    ListBoxSongName.ItemsSource = null;
                    SuiJi_SongGeShouNameList.Clear();
                    SuiJi_SongHttpList.Clear();
                    SuiJi_SongNameList.Clear();
                    SuiJi_SongLike.Clear();
                    SuiJi_SongSid.Clear();
                }
            }
            else
            {
                MessageBoxResult mbr = MessageBox.Show("您确定要清空所有现有歌曲吗？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
                if (mbr == MessageBoxResult.OK)
                {
                    ListBoxSongName.ItemsSource = null;
                    DouBan_SongGeShouNameList.Clear();
                    DouBan_SongHttpList.Clear();
                    DouBan_SongNameList.Clear();
                    DouBan_SongLike.Clear();
                    DouBan_SongSid.Clear();
                }
            }
        }

        private void ListBoxSongName_GotFocus(object sender, RoutedEventArgs e)
        {
            BtDeleteAll.Content = "删除选中";
        }

        private void ListBoxSongName_LostFocus(object sender, RoutedEventArgs e)
        {
            BtDeleteAll.Content = "删除全部";
        }
        //下载按钮
        private void BtXiaZai_Click(object sender, RoutedEventArgs e)
        {
            if (SuiJi)
            {
                Thread threadXiaZaiSuiJi = new Thread(thread_XiaZaiSuiJi);
                threadXiaZaiSuiJi.Start();
                return;
            }
            Thread threadXiaZai = new Thread(thread_XiaZai);
            threadXiaZai.Start();
        }//下载按钮的实现，里面包含红心及随机两种模式下的下载方式
        void thread_XiaZai()//红心模式下
        {
            try
            {
                if (DouBan_SongHttpList.Count == 0) { MessageBox.Show("还未登录！", "提示！", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = DouBan_SongNameList[i];
                sfd.Filter = "音频文件|*.mp3";
                if (sfd.ShowDialog() != true) { return; }
                BtXiaZai.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegate_ZhengZaiXiaZai));
                if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(DouBan_SongNameList[i]) + ".mp3"))
                {
                    FileInfo fio = new FileInfo(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(DouBan_SongNameList[i]) + ".mp3");
                    if (fio.Length != 0)
                    {
                        File.Move(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(DouBan_SongNameList[i]) + ".mp3", sfd.FileName);
                    }
                }
                else
                {
                    if (!Cf_Web.IsConnectedToInternet()) { MessageBox.Show("网络连接失败！请检查您的网络是否连接正常！", "网络错误", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                    WebClient MyWebClient = new WebClient();
                    MyWebClient.DownloadFile(DouBan_SongHttpList[i], sfd.FileName);
                }
                BtXiaZai.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegate_XiaZaiWangCheng));
                MessageBox.Show("下载完成！", "提示！", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        // int progress;
        void thread_XiaZaiSuiJi()//随机模式下
        {
            if (SuiJi_SongHttpList.Count == 0) { MessageBox.Show("还未登录！", "提示！", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = SuiJi_SongNameList[ii];
            sfd.Filter = "音频文件|*.mp3";
            if (sfd.ShowDialog() != true) { return; }
            BtXiaZai.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegate_ZhengZaiXiaZai));
            //WebRequest mywebrquest = WebRequest.Create(SuiJi_SongHttpList[ii]);
            //using (WebResponse mywebrespose = mywebrquest.GetResponse())
            //{
            //    byte[] mybyte = new byte[1024];
            //    using (Stream mysteam = mywebrespose.GetResponseStream())
            //    {
            //        progress = mysteam.Read(mybyte, 0, mybyte.Length);
            //        while (progress > 0)
            //        {
            //            progress++;
            //            progress = mysteam.Read(mybyte, 0, mybyte.Length);
            //            
            //        }
            //    }

            // }

            if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(SuiJi_SongNameList[i]) + ".mp3"))
            {
                FileInfo fio = new FileInfo(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(SuiJi_SongNameList[i]) + ".mp3");
                if (fio.Length != 0)
                {
                    File.Move(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(SuiJi_SongNameList[i]) + ".mp3", sfd.FileName);
                }

            }
            else
            {
                if (!Cf_Web.IsConnectedToInternet()) { MessageBox.Show("网络连接失败！请检查您的网络是否连接正常！", "网络错误", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                WebClient MyWebClient = new WebClient();
                MyWebClient.DownloadFile(SuiJi_SongHttpList[ii], sfd.FileName);
            }
            BtXiaZai.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegate_XiaZaiWangCheng));
            MessageBox.Show("下载完成！", "提示！", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
        void delegate_ZhengZaiXiaZai()
        {
            BtXiaZai.Content = "正在下载";
            BtXiaZai.IsEnabled = false;
        }
        void delegate_XiaZaiWangCheng()
        {
            BtXiaZai.IsEnabled = true;
            BtXiaZai.Content = "下载";
        }
        #endregion
        #region//暂停、开始、切歌按钮方法的实现
        private void BtQieGe_Click(object sender, RoutedEventArgs e)//切歌按钮方法控制
        {
            if (SuiJi)
            {
                if (ii + 1 >= SuiJi_SongHttpList.Count) { MessageBox.Show("没有发现可以切换的下一首歌，请点击获取跟多按钮", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
                ii++;
                //melt.Source = new Uri(SuiJi_SongHttpList[ii]);
                //melt.Play();
                DouBan_SongCache();
            }
            else
            {
                if (i + 1 >= DouBan_SongHttpList.Count) { MessageBox.Show("没有发现可以切换的下一首歌，请点击获取跟多按钮", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
                i++;
                //melt.Source = new Uri(DouBan_SongHttpList[i]);
                //melt.Play();
                DouBan_SongCache();
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (melt.HasAudio)
            {
                melt.Pause();
                g_timer.Stop();
                BtZanTing.Visibility = Visibility.Hidden;
                BtKaiShi.Visibility = Visibility.Visible;
            }
            else { MessageBox.Show("无法使用该功能！", "错误", MessageBoxButton.OK, MessageBoxImage.Asterisk); }
        }//暂停按钮方法的实现

        private void BtKaiShi_Click(object sender, RoutedEventArgs e)
        {
            if (melt.HasAudio)
            {
                melt.Play();
                g_timer.Start();
                BtKaiShi.Visibility = Visibility.Hidden;
                BtZanTing.Visibility = Visibility.Visible;
            }
        }//开始按钮方法的实现
        #endregion
        #region//歌曲获取按钮方法实现
        int GeQuHuoQuCiShu = 0;//用来统计获取的次数
        int SuiJiGeQuHuoQuCiShu = 0;
        int Tbhuoqucishu = 1;
        private void BtHuoQu_Click(object sender, RoutedEventArgs e)
        {
            if (!Cf_Web.IsConnectedToInternet()) { MessageBox.Show("网络连接失败！请检查您的网络是否连接正常！", "网络错误", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            int result;
            if (int.TryParse(TbHuoQuCiShu.Text, out result) == false && TbHuoQuCiShu.Text != "") { MessageBox.Show("输入有误！", "提示", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (TbHuoQuCiShu.Text != "") { Tbhuoqucishu = int.Parse(TbHuoQuCiShu.Text); }
            if (SuiJi)
            {
                SuiJiGeQuHuoQuCiShu = 0;
                Thread thread_SuiJiHuoQu = new Thread(threadHuoQuSuiJi);
                thread_SuiJiHuoQu.Start();
                return;
            }
            GeQuHuoQuCiShu = 0;
            Thread thread_HuoQu = new Thread(threadHuoQu);
            thread_HuoQu.Start();
        }
        void threadHuoQu()//红心模式下获取更多
        {
            try
            {
                for (int y = 0; y < Tbhuoqucishu; y++)
                {
                    myHttpWeb.EncodingSet = "utf-8";
                    ArrayList list_5 = myHttpWeb.PostOrGet("http://douban.fm/j/mine/playlist?type=n&sid=280187&pt=0.4&channel=-3&pb=64&from=mainsite&r=70af77b33f", HttpMethod.GET, (CookieContainer)list_cookie[0]);
                    //MessageBox.Show("成功生成！即将播放音乐", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    string test1 = ((string)list_5[1]).Replace("\\/", "/");
                    List<string> SongHttpList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"url\":\"", "\",\""), 0, "\"url\":\"".Length), 0, "\",\"".Length);
                    List<string> SongNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"title\"", "\",\""), 0, "\"title\"".Length + 2), 0, "\",\"".Length);
                    List<string> SongGeShouNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"artist\"", "\",\""), 0, "\"artist\"".Length + 2), 0, "\",\"".Length);
                    List<string> SongLike = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"like\":", "}"), 0, "\"like\":".Length), 0, 1);
                    List<string> SongSid = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"sid\":\"", "\""), 0, "\"sid\":\"".Length), 0, 1);
                    #region//对歌曲进行去重及错误歌曲剔除
                    while (SongGeShouNameList.IndexOf("") != -1)
                    {
                        SongNameList.RemoveAt(SongGeShouNameList.IndexOf(""));
                        SongLike.RemoveAt(SongGeShouNameList.IndexOf(""));
                        SongSid.RemoveAt(SongGeShouNameList.IndexOf(""));
                        SongGeShouNameList.Remove("");
                    }
                    for (int k = 0; k < SongHttpList.Count; k++)//空白和非.mp3/.mp4链接地址抹除
                    {
                        if (SongHttpList[k].IndexOf(".mp3") == -1 && SongHttpList[k].IndexOf(".mp4") == -1)
                        {
                            SongHttpList.RemoveAt(k);
                            k--;
                        }
                    }
                    for (int j = 0; j < SongLike.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongLike.Add(SongLike[j]); }

                    }
                    for (int j = 0; j < SongSid.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongSid.Add(SongSid[j]); }

                    }
                    for (int j = 0; j < SongHttpList.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongHttpList.Add(SongHttpList[j]); }

                    }
                    for (int j = 0; j < SongGeShouNameList.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongGeShouNameList.Add(SongGeShouNameList[j]); }
                    }
                    for (int j = 0; j < SongNameList.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongNameList.Add(SongNameList[j]); }
                    }
                    #endregion
                    LbSongContent.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegateHuoQu));
                }
                LbSongContent.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegateHuoQu_1));
                Tbhuoqucishu = 1;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        void threadHuoQuSuiJi()//随机模式下的获取更多
        {
            try
            {
                for (int y = 0; y < Tbhuoqucishu; y++)
                {
                    SuiJi = true;
                    myHttpWeb.EncodingSet = "utf-8";
                    ArrayList list_5 = myHttpWeb.PostOrGet("http://douban.fm/j/mine/playlist?type=s&sid=231654&pt=45.7&channel=0&pb=64&from=mainsite&r=74800e8d67", HttpMethod.GET, (CookieContainer)list_cookie[0]);
                    //MessageBox.Show("成功生成！即将播放音乐", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    string test1 = ((string)list_5[1]).Replace("\\/", "/");
                    List<string> SongHttpList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"url\":\"", "\",\""), 0, "\"url\":\"".Length), 0, "\",\"".Length);
                    List<string> SongNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"title\"", "\",\""), 0, "\"title\"".Length + 2), 0, "\",\"".Length);
                    List<string> SongGeShouNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"artist\"", "\",\""), 0, "\"artist\"".Length + 2), 0, "\",\"".Length);
                    List<string> SongLike = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"like\":", "}"), 0, "\"like\":".Length), 0, 1);
                    List<string> SongSid = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"sid\":\"", "\""), 0, "\"sid\":\"".Length), 0, 1);
                    #region//对歌曲进行去重及错误歌曲剔除
                    while (SongGeShouNameList.IndexOf("") != -1)
                    {
                        SongNameList.RemoveAt(SongGeShouNameList.IndexOf(""));
                        SongLike.RemoveAt(SongGeShouNameList.IndexOf(""));
                        SongSid.RemoveAt(SongGeShouNameList.IndexOf(""));
                        SongGeShouNameList.Remove("");
                    }
                    for (int k = 0; k < SongHttpList.Count; k++)//空白和非.mp3/.mp4链接地址抹除
                    {
                        if (SongHttpList[k].IndexOf(".mp3") == -1 && SongHttpList[k].IndexOf(".mp4") == -1)
                        {
                            SongHttpList.RemoveAt(k);
                            k--;
                        }
                    }
                    for (int j = 0; j < SongLike.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in SuiJi_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { SuiJi_SongLike.Add(SongLike[j]); }

                    }
                    for (int j = 0; j < SongSid.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in SuiJi_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { SuiJi_SongSid.Add(SongSid[j]); }

                    }
                    for (int j = 0; j < SongHttpList.Count; j++)//歌曲链接地址去重
                    {
                        int j_1 = 0;
                        foreach (string str in SuiJi_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { SuiJi_SongHttpList.Add(SongHttpList[j]); }

                    }
                    for (int j = 0; j < SongGeShouNameList.Count; j++)//歌曲歌手名去重
                    {
                        int j_1 = 0;
                        foreach (string str in SuiJi_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { SuiJi_SongGeShouNameList.Add(SongGeShouNameList[j]); }
                    }
                    for (int j = 0; j < SongNameList.Count; j++)//歌曲名字去重
                    {
                        int j_1 = 0;
                        foreach (string str in SuiJi_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { SuiJi_SongNameList.Add(SongNameList[j]); }
                    }
                    #endregion
                    LbSongContent.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegateHuoQuSuiJi));
                }
                LbSongContent.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegateHuoQu_1));
                Tbhuoqucishu = 1;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        void delegateHuoQuSuiJi()//随机模式下获取更多按钮的委托
        {
            SuiJiGeQuHuoQuCiShu++;
            List<string> SuiJi_SongNameList_Copy = new List<string>();//此地方必须要重新开辟一个集合空间，然后赋值给ItemsSource
            int i_1 = 1;//用来为ListBox中歌曲加标记数字
            foreach (string str in SuiJi_SongNameList) { SuiJi_SongNameList_Copy.Add(i_1.ToString() + "," + str + "（" + SuiJi_SongGeShouNameList[i_1 - 1] + "）"); i_1++; }
            ListBoxSongName.ItemsSource = SuiJi_SongNameList_Copy;
            LbSongContent.Content = "总歌曲：" + SuiJi_SongHttpList.Count.ToString() + "首，" + "当前播放的歌曲为：" + (string)SuiJi_SongNameList[ii];
            BtHuoQu.Content = SuiJiGeQuHuoQuCiShu + "次！";
        }
        void delegateHuoQu()//红心模式下获取更多按钮的委托
        {
            GeQuHuoQuCiShu++;
            List<string> DouBan_SongNameList_Copy = new List<string>();//此地方必须要重新开辟一个集合空间，然后赋值给ItemsSource
            int i_1 = 1;//用来为ListBox中歌曲加标记数字
            foreach (string str in DouBan_SongNameList) { DouBan_SongNameList_Copy.Add(i_1.ToString() + "," + str + "（" + DouBan_SongGeShouNameList[i_1 - 1] + "）"); i_1++; }
            ListBoxSongName.ItemsSource = DouBan_SongNameList_Copy;
            LbSongContent.Content = "总歌曲：" + DouBan_SongHttpList.Count.ToString() + "首，" + "当前播放的歌曲为：" + (string)DouBan_SongNameList[i];
            BtHuoQu.Content = GeQuHuoQuCiShu + "次！";
        }
        void delegateHuoQu_1()//获取完毕后对按钮及获取框样式的控制
        {
            BtHuoQu.Content = "获取更多";
            TbHuoQuCiShu.Text = "";
        }
        #endregion
        #region//模式切换下的实现代码
        private void BtSuiJi_Click(object sender, RoutedEventArgs e)//模式切换按钮
        {
            if (SuiJi == false)
            {
                SuiJi = true;//进入随机模式
                Thread thread_SuiJi = new Thread(threadSuiJi);
                thread_SuiJi.Start();
            }
            else
            {
                SuiJi = false;//进入红心模式
                Thread thread_HongXin = new Thread(threadHongXin);
                thread_HongXin.Start();
            }
        }
        void threadSuiJi()
        {
            try
            {
                int SuiJi_SongHttpInt = SuiJi_SongHttpList.Count;
                do
                {
                    myHttpWeb.EncodingSet = "utf-8";
                    ArrayList list_5 = myHttpWeb.PostOrGet("http://douban.fm/j/mine/playlist?type=n&sid=231654&pt=45.7&channel=0&pb=64&from=mainsite&r=74800e8d67", HttpMethod.GET, (CookieContainer)list_cookie[0]);
                    //MessageBox.Show("成功生成！即将播放音乐", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    string test1 = ((string)list_5[1]).Replace("\\/", "/");
                    List<string> SongHttpList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"url\":\"", "\",\""), 0, "\"url\":\"".Length), 0, "\",\"".Length);
                    List<string> SongNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"title\"", "\",\""), 0, "\"title\"".Length + 2), 0, "\",\"".Length);
                    List<string> SongGeShouNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"artist\"", "\",\""), 0, "\"artist\"".Length + 2), 0, "\",\"".Length);
                    List<string> SongLike = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"like\":", "}"), 0, "\"like\":".Length), 0, 1);
                    List<string> SongSid = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"sid\":\"", "\""), 0, "\"sid\":\"".Length), 0, 1);
                    #region//对歌曲进行去重及错误歌曲剔除
                    if (SuiJi_SongNameList.Count != 0)
                    {
                        if (SongGeShouNameList.IndexOf("") != -1)
                        {
                            SongNameList.RemoveAt(SongGeShouNameList.IndexOf(""));
                            SongLike.RemoveAt(SongGeShouNameList.IndexOf(""));
                            SongSid.RemoveAt(SongGeShouNameList.IndexOf(""));
                            SongGeShouNameList.Remove("");
                        }
                        for (int k = 0; k < SongHttpList.Count; k++)//空白和非.mp3/.mp4链接地址抹除
                        {
                            if (SongHttpList[k].IndexOf(".mp3") == -1 && SongHttpList[k].IndexOf(".mp4") == -1)
                            {
                                SongHttpList.RemoveAt(k);
                                k--;
                            }
                        }
                        for (int j = 0; j < SongLike.Count; j++)//歌曲红心去重
                        {
                            int j_1 = 0;
                            foreach (string str in SuiJi_SongNameList)
                            {
                                if (str == SongNameList[j]) { j_1++; }
                            }
                            if (j_1 == 0) { SuiJi_SongLike.Add(SongLike[j]); }

                        }
                        for (int j = 0; j < SongSid.Count; j++)//歌曲Sid去重
                        {
                            int j_1 = 0;
                            foreach (string str in SuiJi_SongNameList)
                            {
                                if (str == SongNameList[j]) { j_1++; }
                            }
                            if (j_1 == 0) { SuiJi_SongSid.Add(SongSid[j]); }

                        }
                        for (int j = 0; j < SongHttpList.Count; j++)//歌曲链接地址去重
                        {
                            int j_1 = 0;
                            foreach (string str in SuiJi_SongNameList)
                            {
                                if (str == SongNameList[j]) { j_1++; }
                            }
                            if (j_1 == 0) { SuiJi_SongHttpList.Add(SongHttpList[j]); }

                        }
                        for (int j = 0; j < SongGeShouNameList.Count; j++)//歌曲歌手名去重
                        {
                            int j_1 = 0;
                            foreach (string str in SuiJi_SongNameList)
                            {
                                if (str == SongNameList[j]) { j_1++; }
                            }
                            if (j_1 == 0) { SuiJi_SongGeShouNameList.Add(SongGeShouNameList[j]); }
                        }
                        for (int j = 0; j < SongNameList.Count; j++)//歌曲名字去重
                        {
                            int j_1 = 0;
                            foreach (string str in SuiJi_SongNameList)
                            {
                                if (str == SongNameList[j]) { j_1++; }
                            }
                            if (j_1 == 0) { SuiJi_SongNameList.Add(SongNameList[j]); }
                        }
                    }
                    #endregion
                    else
                    {
                        SuiJi_SongHttpList = SongHttpList;
                        SuiJi_SongNameList = SongNameList;
                        SuiJi_SongGeShouNameList = SongGeShouNameList;
                        SuiJi_SongLike = SongLike;
                        SuiJi_SongSid = SongSid;
                        ii = 0;
                    }
                } while (SuiJi_SongHttpList.Count <= SuiJi_SongHttpInt);//防止生成的歌曲数为零
                LbSongContent.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegate_SuiJi));
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        void delegate_SuiJi()
        {
            //melt.Source = new Uri(SuiJi_SongHttpList[ii]);
            //melt.Play();
            DouBan_SongCache();
            //List<string> SuiJi_SongNameList_Copy = new List<string>();
            //int i_1 = 1;//用来为ListBox中歌曲加标记数字
            //foreach (string str in SuiJi_SongNameList) { SuiJi_SongNameList_Copy.Add(i_1.ToString() + "," + str + "（" + SuiJi_SongGeShouNameList[i_1 - 1] + "）"); i_1++; }
            //ListBoxSongName.ItemsSource = SuiJi_SongNameList_Copy;
            LbSongContent.Content = "当前播放的歌曲为：" + (string)SuiJi_SongNameList[ii];
        }

        void threadHongXin()//切换回红心模式需要的线程
        {
            try
            {
                myHttpWeb.EncodingSet = "utf-8";
                ArrayList list_5 = myHttpWeb.PostOrGet("http://douban.fm/j/mine/playlist?type=n&sid=280187&pt=0.4&channel=-3&pb=64&from=mainsite&r=70af77b33f", HttpMethod.GET, (CookieContainer)list_cookie[0]);
                //MessageBox.Show("成功生成！即将播放音乐", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                string test1 = ((string)list_5[1]).Replace("\\/", "/");
                List<string> SongHttpList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"url\":\"", "\",\""), 0, "\"url\":\"".Length), 0, "\",\"".Length);
                List<string> SongNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"title\"", "\",\""), 0, "\"title\"".Length + 2), 0, "\",\"".Length);
                List<string> SongGeShouNameList = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"artist\"", "\",\""), 0, "\"artist\"".Length + 2), 0, "\",\"".Length);
                List<string> SongLike = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"like\":", "}"), 0, "\"like\":".Length), 0, 1);
                List<string> SongSid = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ExtractString(test1, "\"sid\":\"", "\""), 0, "\"sid\":\"".Length), 0, 1);
                #region//对歌曲进行去重及错误歌曲剔除
                if (DouBan_SongNameList.Count != 0)
                {
                    if (SongGeShouNameList.IndexOf("") != -1)
                    {
                        SongNameList.RemoveAt(SongGeShouNameList.IndexOf(""));
                        SongLike.RemoveAt(SongGeShouNameList.IndexOf(""));
                        SongSid.RemoveAt(SongGeShouNameList.IndexOf(""));
                        SongGeShouNameList.Remove("");
                    }
                    for (int k = 0; k < SongHttpList.Count; k++)//空白和非.mp3/.mp4链接地址抹除
                    {
                        if (SongHttpList[k].IndexOf(".mp3") == -1 && SongHttpList[k].IndexOf(".mp4") == -1)
                        {
                            SongHttpList.RemoveAt(k);
                            k--;
                        }
                    }
                    for (int j = 0; j < SongLike.Count; j++)//歌曲红心去重
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongLike.Add(SongLike[j]); }

                    }
                    for (int j = 0; j < SongSid.Count; j++)//歌曲Sid去重
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongSid.Add(SongSid[j]); }

                    }
                    for (int j = 0; j < SongHttpList.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongHttpList.Add(SongHttpList[j]); }

                    }
                    for (int j = 0; j < SongGeShouNameList.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongGeShouNameList.Add(SongGeShouNameList[j]); }
                    }
                    for (int j = 0; j < SongNameList.Count; j++)
                    {
                        int j_1 = 0;
                        foreach (string str in DouBan_SongNameList)
                        {
                            if (str == SongNameList[j]) { j_1++; }
                        }
                        if (j_1 == 0) { DouBan_SongNameList.Add(SongNameList[j]); }
                    }
                }
                else
                {
                    DouBan_SongHttpList = SongHttpList;
                    DouBan_SongNameList = SongNameList;
                    DouBan_SongGeShouNameList = SongGeShouNameList;
                    DouBan_SongLike = SongLike;
                    DouBan_SongSid = SongSid;
                    i = 0;
                }
                #endregion
                LbSongContent.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new myDelegate(delegateHongXin_1));
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        void delegateHongXin_1()//切换回红心模式需要的线程下的委托
        {
            //melt.Source = new Uri(DouBan_SongHttpList[i]);
            //melt.Play();
            DouBan_SongCache();
            //List<string> DouBan_SongNameList_Copy = new List<string>();
            //int i_1 = 1;//用来为ListBox中歌曲加标记数字
            //foreach (string str in DouBan_SongNameList) { DouBan_SongNameList_Copy.Add(i_1.ToString() + "," + str + "（" + DouBan_SongGeShouNameList[i_1 - 1] + "）"); i_1++; }
            //ListBoxSongName.ItemsSource = DouBan_SongNameList_Copy;
            LbSongContent.Content = "当前播放的歌曲为：" + (string)DouBan_SongNameList[i];
        }
        #endregion
        #region//歌曲模式切换按钮的外观控制
        private void BtSuiJi_MouseEnter(object sender, MouseEventArgs e)
        {
            if (SuiJi)
            {
                BtSuiJi.Content = "红心模式";
            }
            else
            {
                BtSuiJi.Content = "随机模式";
            }
        }

        private void BtSuiJi_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SuiJi)
            {
                BtSuiJi.Content = "随机模式";
            }
            else
            {
                BtSuiJi.Content = "红心模式";
            }
        }
        #endregion
        #region//实现搜索按钮的下载歌曲功能
        bool tbsearch = false, bool_1 = true;//用来判断验证码是否已经提交
        string TbSearch_Text;
        string currenttime()//返回当前时间
        {
            return DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).TotalMilliseconds.ToString("0");
        }
        private void BtSearch_Click(object sender, RoutedEventArgs e)//用来实现歌曲搜索功能的按钮
        {
            TbSearch_Text = TbSearch.Text;
            Thread threadBtSearch = new Thread(Thread_BtSearch_Click);
            threadBtSearch.Start();
        }
        void Delegate_BtSearch_Click_1()
        {
            ImSongXiaZaiYangZhengMa.Visibility = Visibility.Visible;
            BtSearch.Content = "确定";
            TbSearch.Text = "请输入下方的验证码";
            TbSearch.Visibility = Visibility.Visible;
        }
        void Delegate_BtSearch_Click_2()
        {
            //用Image控件显示图片
            ImSongXiaZaiYangZhengMa.Source = new BitmapImage(new Uri("https://passport.baidu.com/cgi-bin/genimage?" + stringyzm));
        }
        void Delegate_BtSearch_Click_3()
        {
            //使图片容器消失
            ImSongXiaZaiYangZhengMa.IsEnabled = false;
            BtSearch.Content = "搜索";
            TbSearch.Visibility = Visibility.Hidden;
            TbSearch.Clear();
        }
        void Delegate_BtSearch_Click_3_1()
        {
            TbSearch.Clear();
            TbSearch.Focus();
        }
        void Delegate_BtSearch_Click_4()
        {
            //退出当前搜歌功能
            if (ListBoxSousuo.Visibility == Visibility.Visible)
            {
                ListBoxSousuo.Visibility = Visibility.Hidden;
                BtSearch.Content = "搜索";
                return;
            }
            //提供搜歌的TextBox
            TbSearch.Visibility = Visibility.Visible;
            TbSearch.Text = "请输入你要搜索的歌曲";
            BtSearch.Content = "确定";
            if (ListBoxSousuo.Visibility == Visibility.Visible)
            {
                ListBoxSousuo.Visibility = Visibility.Hidden;
            }
            bool_1 = false;
        }
        List<List<string>> BaiDu_SongHttpList = new List<List<string>>();//用于接受搜索歌曲的信息
        void Delegate_BtSearch_Click_5()
        {
            BaiDu_SongHttpList.Clear();
            BaiDu_SongHttpList = SongListHuoQu(TbSearch_Text);
            //判断搜索的歌曲是否存在
            if (boolsongcunzai)
            {
                BtSearch.Content = "搜索";
                TbSearch.Visibility = Visibility.Hidden;
                boolsongcunzai = false;
                bool_1 = true;
                return;
            }
            List<string> ZSongHttpList = new List<string>();
            for (int k = 0; k < BaiDu_SongHttpList[1].Count; k++)
            {
                ZSongHttpList.Add(BaiDu_SongHttpList[1][k] + "（" + BaiDu_SongHttpList[2][k] + "）");
            }
            ListBoxSousuo.ItemsSource = ZSongHttpList;
            ListBoxSousuo.IsEnabled = true;
            ListBoxSousuo.Visibility = Visibility.Visible;
            BtSearch.Content = "退出";
            TbSearch.Visibility = Visibility.Hidden;
            bool_1 = true;
        }
        void Thread_BtSearch_Click()
        {
            if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\Cookie.Chengf"))
            {
                #region//用来提交验证码
                //if (stringyzm == null)
                //{
                //    //使用委托1改变UI界面
                //    BtSearch.Dispatcher.BeginInvoke(new myDelegate(Delegate_BtSearch_Click_1), System.Windows.Threading.DispatcherPriority.Normal);
                //    //获取到验证码的地址
                //    BaiDu_YangZhengMaHuoQu();
                //    //使用委托2改变UI界面
                //    BtSearch.Dispatcher.BeginInvoke(new myDelegate(Delegate_BtSearch_Click_2), System.Windows.Threading.DispatcherPriority.Normal);
                //    return;
                //}
                if (!tbsearch)
                {
                    //赋值重要字段baiducookie值,同时检验输入的验证码是否正确
                    Chengf.Baidu_SongRequest newbaidusong = new Chengf.Baidu_SongRequest();
                    bool BaiDuCookie = newbaidusong.BaiDu_Cookie();
                    if (!BaiDuCookie)
                    {
                        // BaiDu_YangZhengMaHuoQu();
                        BtSearch.Dispatcher.BeginInvoke(new myDelegate(Delegate_BtSearch_Click_2), System.Windows.Threading.DispatcherPriority.Normal);
                        BtSearch.Dispatcher.BeginInvoke(new myDelegate(Delegate_BtSearch_Click_3_1), System.Windows.Threading.DispatcherPriority.Normal);
                        return;
                    }
                    baiducookie = newbaidusong.BaiDu_LoginCookie;
                    //使用委托3改变UI界面
                    BtSearch.Dispatcher.BeginInvoke(new myDelegate(Delegate_BtSearch_Click_3), System.Windows.Threading.DispatcherPriority.Normal);
                    //赋值重要字段baiducookie值
                    //BaiDuCookie(TbSearch_Text);
                    File.WriteAllText(System.Windows.Forms.Application.StartupPath + "\\Cookie.Chengf", Cf_PassWordClass.Encrypt(baiducookie));
                    tbsearch = true;
                    return;
                }
                #endregion
            }
            #region//用于搜索下载歌曲
            if (bool_1)
            {
                //使用委托4改变UI界面
                BtSearch.Dispatcher.BeginInvoke(new myDelegate(Delegate_BtSearch_Click_4), System.Windows.Threading.DispatcherPriority.Normal);
                return;
            }
            else if (!bool_1)
            {
                //使用委托5改变UI界面
                BtSearch.Dispatcher.BeginInvoke(new myDelegate(Delegate_BtSearch_Click_5), System.Windows.Threading.DispatcherPriority.Normal);
            }
            #endregion
        }
        private void TbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TbSearch.Clear();
        }
        bool boolsongcunzai = false;
        string baiducookie, stringtoken, stringyzm, stringBAIDUID, stringH_PS_PSSID, stringUBI;//用于以下各个cookie的存储
        void BaiDu_YangZhengMaHuoQu()//获取验证码地址stringyzm
        {
            Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
            string cookie_1 = BaiDu_Web.PostOrGet("http://www.baidu.com/", HttpMethod.GET)[0];
            stringBAIDUID = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_1, "BAIDUID", ";")[0], 0, 1).Insert("BAIDUID=".Length, "\"") + "\"";
            stringH_PS_PSSID = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_1, "H_PS_PSSID", ";")[0], 0, 1).Insert("H_PS_PSSID=".Length, "\"") + "\"";
            List<string> tokenlist = BaiDu_Web.PostOrGet("https://passport.baidu.com/v2/api/?getapi&tpl=mn&apiver=v3&tt=" + currenttime() + "&class=login&logintype=dialogLogin&callback=bd__cbs__gfhyis", HttpMethod.GET, stringBAIDUID + ";" + stringH_PS_PSSID);
            stringtoken = Cf_String.LastStringRemove(Cf_String.ExtractString(tokenlist[1], "token\" : \"", ",")[0].Remove(0, "token\" : \"".Length), 0, 2);
            //再次获取baiduid
            stringBAIDUID = Cf_String.LastStringRemove(Cf_String.ExtractString(tokenlist[0], "BAIDUID", ";")[0], 0, 1).Insert("BAIDUID=".Length, "\"") + "\"";
            List<string> list = BaiDu_Web.PostOrGet("https://passport.baidu.com/v2/api/?loginhistory&token=" + stringtoken + "&tpl=mn&apiver=v3&tt=" + currenttime() + "&callback=bd__cbs__gfhyis", HttpMethod.GET, stringBAIDUID + ";HOSUPPORT=1" + ";" + stringH_PS_PSSID);
            stringUBI = Cf_String.LastStringRemove(Cf_String.ExtractString(list[0], "UBI", ";")[0], 0, 1).Insert(4, "\"") + "\"";
            //再次获取baiduid
            stringBAIDUID = Cf_String.LastStringRemove(Cf_String.ExtractString(list[0], "BAIDUID", ";")[0], 0, 1).Insert("BAIDUID=".Length, "\"") + "\"";
            #region//POST提交数据
            string formdate = "charset=utf-8";
            formdate += "&token=" + stringtoken;
            formdate += "&tpl=mn";
            formdate += "&apiver=v3";
            formdate += "&tt=" + currenttime();
            formdate += "&codestring=";
            formdate += "&safeflg=0";
            formdate += "&u=" + "http://www.baidu.com/";
            formdate += "&isPhone=";
            formdate += "&quick_user=0";
            formdate += "&logintype=dialogLogin";
            formdate += "&logLoginType=pc_loginDialog";
            formdate += "&loginmerge=true";
            formdate += "&splogin=rate";
            formdate += "&username=space_god@126.com";
            formdate += "&password=baiduxiage";
            formdate += "&verifycode=";
            formdate += "&mem_pass=on";
            formdate += "&ppui_logintime=20339";
            formdate += "&callback=parent.bd__cbs__gfhyis";
            formdate += "&staticpage=http://www.baidu.com/cache/user/html/v3Jump.html";
            #endregion
            BaiDu_Web.UserDate = formdate;
            BaiDu_Web.ContentType = "application/x-www-form-urlencoded";
            List<string> cookie_3 = BaiDu_Web.PostOrGet("https://passport.baidu.com/v2/api/?login", HttpMethod.POST, stringBAIDUID + ";HOSUPPORT=1" + ";" + stringH_PS_PSSID + ";" + stringUBI);
            //验证码的获取地址
            stringyzm = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_3[1], "captchaservice", "&")[0], 0, 1);
        }
        bool BaiDuCookie()//用于获取百度重要的最终cookie用于下载歌曲,前提是需要获取验证码的ID
        {
            #region 旧版本
            Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
            #region//POST提交数据
            //string formdate = "charset=utf-8";
            //formdate += "&token=" + stringtoken;
            //formdate += "&tpl=mn";
            //formdate += "&apiver=v3";
            //formdate += "&tt=" + currenttime();
            //formdate += "&codestring=" + stringyzm;
            //formdate += "&safeflg=0";
            //formdate += "&u=" + "http://www.baidu.com/";
            //formdate += "&isPhone=";
            //formdate += "&quick_user=0";
            //formdate += "&logintype=dialogLogin";
            //formdate += "&logLoginType=pc_loginDialog";
            //formdate += "&loginmerge=true";
            //formdate += "&splogin=rate";
            //formdate += "&username=space_god@126.com";
            //formdate += "&password=baiduxiage";
            //formdate += "&verifycode=" + TbYangZhengMa;
            //formdate += "&mem_pass=on";
            //formdate += "&ppui_logintime=20339";
            //formdate += "&callback=parent.bd__cbs__gfhyis";
            //formdate += "&staticpage=http://www.baidu.com/cache/user/html/v3Jump.html";
            //#endregion
            //BaiDu_Web.UserDate = formdate;
            //BaiDu_Web.ContentType = "application/x-www-form-urlencoded";
            //List<string> cookie_4 = BaiDu_Web.PostOrGet("https://passport.baidu.com/v2/api/?login", HttpMethod.POST, stringBAIDUID + ";HOSUPPORT=1" + ";" + stringH_PS_PSSID + ";" + stringUBI);
            //string param = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_4[1], "Param", "&")[0], 0, 1).Remove(0, "Param=".Length);
            ////此处获取的cookie十分重要，用于直接下载歌曲
            #endregion
            #endregion
            Chengf.Baidu_SongRequest newbaidusong = new Chengf.Baidu_SongRequest();
            newbaidusong.BaiDu_Cookie();
            baiducookie = newbaidusong.BaiDu_LoginCookie;
            if (baiducookie == "##$") { MessageBox.Show("验证码错误", "提示", MessageBoxButton.OK, MessageBoxImage.Error); BaiDu_YangZhengMaHuoQu(); return false; }
            return true;
        }
        List<List<string>> SongListHuoQu(string SongName)//用来获取搜索歌曲网页列表
        {
            Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
            BaiDu_Web.EncodingSet = "utf-8";
            string searchweb = BaiDu_Web.PostOrGet("http://music.baidu.com/search?key=" + Cf_PassWordClass.UrlEncode(SongName), HttpMethod.GET)[1];
            List<string> songlistLi = Cf_String.ExtractString(searchweb, "<li data-songitem =", "</li>");
            List<List<string>> TotalSongList = new List<List<string>>();//歌曲的所有信息,0中存储歌曲的下载地址
            TotalSongList.Add(new List<string>());
            TotalSongList.Add(new List<string>());
            TotalSongList.Add(new List<string>());
            foreach (string str in songlistLi)
            {
                if (str.IndexOf("该资源来自其他网站") == -1)
                {
                    //获取歌曲下载地址
                    TotalSongList[0].Add("http://music.baidu.com/song/" + Cf_String.LastStringRemove(Cf_String.ExtractString(str, "sid", ",")[0].Remove(0, "sid&quot;:".Length), 0, 1) + "/download?__o=%2Fsearch");
                    //获取歌曲名
                    TotalSongList[1].Add(Cf_String.LastStringRemove(Cf_String.ExtractString(Cf_String.ExtractString(str, "data-songdata=", "\">")[0], "title=\"", "\"")[0].Remove(0, "title=\"".Length), 0, 1));
                    //获取歌曲作者名
                    TotalSongList[2].Add(Cf_String.LastStringRemove(Cf_String.ExtractString(str, "<span class=\"author_list\" title=\"", "\"")[0].Remove(0, "<span class=\"author_list\" title=\"".Length), 0, 1));
                }
            }
            if (TotalSongList[0].Count == 0) { MessageBox.Show("没有这首歌可供下载", "", MessageBoxButton.OK, MessageBoxImage.Asterisk); boolsongcunzai = true; }
            return TotalSongList;
        }
        void SongDownload(string songhttp)//传入一个歌曲的下载地址，完成cookies的配置和下载
        {
            Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
            List<string> cookie_6 = BaiDu_Web.PostOrGet(songhttp, HttpMethod.GET, baiducookie);
            string songdownload = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_6[1], "http://zhangmenshiting.baidu.com", "&")[0], 0, 1);
            WebClient mywebclient = new WebClient();
            mywebclient.DownloadFile(songdownload, @"C:\Users\chengf_Pc\Desktop\" + TbSearch_Text + ".mp3");
            MessageBox.Show("下载完毕", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
        private void ListBoxSousuo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
            if (ListBoxSousuo.SelectedIndex == -1) { return; }
            BaiDu_Web.EncodingSet = "utf-8";
            List<string> cookie_6 = BaiDu_Web.PostOrGet(BaiDu_SongHttpList[0][ListBoxSousuo.SelectedIndex], HttpMethod.GET, baiducookie);
            //检验cookie是否过期
            if (cookie_6[1].IndexOf("登录") != -1)
            {
                MessageBox.Show("登录信息过期，确定后将自动重新获取", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
                Chengf.Baidu_SongRequest jiangyou = new Baidu_SongRequest();
                jiangyou.BaiDu_Cookie();
                baiducookie = jiangyou.BaiDu_LoginCookie;
                cookie_6 = BaiDu_Web.PostOrGet(BaiDu_SongHttpList[0][ListBoxSousuo_SelectedIndex], HttpMethod.GET, baiducookie);
                File.Delete(System.Windows.Forms.Application.StartupPath + "//Cookie.Chengf");
                File.WriteAllText(System.Windows.Forms.Application.StartupPath + "//Cookie.Chengf", Cf_PassWordClass.Encrypt(baiducookie));
               // return;
            }
            string songdownload = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_6[1], "http://yinyueshiting.baidu.com", "\"")[0], 0, 1);
            melt.Source = new Uri(songdownload);
            melt.LoadedBehavior = MediaState.Manual;
            melt.Play();
        }
        int ListBoxSousuo_SelectedIndex;
        private void BtSouSuoXiaZai_Click(object sender, RoutedEventArgs e)
        {
            ListBoxSousuo_SelectedIndex = ListBoxSousuo.SelectedIndex;
            Thread threadSouSuoXiaZai = new Thread(Thread_BtSouSuoXiaZai_Click);
            threadSouSuoXiaZai.Start();
        }
        void Thread_BtSouSuoXiaZai_Click()
        {
            if (ListBoxSousuo_SelectedIndex == -1) { MessageBox.Show("先在列表中选择一个下载的歌曲", "下歌提示", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
            MessageBoxResult mbr = MessageBox.Show("你想要下载它吗？", "下歌提示", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
            if (mbr == MessageBoxResult.OK)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "音频|*.mp3";
                sfd.FileName = BaiDu_SongHttpList[1][ListBoxSousuo_SelectedIndex] + "（" + BaiDu_SongHttpList[2][ListBoxSousuo_SelectedIndex] + "）";
                if ((bool)sfd.ShowDialog())
                {
                    BtSouSuoXiaZai.Dispatcher.BeginInvoke(new myDelegate(Delegate_Thread_BtSouSuoXiaZai_Click_1), System.Windows.Threading.DispatcherPriority.Normal);
                  List<string> cookie_6 = BaiDu_Web.PostOrGet(BaiDu_SongHttpList[0][ListBoxSousuo_SelectedIndex], HttpMethod.GET, baiducookie);
                    //检验cookie是否过期
                    if (cookie_6[1].IndexOf("登录") != -1)
                    {
                        MessageBox.Show("登录信息过期，确定后将自动重新获取", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        Chengf.Baidu_SongRequest jiangyou = new Baidu_SongRequest();
                        jiangyou.BaiDu_Cookie();
                        baiducookie = jiangyou.BaiDu_LoginCookie;
                        cookie_6 = BaiDu_Web.PostOrGet(BaiDu_SongHttpList[0][ListBoxSousuo_SelectedIndex], HttpMethod.GET, baiducookie);
                      //  File.Delete(System.Windows.Forms.Application.StartupPath + "//Cookie.Chengf");
                        File.Delete(System.Windows.Forms.Application.StartupPath + "//Cookie.Chengf");
                        File.WriteAllText(System.Windows.Forms.Application.StartupPath + "//Cookie.Chengf", Cf_PassWordClass.Encrypt(baiducookie));
                        //ListBoxSousuo.Visibility = Visibility.Hidden;
                        //BtSearch.Content = "验证";
                        //stringyzm = null;
                        //tbsearch = false;
                        //return;
                    }
                    string songdownload = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_6[1], "http://yinyueshiting.baidu.com", "\"")[0], 0, 1);
                    WebClient mywebclient = new WebClient();
                    mywebclient.DownloadFile(songdownload, sfd.FileName);
                    BtSouSuoXiaZai.Dispatcher.BeginInvoke(new myDelegate(Delegate_Thread_BtSouSuoXiaZai_Click_2), System.Windows.Threading.DispatcherPriority.Normal);
                    MessageBox.Show("下载完毕", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
        }
        void Delegate_Thread_BtSouSuoXiaZai_Click_1()
        {
            BtSouSuoXiaZai.Content = "正在下载";
            BtSouSuoXiaZai.IsEnabled = false;
        }
        void Delegate_Thread_BtSouSuoXiaZai_Click_2()
        {
            BtSouSuoXiaZai.Content = "下载";
            BtSouSuoXiaZai.IsEnabled = true;
        }

        private void ListBoxSousuo_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                BtSouSuoXiaZai.Visibility = Visibility.Visible;
            }
            else if (!(bool)e.NewValue)
            {
                BtSouSuoXiaZai.Visibility = Visibility.Hidden;
            }

        }
        #endregion
        #region//对当前歌曲标志为红心或者取消红心
        private void BtHongXing_Click(object sender, RoutedEventArgs e)
        {
            if (SuiJi) { if (SuiJi_SongNameList.Count == 0) { MessageBox.Show("无法进行此操作，目标歌曲已经不存在！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error); return; } }
            else { if (DouBan_SongNameList.Count == 0) { MessageBox.Show("无法进行此操作，目标歌曲已经不存在！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error); return; } }
            BtHongXing.Visibility = Visibility.Hidden;
            BtHeiXing.Visibility = Visibility.Visible;
            BtHeiXing.IsEnabled = false;
            Thread newthread = new Thread(DouBan_HeiXingState);
            newthread.Start();

        }
        private void BtHeiXing_Click(object sender, RoutedEventArgs e)
        {
            if (SuiJi) { if (SuiJi_SongNameList.Count == 0) { MessageBox.Show("无法进行加此操作，目标歌曲已经不存在！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error); return; } }
            else { if (DouBan_SongNameList.Count == 0) { MessageBox.Show("无法进行此操作，目标歌曲已经不存在！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error); return; } }
            BtHongXing.Visibility = Visibility.Visible;
            BtHeiXing.Visibility = Visibility.Hidden;
            BtHongXing.IsEnabled = false;
            Thread newthread = new Thread(DouBan_HongXingState);
            newthread.Start();

        }
        void delegate_BtHongXing_Click()
        {
            Lbchannel.Content = "已撤销红心！";
            Lbchannel.Visibility = Visibility.Visible;
            BtHongXing.IsEnabled = true;
        }
        void delegate_BtHeiXing_Click()
        {
            Lbchannel.Content = "已加入红心！";
            Lbchannel.Visibility = Visibility.Hidden;
            BtHeiXing.IsEnabled = true;
        }
        void DouBan_HongXingState()
        {
            if (SuiJi)
            {
                myHttpWeb.EncodingSet = "utf-8";
                myHttpWeb.PostOrGet("http://douban.fm/j/mine/playlist?type=r&sid=" + SuiJi_SongSid[ii] + "&pt=3.3&channel=0&pb=64&from=mainsite&r=fc004f85df", HttpMethod.GET, (CookieContainer)list_cookie[0]);
                SuiJi_SongLike[ii] = "1";
            }
            else
            {
                myHttpWeb.EncodingSet = "utf-8";
                myHttpWeb.PostOrGet("http://douban.fm/j/mine/playlist?type=r&sid=" + DouBan_SongSid[i] + "&pt=3.3&channel=-3&pb=64&from=mainsite&r=fc004f85df", HttpMethod.GET, (CookieContainer)list_cookie[0]);
                DouBan_SongLike[i] = "1";
            }
            BtHongXing.Dispatcher.BeginInvoke(new myDelegate(delegate_BtHongXing_Click), System.Windows.Threading.DispatcherPriority.Normal);
        }
        void DouBan_HeiXingState()
        {
            if (SuiJi)
            {
                myHttpWeb.EncodingSet = "utf-8";
                myHttpWeb.PostOrGet("http://douban.fm/j/mine/playlist?type=u&sid=" + SuiJi_SongSid[ii] + "&pt=3.3&channel=0&pb=64&from=mainsite&r=fc004f85df", HttpMethod.GET, (CookieContainer)list_cookie[0]);
                SuiJi_SongLike[ii] = "0";
            }
            else
            {
                myHttpWeb.EncodingSet = "utf-8";
                myHttpWeb.PostOrGet("http://douban.fm/j/mine/playlist?type=u&sid=" + DouBan_SongSid[i] + "&pt=3.3&channel=-3&pb=64&from=mainsite&r=fc004f85df", HttpMethod.GET, (CookieContainer)list_cookie[0]);
                DouBan_SongLike[i] = "0";
            }
            BtHongXing.Dispatcher.BeginInvoke(new myDelegate(delegate_BtHeiXing_Click), System.Windows.Threading.DispatcherPriority.Normal);
        }
        void DouBan_Channel()
        {
            Cf_HttpWeb myhttpweb = new Cf_HttpWeb();
            myhttpweb.EncodingSet = "utf-8";
            string stringchannel = myhttpweb.PostOrGet("http://douban.fm/", HttpMethod.GET)[1];
            string redChannel = Cf_String.LastStringRemove(Cf_String.ExtractString(Cf_String.LastExtractString(stringchannel, "<a class=\"chl_name\"><span class='cname'>我的私人兆赫", "cid")[0], "cid=\"", "\"")[0].Remove(0, "cid=\"".Length), 0, 1);
            string blackChannel = Cf_String.LastStringRemove(Cf_String.ExtractString(Cf_String.LastExtractString(stringchannel, "<a class=\"chl_name\"><span class='cname'>我的红心兆赫", "cid")[0], "cid=\"", "\"")[0].Remove(0, "cid=\"".Length), 0, 1);
            DouBan_redchannel = redChannel;
            DouBan_blackchannel = blackChannel;
        }
        #endregion
        #region//歌曲缓存的实现
        void DouBan_SongCache()
        {
            try
            {
                if (SuiJi)
                {
                    if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(SuiJi_SongNameList[ii]) + ".mp3"))
                    {
                        if (!Cf_Web.IsConnectedToInternet()) { MessageBox.Show("网络连接失败！请检查您的网络是否连接正常！", "网络错误", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        //WebClient mywebclient = new WebClient();
                        //mywebclient.DownloadFile(SuiJi_SongHttpList[ii], System.Windows.Forms.Application.StartupPath + "Cache\\" + SuiJi_SongNameList[ii] + ".mp3");
                        Thread mythread = new Thread(thread_DouBan_FileDownload_SuiJi);
                        mythread.Start();
                        melt.Source = new Uri(SuiJi_SongHttpList[ii]);
                        melt.LoadedBehavior = MediaState.Manual;
                        melt.Play();
                        listboxitem(SuiJi_SongNameList, SuiJi_SongGeShouNameList);
                    }
                    else
                    {
                        melt.Source = new Uri(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(SuiJi_SongNameList[ii]) + ".mp3");
                        melt.LoadedBehavior = MediaState.Manual;
                        melt.Play();
                        listboxitem(SuiJi_SongNameList, SuiJi_SongGeShouNameList);
                    }
                }
                else
                {
                    if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(DouBan_SongNameList[i]) + ".mp3"))
                    {
                        if (!Cf_Web.IsConnectedToInternet()) { MessageBox.Show("网络连接失败！请检查您的网络是否连接正常！", "网络错误", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        //WebClient mywebclient = new WebClient();
                        //mywebclient.DownloadFile(DouBan_SongHttpList[i], System.Windows.Forms.Application.StartupPath + "Cache\\" + DouBan_SongNameList[i] + ".mp3");
                        Thread mythread = new Thread(thread_DouBan_FileDownload_HongXing);
                        mythread.Start();
                        melt.Source = new Uri(DouBan_SongHttpList[i]);
                        melt.LoadedBehavior = MediaState.Manual;
                        melt.Play();
                        listboxitem(DouBan_SongNameList, DouBan_SongGeShouNameList);
                    }
                    else
                    {
                        melt.Source = new Uri(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(DouBan_SongNameList[i]) + ".mp3");
                        melt.LoadedBehavior = MediaState.Manual;
                        melt.Play();
                        listboxitem(DouBan_SongNameList, DouBan_SongGeShouNameList);
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "缓存处出错提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        void thread_DouBan_FileDownload_SuiJi()
        {
            WebClient mywebclient = new WebClient();
            mywebclient.DownloadFile(SuiJi_SongHttpList[ii], System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(SuiJi_SongNameList[ii]) + ".mp3");
        }
        void thread_DouBan_FileDownload_HongXing()
        {
            WebClient mywebclient = new WebClient();
            mywebclient.DownloadFile(DouBan_SongHttpList[i], System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(DouBan_SongNameList[i]) + ".mp3");
        }
        //检查到本地是否存在与SongNameList相同的歌曲，如果存在就以特定格式显示到ListBox上
        void listboxitem(List<string> SongNameList, List<string> SongGeShouNameList)
        {
            List<string> list_cache = new List<string>();
            string[] stringlistcache = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\Cache");
            if (stringlistcache.Length == 0)
            {
                MessageBox.Show("没有文件", "测试", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            for (int k = 0; k < SongNameList.Count; k++)
            {
                bool bool1 = false;
                for (int k1 = 0; k1 < stringlistcache.Length; k1++)
                {
                    if (stringlistcache[k1] == System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(SongNameList[k]) + ".mp3")
                    {
                        list_cache.Add(k + 1 + "," + SongNameList[k] + "（" + SongGeShouNameList[k] + "）" + "...↓");
                        bool1 = true;
                        break;
                    }
                }
                if (bool1) { continue; }
                list_cache.Add(k + 1 + "," + SongNameList[k] + "（" + SongGeShouNameList[k] + "）");
            }
            ListBoxSongName.ItemsSource = list_cache;
        }

        private void melt_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show("歌曲文件损坏或者不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            if (SuiJi)
            {
                File.Delete(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(SuiJi_SongNameList[ii]) + ".mp3");
            }
            else
            {
                File.Delete(System.Windows.Forms.Application.StartupPath + "\\Cache\\" + Cf_PassWordClass.Encrypt(DouBan_SongNameList[i]) + ".mp3");
            }

        }
        #endregion
    }
}
