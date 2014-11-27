
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Web;

namespace Chengf
{
    /// <summary>
    /// Chengf类下des加密算法
    /// </summary>
    public static class Cf_PassWordClass
    {
        #region     des加密算法
        ///   <summary>   
        ///   加密字符串   
        ///   </summary>   
        ///   <param   name="pToEncrypt">待加密字符串</param>   
        ///   <returns></returns>   
        public static string Encrypt(string pToEncrypt)
        {

            byte[] desKey = new byte[] { 0x16, 0x09, 0x14, 0x15, 0x07, 0x01, 0x05, 0x08 };
            byte[] desIV = new byte[] { 0x16, 0x09, 0x14, 0x15, 0x07, 0x01, 0x05, 0x08 };

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            try
            {
                //把字符串放到byte数组中   
                //原来使用的UTF8编码，我改成Unicode编码了，不行   
                byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
                //byte[]   inputByteArray=Encoding.Unicode.GetBytes(pToEncrypt);   

                //建立加密对象的密钥和偏移量   
                //原文使用ASCIIEncoding.ASCII方法的GetBytes方法   
                //使得输入密码必须输入英文文本   
                des.Key = desKey; //   ASCIIEncoding.ASCII.GetBytes(sKey);   
                des.IV = desIV; //ASCIIEncoding.ASCII.GetBytes(sKey);   
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(),
                CryptoStreamMode.Write);
                //Write   the   byte   array   into   the   crypto   stream   
                //(It   will   end   up   in   the   memory   stream)   
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();

                //Get   the   data   back   from   the   memory   stream,   and   into   a   string   
                StringBuilder ret = new StringBuilder();
                foreach (byte b in ms.ToArray())
                {
                    //Format   as   hex   
                    ret.AppendFormat("{0:X2}", b);
                }
                ret.ToString();
                return ret.ToString();
            }
            catch
            {
                return pToEncrypt;
            }
            finally
            {
                des = null;
            }
        }

        ///   <summary>   
        ///   解密字符串   
        ///   </summary>   
        ///   <param   name="pToDecrypt">待解密字符串</param>   
        ///   <returns></returns>   
        public static string Decrypt(string pToDecrypt)
        {
            byte[] desKey = new byte[] { 0x16, 0x09, 0x14, 0x15, 0x07, 0x01, 0x05, 0x08 };
            byte[] desIV = new byte[] { 0x16, 0x09, 0x14, 0x15, 0x07, 0x01, 0x05, 0x08 };

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            try
            {
                //Put   the   input   string   into   the   byte   array   
                byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
                for (int x = 0; x < pToDecrypt.Length / 2; x++)
                {
                    int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                    inputByteArray[x] = (byte)i;
                }

                //建立加密对象的密钥和偏移量，此值重要，不能修改   
                des.Key = desKey; //ASCIIEncoding.ASCII.GetBytes(sKey);   
                des.IV = desIV; //ASCIIEncoding.ASCII.GetBytes(sKey);   
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                //Flush   the   data   through   the   crypto   stream   into   the   memory   stream   
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();

                //Get   the   decrypted   data   back   from   the   memory   stream   
                //建立StringBuild对象，CreateDecrypt使用的是流对象，必须把解密后的文本变成流对象   
                StringBuilder ret = new StringBuilder();

                return System.Text.Encoding.Default.GetString(ms.ToArray());
            }
            catch
            {
                return pToDecrypt;
            }
            finally
            {
                des = null;
            }
        }


        #endregion
        #region//MD5加密
        ///   <summary>
        ///   给一个字符串进行MD5加密
        ///   </summary>
        ///   <param   name="strText">待加密字符串</param>
        ///   <returns>加密后的字符串</returns>
        public static string MD5Encrypt(string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(strText));
            return System.Text.Encoding.Default.GetString(result);
        }
        #endregion
        #region//URLEncoed加密
        /// <summary>
        /// 用于对字符串进行URLEncoed加密
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }
        #endregion
        #region//UTF-8转汉字
        /// <summary>
        /// UTF-8转汉字函数
        /// </summary>
        /// <param name="m_strContent">要转换的字符串</param>
        /// <returns>返回转换好的字符串</returns>
        public static string UTF8ToString(string m_strContent)
        {
            string reString = null;
            char[] content = m_strContent.ToCharArray(); //把字符串变为字符数组，以进行处理
            for (int i = 0; i < content.Length; i++) //遍历所有字符
            {
                if (content[i] == '\\' && i != content.Length - 1) //判断是否转义字符 \ 
                {
                    switch (content[i + 1]) //判断转义字符的下一个字符是什么
                    {
                        case 'u': //转换的是汉字
                        case 'U':
                            reString += HexArrayToChar(content, i + 2); //获取对应的汉字
                            i = i + 5;
                            break;
                        case '/': //转换的是 /
                        case '\\': //转换的是 \
                        case '"':
                            break;
                        default: //其它
                            reString += EscapeCharacter(content[i + 1]); //转为其它类型字符
                            i = i + 1;
                            break;
                    }
                }
                else
                    reString += content[i]; //非转义字符则直接加入
            }
            return reString;
        }
        /// <summary>
        /// 字符数组转对应汉字字符
        /// </summary>
        /// <param name="content">要转换的数字</param>
        /// <param name="startIndex">起始位置</param>
        /// <returns>对应的汉字</returns>
        private static char HexArrayToChar(char[] content, int startIndex)
        {
            char[] ac = new char[4];
            for (int i = 0; i < 4; i++) //获取要转换的部分
                ac[i] = content[startIndex + i];
            string num = new string(ac); //字符数组转为字符串
            return HexStringToChar(num);
        }
        /// <summary>
        /// 字符串转对应汉字字符
        /// 只能处理如"8d34"之类的数字字符为对应的汉字
        /// 例子："9648" 转为 '陈'
        /// </summary>
        /// <param name="content">转换的字符串</param>
        /// <returns>对应的汉字</returns>
        static char HexStringToChar(string content)
        {
            int num = Convert.ToInt32(content, 16);
            return (char)num;
        }
        /// <summary>
        /// 转义字符转换函数
        /// 转换字符为对应的转义字符
        /// </summary>
        /// <param name="c">要转的字符</param>
        /// <returns>对应的转义字符</returns>
        private static char EscapeCharacter(char c)
        {
            switch (c)
            {
                case 't':
                    c = '\t';
                    break;
                case 'n':
                    c = '\n';
                    break;
                case 'r':
                    c = '\r';
                    break;
                case '\'':
                    c = '\'';
                    break;
                case '0':
                    c = '\0';
                    break;
            }
            return c;
        }
        #endregion
    }
}

