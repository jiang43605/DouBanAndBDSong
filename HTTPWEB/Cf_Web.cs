using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Chengf
{
    /// <summary>
    /// 网络的一些相关类
    /// </summary>
    public class Cf_Web
    {
        #region//检测网络是否联网
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(int Description, int ReservedValue);
        /// <summary>
        /// 检测网络是否联网，联网则返回true,否则返回false
        /// </summary>
        /// <returns></returns>
        public static bool IsConnectedToInternet()
        {
            int Desc = 0;
            return InternetGetConnectedState(Desc, 0);
        }
        #endregion
        #region//返回一个距离现在的毫秒级数字（string）
        /// <summary>
        /// 返回一个毫秒的数字
        /// </summary>
        /// <returns></returns>
        public static string currenttime()
        {
            return DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).TotalMilliseconds.ToString("0");
        }
        #endregion
    }

}
