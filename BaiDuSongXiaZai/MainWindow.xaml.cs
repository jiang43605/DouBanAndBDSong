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
using Chengf;
using System.IO;
using System.Net;
using System.Threading;

namespace BaiDuSongXiaZai
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BaiDuSongXiaZai();

        }
        string currenttime()//返回当前时间
        {
            return DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).TotalMilliseconds.ToString("0");
        }
        
        string stringtoken,stringyzm, stringBAIDUID, stringH_PS_PSSID, stringUBI;
        void BaiDuSongXiaZai()
        {
            Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
            string cookie_1 = BaiDu_Web.PostOrGet("http://www.baidu.com/", HttpMethod.GET)[0];
            stringBAIDUID = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_1, "BAIDUID", ";")[0], 0, 1).Insert("BAIDUID=".Length, "\"") + "\"";
            stringH_PS_PSSID = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_1, "H_PS_PSSID", ";")[0], 0, 1).Insert("H_PS_PSSID=".Length, "\"") + "\"";
            BaiDu_Web.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)";
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
            formdate += "&username=jiang43605";
            formdate += "&password=jiang63020533";
            formdate += "&verifycode=";
            formdate += "&mem_pass=on";
            formdate += "&ppui_logintime=20339";
            formdate += "&callback=parent.bd__cbs__gfhyis";
            formdate += "&staticpage=http://www.baidu.com/cache/user/html/v3Jump.html";
            #endregion
            BaiDu_Web.UserDate = formdate;
            BaiDu_Web.ContentType = "application/x-www-form-urlencoded";
            List<string> cookie_3 = BaiDu_Web.PostOrGet("https://passport.baidu.com/v2/api/?login", HttpMethod.POST, stringBAIDUID + ";HOSUPPORT=1" + ";" + stringH_PS_PSSID + ";" + stringUBI);
            stringyzm = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_3[1], "captchaservice", "&")[0], 0, 1);
            ImCheack.Source = new BitmapImage(new Uri("https://passport.baidu.com/cgi-bin/genimage?" + stringyzm));
        }
        void BaiDuLogin()
        {
            Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
            #region//POST提交数据
            string formdate = "charset=utf-8";
            formdate += "&token=" + stringtoken;
            formdate += "&tpl=mn";
            formdate += "&apiver=v3";
            formdate += "&tt=" + currenttime();
            formdate += "&codestring=" + stringyzm;
            formdate += "&safeflg=0";
            formdate += "&u=" + "http://www.baidu.com/";
            formdate += "&isPhone=";
            formdate += "&quick_user=0";
            formdate += "&logintype=dialogLogin";
            formdate += "&logLoginType=pc_loginDialog";
            formdate += "&loginmerge=true";
            formdate += "&splogin=rate";
            formdate += "&username=jiang43605";
            formdate += "&password=jiang63020533";
            formdate += "&verifycode=" + TbInput.Text;
            formdate += "&mem_pass=on";
            formdate += "&ppui_logintime=20339";
            formdate += "&callback=parent.bd__cbs__gfhyis";
            formdate += "&staticpage=http://www.baidu.com/cache/user/html/v3Jump.html";
            #endregion
            BaiDu_Web.UserDate = formdate;
            BaiDu_Web.ContentType = "application/x-www-form-urlencoded";
            List<string> cookie_4 = BaiDu_Web.PostOrGet("https://passport.baidu.com/v2/api/?login", HttpMethod.POST, stringBAIDUID + ";HOSUPPORT=1" + ";" + stringH_PS_PSSID + ";" + stringUBI);
            string param = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_4[1], "Param", "&")[0], 0, 1).Remove(0, "Param=".Length);
            try
            {
                cookie_5 = BaiDu_Web.PostOrGet("http://user.hao123.com/static/crossdomain.php?bdu=" + param + "&t=" + currenttime(), HttpMethod.GET, cookie_4[0]);
                BaiDu_Web.EncodingSet = "utf-8";
                string searchweb = BaiDu_Web.PostOrGet("http://music.baidu.com/search?key=" + Cf_PassWordClass.UrlEncode(TbSong.Text), HttpMethod.GET)[1];
                List<string> songlist = Cf_String.ExtractString(searchweb, "<li data-songitem =", "</li>");
                List<string> songlistFilt = new List<string>();
                foreach (string str in songlist)
                {
                    if (str.IndexOf("该资源来自其他网站") == -1)
                    {
                        songlistFilt.Add(str);
                    }
                }
                if (songlistFilt.Count == 0) { MessageBox.Show("没有这首歌可供下载", "", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
                songlistFilt = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ListExtractString(songlistFilt, "sid", ","), 0, "sid&quot;:".Length), 0, 1);
                List<string> cookie_6 = BaiDu_Web.PostOrGet("http://music.baidu.com/song/" + songlistFilt[0] + "/download?__o=%2Fsearch", HttpMethod.GET, cookie_5[0]);
                string xcode = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_6[1], "http://zhangmenshiting.baidu.com", "&")[0], 0, 1);
                WebClient mywebclient = new WebClient();
                mywebclient.DownloadFile(xcode, System.Windows.Forms.Application.StartupPath + "\\" + TbSong.Text + ".mp3");
                MessageBox.Show("下载完毕", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                TbSong.Clear();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        List<string> cookie_5 = new List<string>();

        private void Bt2_Click(object sender, RoutedEventArgs e)
        {
            BtLogion.IsEnabled = false;
            BtXiaZai.IsEnabled = false;
            ImCheack.Visibility = Visibility.Hidden;
            TbInput.Visibility = Visibility.Hidden;
            BaiDuLogin();
            
        }

        private void BtCongXia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
                BaiDu_Web.EncodingSet = "utf-8";
                string searchweb = BaiDu_Web.PostOrGet("http://music.baidu.com/search?key=" + Cf_PassWordClass.UrlEncode(TbSong.Text), HttpMethod.GET)[1];
                List<string> songlist = Cf_String.ExtractString(searchweb, "<li data-songitem =", "</li>");
                List<string> songlistFilt = new List<string>();
                foreach (string str in songlist)
                {
                    if (str.IndexOf("该资源来自其他网站") == -1)
                    {
                        songlistFilt.Add(str);
                    }
                }
                if (songlistFilt.Count == 0) { MessageBox.Show("没有这首歌可供下载", "", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
                songlistFilt = Cf_String.LastListRemove(Cf_String.ListRemove(Cf_String.ListExtractString(songlistFilt, "sid", ","), 0, "sid&quot;:".Length), 0, 1);
                //以上获取要下载歌曲的字符串值
                List<string> cookie_6 = BaiDu_Web.PostOrGet("http://music.baidu.com/song/" + songlistFilt[0] + "/download?__o=%2Fsearch", HttpMethod.GET, cookie_5[0]);
                string xcode = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_6[1], "http://zhangmenshiting.baidu.com", "&")[0], 0, 1);
                WebClient mywebclient = new WebClient();
                mywebclient.DownloadFile(xcode, System.Windows.Forms.Application.StartupPath +"\\"+ TbSong.Text + ".mp3");
                MessageBox.Show("下载完毕", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                TbSong.Clear();
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
