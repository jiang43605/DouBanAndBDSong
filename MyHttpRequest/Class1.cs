using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Collections;

namespace MyHttp
{
    public class MyHttPrequest
    {
     /// <summary>
     /// 用于进行测试的
     /// </summary>
        public string cookieheader { set; get; }
        public CookieContainer MyHttpPostRequest(string uri, string referer, string postcanshu)
        {
            //string cookie;
            CookieContainer myCookieCintainer = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.KeepAlive = true;
            request.Timeout = 10000;
            request.Referer = referer;
            request.ContentLength = postcanshu.Length;
            request.Headers.Set("Origin", "http://www.douban.com");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36";
            request.CookieContainer = myCookieCintainer;
            Encoding myenconding = Encoding.GetEncoding("gb2312");
            byte[] mybyte = myenconding.GetBytes(postcanshu);
            request.GetRequestStream().Write(mybyte, 0, mybyte.Length);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                //cookie = response.Headers.Get("set-cookie");
                response.Cookies = myCookieCintainer.GetCookies(request.RequestUri);
                cookieheader = myCookieCintainer.GetCookieHeader(request.RequestUri);
                
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), myenconding))
                {
                    string sstt = sr.ReadToEnd();
                }
                return myCookieCintainer;
            }


        }
        public CookieContainer MyHttpPostRequest(CookieContainer myCookieCintainer, string uri, string referer, string postcanshu)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.KeepAlive = true;
            request.Timeout = 10000;
            request.Referer = referer;
            request.ContentLength = postcanshu.Length;
            request.Headers.Set("Origin", "http://www.douban.com");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36";
            request.CookieContainer = myCookieCintainer;
            Encoding myenconding = Encoding.GetEncoding("gb2312");
            byte[] mybyte = myenconding.GetBytes(postcanshu);
            request.GetRequestStream().Write(mybyte, 0, mybyte.Length);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                //cookie = response.Headers.Get("set-cookie");
                response.Cookies = myCookieCintainer.GetCookies(request.RequestUri);
                cookieheader = myCookieCintainer.GetCookieHeader(request.RequestUri);

                using (StreamReader sr = new StreamReader(response.GetResponseStream(), myenconding))
                {
                    string sstt = sr.ReadToEnd();
                }
                return myCookieCintainer;
            }


        }
        public static string staticMyHttpGetRequest(string cookheader, string encoding, string uri, string referer, CookieContainer mycookiecontaniner)
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.CookieContainer = mycookiecontaniner;
            request.Method = "GET";
            request.KeepAlive = true;
            request.Timeout = 10000;
            request.Referer = referer;
            request.Headers.Set(HttpRequestHeader.Cookie, cookheader);
            request.ContentType = "application/x-www-form-urlencoded";
            Encoding myenconding = Encoding.GetEncoding(encoding);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                response.Cookies = mycookiecontaniner.GetCookies(request.RequestUri);
                
                using (StreamReader sw = new StreamReader(response.GetResponseStream(), myenconding))
                {                     
                    string htmltext = sw.ReadToEnd();
                    return htmltext;
                }
            }
        }
        public static ArrayList YanZhengMa(string cookheader, string encoding, string uri, string referer)//, CookieContainer mycookiecontaniner
        {
            ArrayList list = new ArrayList();
            string yzmUrl;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            CookieContainer mycookiecontaniner = new CookieContainer();
            request.CookieContainer = mycookiecontaniner;
            request.Method = "GET";
            request.KeepAlive = true;
            request.Timeout = 10000;
            request.Referer = referer;
            request.Headers.Set(HttpRequestHeader.Cookie, cookheader);
            request.ContentType = "application/x-www-form-urlencoded";
            Encoding myenconding = Encoding.GetEncoding(encoding);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                response.Cookies = mycookiecontaniner.GetCookies(request.RequestUri);
                list.Add(mycookiecontaniner);
                using (StreamReader sw = new StreamReader(response.GetResponseStream(), myenconding))
                {
                    try
                    {
                        string htmltext = sw.ReadToEnd();
                        yzmUrl = htmltext;
                        yzmUrl = yzmUrl.Remove(yzmUrl.IndexOf("\" alt=\"captcha\""));
                        yzmUrl = yzmUrl.Remove(0, yzmUrl.Remove(yzmUrl.IndexOf("<img id=\"captcha_image\" src=\"")).Length).Remove(0, "<img id=\"captcha_image\" src=\"".Length);
                        string[] id = yzmUrl.Split(new char[] { '=', '&' });
                        list.Add(yzmUrl);
                        list.Add(id[1]);
                    }
                    catch { list.Add("0"); }
                }
            }
          
            return list;
        }

    }
}
