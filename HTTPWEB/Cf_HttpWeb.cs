using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Chengf
{
    /// <summary>
    /// 设置要请求的方法
    /// </summary>
    public enum HttpMethod { POST, GET };
    /// <summary>
    /// 用于获取Http网页内容的类
    /// </summary>
    public class Cf_HttpWeb
    {
        #region//属性值或字段的声明
        bool keepalive = true;//设置默认值
        int timeout = 10000;//设置默认值
        string rotocolversion = "1.1";
        string encodingset = "gb2312";
        string useragent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1897.2 Safari/537.36";
        /// <summary>
        /// 设置Accpet标头,默认为null
        /// </summary>
        public string Accpet { set; get; }
        /// <summary>
        /// 设置Connection标头,默认为null
        /// </summary>
        public string Connection { set; get; }
        /// <summary>
        /// 设置ContentType标头,默认为null
        /// </summary>
        public string ContentType { set; get; }
        /// <summary>
        /// 设置Expect标头,默认为null
        /// </summary>
        public string Expect { set; get; }
        /// <summary>
        /// 设置KeepAlive标头,默认为true
        /// </summary>
        public bool KeepAlive { set { keepalive = value; } get { return keepalive; } }
        /// <summary>
        /// 设置MediaType标头,默认为null
        /// </summary>
        public string MediaType { set; get; }
        /// <summary>
        /// 设置Referer标头,默认为null
        /// </summary>
        public string Referer { set; get; }
        /// <summary>
        /// 设置Timeout标头,默认为10000毫秒
        /// </summary>
        public int Timeout { set { timeout = value; } get { return timeout; } }
        /// <summary>
        /// 设置TransferEncoding标头,默认为null
        /// </summary>
        public string TransferEncoding { set; get; }
        /// <summary>
        /// 设置UserAgent标头,默认为null
        /// </summary>
        public string UserAgent { set { useragent = value; } get { return useragent; } }
        /// <summary>
        /// 设置ProtocolVersion标头,默认为1.1
        /// </summary>
        public string ProtocolVersion { set { rotocolversion = value; } get { return rotocolversion; } }
        /// <summary>
        /// 设置UserDate标头,即当方法为POST时，需要传递的用户数据，默认为null
        /// </summary>
        public string UserDate { set; get; }
        /// <summary>
        /// 设置EncodingSet标头,默认为gb2312
        /// </summary>
        public string EncodingSet { set { encodingset = value; } get { return encodingset; } }
        /// <summary>
        /// 通过直接的字符串对指定的标头赋值
        /// </summary>
        public string HeaderSet { set; get; }
        #endregion
        #region//PostOrGet方法传递的Cookie为Container容器
        /// <summary>
        /// 用指定的方法从指定的地址获取该地址返回的Cookie以及网页Responed
        /// </summary>
        /// <param name="method">为"POST"或者"GET"</param>
        /// <param name="uri">指定的Url地址</param>
        /// <param name="myCookieContainer">传入已经存在的Cookie,如果无则:new CookieContainer()</param>
        /// <returns>返回一个ArrayList对象，索引为0的值为Cookie值，索引为1的值为指定地址网页的内容</returns>
        public ArrayList PostOrGet(string uri, HttpMethod method, CookieContainer myCookieContainer)
        {
            ArrayList list = new ArrayList();
            HttpWebRequest myHttpWebRequest = HttpWebRequest.Create(uri) as HttpWebRequest;
            myHttpWebRequest.CookieContainer = myCookieContainer;
            if (method == HttpMethod.POST) { myHttpWebRequest.Method = "POST"; }
            else { myHttpWebRequest.Method = "GET"; }//默认的为GET
            myHttpWebRequest.Accept = Accpet;
            myHttpWebRequest.Connection = Connection;
            if (UserDate != null) { myHttpWebRequest.ContentLength = UserDate.Length; }//默认-1
            myHttpWebRequest.ContentType = ContentType;
            myHttpWebRequest.Expect = Expect;
            myHttpWebRequest.KeepAlive = keepalive;//默认true
            myHttpWebRequest.MediaType = MediaType;
            myHttpWebRequest.ProtocolVersion = Version.Parse(rotocolversion);//默认1.1
            myHttpWebRequest.Referer = Referer;
            myHttpWebRequest.Timeout = timeout;//默认100000
            myHttpWebRequest.TransferEncoding = TransferEncoding;
            myHttpWebRequest.UserAgent = useragent;
            if (HeaderSet != null)
            {
                string[] strHeaderSet = HeaderSet.Split(new char[] { ',' });
                myHttpWebRequest.Headers.Set(strHeaderSet[0], strHeaderSet[1]);
            }

            if (method == HttpMethod.POST)//如果是POST那么此处的UserDate必须要被附有效值
            {
                byte[] mybyte = Encoding.GetEncoding(encodingset).GetBytes(UserDate);
                myHttpWebRequest.GetRequestStream().Write(mybyte, 0, mybyte.Length);
            }

            using (HttpWebResponse myhttpwebresponse = myHttpWebRequest.GetResponse() as HttpWebResponse)
            {
                myhttpwebresponse.Cookies = myCookieContainer.GetCookies(myHttpWebRequest.RequestUri);
                list.Add(myCookieContainer);
                using (StreamReader sr = new StreamReader(myhttpwebresponse.GetResponseStream(), Encoding.GetEncoding(encodingset)))
                {
                    string html = sr.ReadToEnd();
                    list.Add(html);
                }
            }
            Clear();
            return list;
        }
        /// <summary>
        /// 从指定Url处返回图形格式流
        /// </summary>
        /// <param name="uri">指定的图形所在的地址</param>
        /// <param name="method">指定获取的方式，一般为GET</param>
        /// <param name="myCookieContainer">传入已经存在的Cookie,如果无则:new CookieContainer()</param>
        /// <returns>返回一个ArryList动态数组，返回的Cookie存在索引位置0，图片存在索引位置1</returns>
        public ArrayList PostOrGetCheck(string uri, HttpMethod method, CookieContainer myCookieContainer)
        {

            ArrayList list = new ArrayList();
            HttpWebRequest myHttpWebRequest = HttpWebRequest.Create(uri) as HttpWebRequest;
            myHttpWebRequest.CookieContainer = myCookieContainer;
            if (method == HttpMethod.POST) { myHttpWebRequest.Method = "POST"; }
            else { myHttpWebRequest.Method = "GET"; }//默认的为GET
            myHttpWebRequest.Accept = Accpet;
            myHttpWebRequest.Connection = Connection;
            if (UserDate != null) { myHttpWebRequest.ContentLength = UserDate.Length; }//默认-1
            myHttpWebRequest.ContentType = ContentType;
            myHttpWebRequest.Expect = Expect;
            myHttpWebRequest.KeepAlive = keepalive;//默认true
            myHttpWebRequest.MediaType = MediaType;
            myHttpWebRequest.ProtocolVersion = Version.Parse(rotocolversion);//默认1.1
            myHttpWebRequest.Referer = Referer;
            myHttpWebRequest.Timeout = timeout;//默认100000
            myHttpWebRequest.TransferEncoding = TransferEncoding;
            myHttpWebRequest.UserAgent = useragent;
            if (method == HttpMethod.POST)//如果是POST那么此处的UserDate必须要被附有效值
            {
                byte[] mybyte = Encoding.GetEncoding(encodingset).GetBytes(UserDate);
                myHttpWebRequest.GetRequestStream().Write(mybyte, 0, mybyte.Length);
            }
            using (HttpWebResponse myhttpwebresponse = myHttpWebRequest.GetResponse() as HttpWebResponse)
            {
                myhttpwebresponse.Cookies = myCookieContainer.GetCookies(myHttpWebRequest.RequestUri);
                list.Add(myCookieContainer);
                using (Stream sr = myhttpwebresponse.GetResponseStream())
                {
                    list.Add(sr);
                }
            }
            Clear();
            return list;
        }
        #endregion
        #region//PostOrGet方法传递的Cookie为String类型
        /// <summary>
        /// 用指定的方法从指定的地址获取该地址返回的Cookie以及网页Responed
        /// </summary>
        /// <param name="method">为"POST"或者"GET"</param>
        /// <param name="uri">指定的Url地址</param>
        /// <param name="cookie">以字串的形式传入Cookie值</param>
        /// <returns>返回一个string集合对象，索引为0的值为Cookie值，索引为1的值为指定地址网页的内容</returns>
        public List<string> PostOrGet(string uri, HttpMethod method, string cookie)
        {
            try
            {
                List<string> list = new List<string>();
                HttpWebRequest myHttpWebRequest = HttpWebRequest.Create(uri) as HttpWebRequest;
                if (method == HttpMethod.POST) { myHttpWebRequest.Method = "POST"; }
                else { myHttpWebRequest.Method = "GET"; }//默认的为GET
                myHttpWebRequest.Accept = Accpet;
                myHttpWebRequest.Connection = Connection;
                if (UserDate != null) { myHttpWebRequest.ContentLength = UserDate.Length; }//默认-1
                myHttpWebRequest.ContentType = ContentType;
                myHttpWebRequest.Expect = Expect;
                myHttpWebRequest.KeepAlive = keepalive;//默认true
                myHttpWebRequest.MediaType = MediaType;
                myHttpWebRequest.ProtocolVersion = Version.Parse(rotocolversion);//默认1.1
                myHttpWebRequest.Referer = Referer;
                myHttpWebRequest.Timeout = timeout;//默认100000
                myHttpWebRequest.TransferEncoding = TransferEncoding;
                myHttpWebRequest.UserAgent = useragent;
                myHttpWebRequest.Headers.Set(HttpRequestHeader.Cookie, cookie);
                if (HeaderSet != null)
                {
                    string[] strHeaderSet = HeaderSet.Split(new char[] { ',' });
                    myHttpWebRequest.Headers.Set(strHeaderSet[0], strHeaderSet[1]);
                }

                if (method == HttpMethod.POST)//如果是POST那么此处的UserDate必须要被附有效值
                {
                    byte[] mybyte = Encoding.GetEncoding(encodingset).GetBytes(UserDate);
                    myHttpWebRequest.GetRequestStream().Write(mybyte, 0, mybyte.Length);
                }
                using (HttpWebResponse myhttpwebresponse = myHttpWebRequest.GetResponse() as HttpWebResponse)
                {
                    list.Add(myhttpwebresponse.Headers.Get("set-cookie"));
                    using (StreamReader sr = new StreamReader(myhttpwebresponse.GetResponseStream(), Encoding.GetEncoding(encodingset)))
                    {
                        string html = sr.ReadToEnd();
                        list.Add(html);
                    }
                }
                Clear();
                return list;
            }
            catch
            {
                List<string> list1 = new List<string>();
                list1.Add("##$");
                return list1;
            }
        }
        /// <summary>
        /// 用指定的方法从指定的地址获取该地址返回的Cookie以及网页Responed
        /// </summary>
        /// <param name="method">为"POST"或者"GET"</param>
        /// <param name="uri">指定的Url地址</param>
        /// <returns>返回一个string集合对象，索引为0的值为Cookie值，索引为1的值为指定地址网页的内容</returns>
        public List<string> PostOrGet(string uri, HttpMethod method)
        {
            List<string> list = new List<string>();
            HttpWebRequest myHttpWebRequest = HttpWebRequest.Create(uri) as HttpWebRequest;
            if (method == HttpMethod.POST) { myHttpWebRequest.Method = "POST"; }
            else { myHttpWebRequest.Method = "GET"; }//默认的为GET
            myHttpWebRequest.Accept = Accpet;
            myHttpWebRequest.Connection = Connection;
            if (UserDate != null) { myHttpWebRequest.ContentLength = UserDate.Length; }//默认-1
            myHttpWebRequest.ContentType = ContentType;
            myHttpWebRequest.Expect = Expect;
            myHttpWebRequest.KeepAlive = keepalive;//默认true
            myHttpWebRequest.MediaType = MediaType;
            myHttpWebRequest.ProtocolVersion = Version.Parse(rotocolversion);//默认1.1
            myHttpWebRequest.Referer = Referer;
            myHttpWebRequest.Timeout = timeout;//默认100000
            myHttpWebRequest.TransferEncoding = TransferEncoding;
            myHttpWebRequest.UserAgent = useragent;
            if (HeaderSet != null)
            {
                string[] strHeaderSet = HeaderSet.Split(new char[] { ',' });
                myHttpWebRequest.Headers.Set(strHeaderSet[0], strHeaderSet[1]);
            }

            if (method == HttpMethod.POST)//如果是POST那么此处的UserDate必须要被附有效值
            {
                byte[] mybyte = Encoding.GetEncoding(encodingset).GetBytes(UserDate);
                myHttpWebRequest.GetRequestStream().Write(mybyte, 0, mybyte.Length);
            }
            try
            {
                using (HttpWebResponse myhttpwebresponse = myHttpWebRequest.GetResponse() as HttpWebResponse)
                {
                    list.Add(myhttpwebresponse.Headers.Get("set-cookie"));
                    using (StreamReader sr = new StreamReader(myhttpwebresponse.GetResponseStream(), Encoding.GetEncoding(encodingset)))
                    {
                        string html = sr.ReadToEnd();
                        list.Add(html);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            Clear();
            return list;
        }
        /// <summary>
        /// 从指定Url处返回图形格式流
        /// </summary>
        /// <param name="uri">指定的图形所在的地址</param>
        /// <param name="method">指定获取的方式，一般为GET</param>
        /// <param name="cookie">传入已经存在的Cookie，要求的类型为string</param>
        /// <returns>返回一个string集合对象，返回的Cookie存在索引位置0，图片存在索引位置1</returns>
        public ArrayList PostOrGetCheck(string uri, HttpMethod method, string cookie)
        {

            ArrayList list = new ArrayList();
            HttpWebRequest myHttpWebRequest = HttpWebRequest.Create(uri) as HttpWebRequest;
            if (method == HttpMethod.POST) { myHttpWebRequest.Method = "POST"; }
            else { myHttpWebRequest.Method = "GET"; }//默认的为GET
            myHttpWebRequest.Accept = Accpet;
            myHttpWebRequest.Connection = Connection;
            if (UserDate != null) { myHttpWebRequest.ContentLength = UserDate.Length; }//默认-1
            myHttpWebRequest.ContentType = ContentType;
            myHttpWebRequest.Expect = Expect;
            myHttpWebRequest.KeepAlive = keepalive;//默认true
            myHttpWebRequest.MediaType = MediaType;
            myHttpWebRequest.ProtocolVersion = Version.Parse(rotocolversion);//默认1.1
            myHttpWebRequest.Referer = Referer;
            myHttpWebRequest.Timeout = timeout;//默认100000
            myHttpWebRequest.TransferEncoding = TransferEncoding;
            myHttpWebRequest.UserAgent = useragent;
            myHttpWebRequest.Headers.Set(HttpRequestHeader.Cookie, cookie);
            if (method == HttpMethod.POST)//如果是POST那么此处的UserDate必须要被附有效值
            {
                byte[] mybyte = Encoding.GetEncoding(encodingset).GetBytes(UserDate);
                myHttpWebRequest.GetRequestStream().Write(mybyte, 0, mybyte.Length);
            }
            using (HttpWebResponse myhttpwebresponse = myHttpWebRequest.GetResponse() as HttpWebResponse)
            {
                list.Add(myhttpwebresponse.Headers.Get("set-cookie"));
                using (Stream sr = myhttpwebresponse.GetResponseStream())
                {
                    list.Add(sr);
                }
            }
            Clear();
            return list;
        }
        /// <summary>
        /// 从指定Url处返回图形格式流
        /// </summary>
        /// <param name="uri">指定的图形所在的地址</param>
        /// <param name="method">指定获取的方式，一般为GET</param>
        /// <returns>返回一个string集合对象，返回的Cookie存在索引位置0，图片存在索引位置1</returns>
        public ArrayList PostOrGetCheck(string uri, HttpMethod method)
        {

            ArrayList list = new ArrayList();
            HttpWebRequest myHttpWebRequest = HttpWebRequest.Create(uri) as HttpWebRequest;
            if (method == HttpMethod.POST) { myHttpWebRequest.Method = "POST"; }
            else { myHttpWebRequest.Method = "GET"; }//默认的为GET
            myHttpWebRequest.Accept = Accpet;
            myHttpWebRequest.Connection = Connection;
            if (UserDate != null) { myHttpWebRequest.ContentLength = UserDate.Length; }//默认-1
            myHttpWebRequest.ContentType = ContentType;
            myHttpWebRequest.Expect = Expect;
            myHttpWebRequest.KeepAlive = keepalive;//默认true
            myHttpWebRequest.MediaType = MediaType;
            myHttpWebRequest.ProtocolVersion = Version.Parse(rotocolversion);//默认1.1
            myHttpWebRequest.Referer = Referer;
            myHttpWebRequest.Timeout = timeout;//默认100000
            myHttpWebRequest.TransferEncoding = TransferEncoding;
            myHttpWebRequest.UserAgent = useragent;
            if (method == HttpMethod.POST)//如果是POST那么此处的UserDate必须要被附有效值
            {
                byte[] mybyte = Encoding.GetEncoding(encodingset).GetBytes(UserDate);
                myHttpWebRequest.GetRequestStream().Write(mybyte, 0, mybyte.Length);
            }
            using (HttpWebResponse myhttpwebresponse = myHttpWebRequest.GetResponse() as HttpWebResponse)
            {
                list.Add(myhttpwebresponse.Headers.Get("set-cookie"));
                using (Stream sr = myhttpwebresponse.GetResponseStream())
                {
                    list.Add(sr);
                }
            }
            Clear();
            return list;
        }
        #endregion
        /// <summary>
        /// 将所有的设置清除
        /// </summary>
        public void Clear()//对所有声明对象回复初值
        {
            keepalive = true;//设置默认值
            timeout = 10000;//设置默认值
            rotocolversion = "1.1";
            encodingset = "gb2312";
            useragent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1897.2 Safari/537.36";
            Accpet = null;
            Connection = null;
            ContentType = null;
            Expect = null;
            //KeepAlive = true;
            MediaType = null;
            Referer = null;
            //Timeout = 10000;
            TransferEncoding = null;
            //UserAgent = null;
            //ProtocolVersion = "1.1";
            UserDate = null;
            //EncodingSet = "gb2312";
        }
    }


}
