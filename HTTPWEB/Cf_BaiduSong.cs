using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chengf
{
    /// <summary>
    /// 通过一定的设置，从Baidu获取歌曲的下载地址
    /// </summary>
    public class Cf_BaiduSong
    {
        bool boolsongcunzai = false;
        string baiducookie, stringtoken, stringyzm, stringBAIDUID, stringH_PS_PSSID, stringUBI;
        string baidu_username = "space_god@126.com", baidu_password = "baiduxiage";
        /// <summary>
        /// 对BaiDu帐号设置，如果不设则采用默认值（注意必须要与相应的密码搭配）
        /// </summary>
        public string BaiDu_UserName { set { baidu_username = value; } get { return baidu_username; } }
        /// <summary>
        /// 对BaiDu密码设置，如果不设则采用默认值（注意必须要与相应的帐号搭配）
        /// </summary>
        public string BaiDu_PassWord { set { baidu_password = value; } get { return baidu_password; } }
        /// <summary>
        /// 获取或设置BaiDu的登录Cookie
        /// </summary>
        public string BaiDu_LoginCookie { set { baiducookie = value; } get { return baiducookie; } }
        /// <summary>
        /// 返回一个当前距1970的毫秒数字符串
        /// </summary>
        /// <returns>返回一个当前距1970的毫秒数字符串</returns>
        string currenttime()
        {
            return DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).TotalMilliseconds.ToString("0");
        }
        /// <summary>
        /// 获取Baidu验证码链接地址
        /// </summary>
        /// <returns>返回一个Baidu验证码的http连接地址字符串</returns>
        public string BaiDu_YangZhengMaHuoQu()
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
            formdate += "&username=" + baidu_username;
            formdate += "&password=" + baidu_password;
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
            return "https://passport.baidu.com/cgi-bin/genimage?" + stringyzm;
        }
        /// <summary>
        /// 用于获取BaiDu最终LoginCookie用于下载歌曲,前提是需要获取验证码
        /// </summary>
        /// <param name="TbYangZhengMa">验证码</param>
        /// <returns>返回一个值指示否成功获取到，获取到则为true,否则为false</returns>
        public bool BaiDu_Cookie(string TbYangZhengMa)
        {
            #region 旧版本
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
            formdate += "&username=" + baidu_username;
            formdate += "&password=" + baidu_password;
            formdate += "&verifycode=" + TbYangZhengMa;
            formdate += "&mem_pass=on";
            formdate += "&ppui_logintime=20339";
            formdate += "&callback=parent.bd__cbs__gfhyis";
            formdate += "&staticpage=http://www.baidu.com/cache/user/html/v3Jump.html";
            #endregion
            BaiDu_Web.UserDate = formdate;
            BaiDu_Web.ContentType = "application/x-www-form-urlencoded";
            List<string> cookie_4 = BaiDu_Web.PostOrGet("https://passport.baidu.com/v2/api/?login", HttpMethod.POST, stringBAIDUID + ";HOSUPPORT=1" + ";" + stringH_PS_PSSID + ";" + stringUBI);
            string param = Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_4[1], "Param", "&")[0], 0, 1).Remove(0, "Param=".Length);
            //此处获取的cookie十分重要，用于直接下载歌曲
            #endregion
            return true;
        }
        /// <summary>
        /// 输入歌曲名，获得一个歌曲下载列表
        /// </summary>
        /// <param name="SongName">歌曲名</param>
        /// <returns>包含歌曲下载地址，歌曲名，歌手</returns>
        public List<List<string>> BaiDu_SongListHuoQu(string SongName)
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
                    TotalSongList[0].Add(SongDownload("http://music.baidu.com/song/" + Cf_String.LastStringRemove(Cf_String.ExtractString(str, "sid", ",")[0].Remove(0, "sid&quot;:".Length), 0, 1) + "/download?__o=%2Fsearch"));
                    //获取歌曲名
                    TotalSongList[1].Add(Cf_String.LastStringRemove(Cf_String.ExtractString(Cf_String.ExtractString(str, "data-songdata=", "\">")[0], "title=\"", "\"")[0].Remove(0, "title=\"".Length), 0, 1));
                    //获取歌曲作者名
                    TotalSongList[2].Add(Cf_String.LastStringRemove(Cf_String.ExtractString(str, "<span class=\"author_list\" title=\"", "\"")[0].Remove(0, "<span class=\"author_list\" title=\"".Length), 0, 1));
                }
            }
            if (TotalSongList[0].Count == 0) { boolsongcunzai = true; }
            return TotalSongList;
        }
        string SongDownload(string songhttp)//传入一个歌曲的下载地址，完成cookies的配置返回歌曲下载地址
        {
            Cf_HttpWeb BaiDu_Web = new Cf_HttpWeb();
            List<string> cookie_6 = BaiDu_Web.PostOrGet(songhttp, HttpMethod.GET, baiducookie);
            return Cf_String.LastStringRemove(Cf_String.ExtractString(cookie_6[1], "http://zhangmenshiting.baidu.com", "&")[0], 0, 1);
        }

    }
}
