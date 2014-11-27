using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chengf
{
    /// <summary>
    /// 处理字符串的类
    /// </summary>
    public class Cf_String//处理字符串的类
    {
        /// <summary>
        /// 将stringname中符合开头为startString结尾为endString的都提取出来
        /// </summary>
        /// <param name="stringname">要被提取的的字符串</param>
        /// <param name="startString">开头匹配的字符串</param>
        /// <param name="endString">结尾匹配的的字符串</param>
        /// <returns>以字符串数组返回提取的所有符合要求的字符串</returns>
        public static List<string> ExtractString(string stringname, string startString, string endString)
        {
            List<string> stringlist = new List<string>();
            while (stringname.IndexOf(startString) != -1)
            {
                stringlist.Add(stringname.Substring(stringname.IndexOf(startString), stringname.IndexOf(endString, stringname.IndexOf(startString)+startString.Length) + endString.Length - stringname.IndexOf(startString)));
                stringname = stringname.Remove(0, stringname.IndexOf(endString, stringname.IndexOf(startString)) + endString.Length);
            }
            return stringlist;
        }
        /// <summary>
        /// 将stringname中符合开头为startString结尾为endString的都提取出来，并通过确定bool值来决定是否将得到的字符串顺序反转
        /// </summary>
        /// <param name="stringname">要被提取的的字符串</param>
        /// <param name="startString">开头匹配的字符串</param>
        /// <param name="endString">结尾匹配的的字符串</param>
        /// <param name="ReverseBool">决定是否将得到的字符串顺序反转，true为反转，默认为false</param>
        /// <returns>以字符串数组返回提取的所有符合要求的字符串</returns>
        public static List<string> ExtractString(string stringname, string startString, string endString, bool ReverseBool)
        {
            List<string> stringlist = new List<string>();
            while (stringname.IndexOf(startString) != -1)
            {
                if (ReverseBool) { stringlist.Add(Reverse(stringname.Remove(0, stringname.IndexOf(startString)).Substring(0, stringname.Remove(0, stringname.IndexOf(startString)).IndexOf(endString) + endString.Length))); }
                else { stringlist.Add(stringname.Remove(0, stringname.IndexOf(startString)).Substring(0, stringname.Remove(0, stringname.IndexOf(startString)).IndexOf(endString) + endString.Length)); }
                stringname = stringname.Remove(0, stringname.Remove(0, stringname.IndexOf(startString)).IndexOf(endString) + endString.Length + stringname.IndexOf(startString));
            }
            return stringlist;
        }
        /// <summary>
        /// 将字符串反转
        /// </summary>
        /// <param name="stringname">要反转的字符串</param>
        /// <returns></returns>
        public static string Reverse(string stringname)
        {
            string stringnameReverse = "";
            for (int i = stringname.Length - 1; i >= 0; i--)
            {
                stringnameReverse += stringname[i];
            }
            return stringnameReverse;
        }
        /// <summary>
        /// 从末尾将stringname中符合开头为startString结尾为endString的都提取出来
        /// </summary>
        /// <param name="stringname">要被提取的的字符串</param>
        /// <param name="startString">开头匹配的字符串</param>
        /// <param name="endString">结尾匹配的的字符串</param>
        /// <returns>以字符串数组返回提取的所有符合要求的字符串</returns>
        public static List<string> LastExtractString(string stringname, string startString, string endString)
        {
            stringname = Reverse(stringname);
            startString = Reverse(startString);
            endString = Reverse(endString);
            List<string> testList = ExtractString(stringname, startString, endString, true);
            testList.Reverse();
            return testList;
        }
        /// <summary>
        /// Remove方法用于List,从指定位置删除指定的字符
        /// </summary>
        /// <param name="stringlist">传入的String List集合</param>
        /// <param name="index">开始的位置</param>
        /// <param name="count">移除的指定字符数</param>
        /// <returns>返回删除后的字符串</returns>
        public static List<string> ListRemove(List<string> stringlist, int index, int count)
        {
            List<string> testList = new List<string>();
            for (int i = 0; i < stringlist.Count; i++)
            {
                testList.Add(stringlist[i].Remove(index, count));
            }
            return testList;
        }//用于移除字符串数组中的特定字符，同Remove中的重载2
        /// <summary>
        /// Remove方法用于List，删除从指定位置到最后位置的所以字符
        /// </summary>
        /// <param name="stringlist">传入的String List集合</param>
        /// <param name="index">开始的位置</param>
        /// <returns>返回删除后的字符串</returns>
        public static List<string> ListRemove(List<string> stringlist, int index)
        {
            List<string> testList = new List<string>();
            for (int i = 0; i < stringlist.Count; i++)
            {
                testList.Add(stringlist[i].Remove(index));
            }
            return testList;
        }////用于移除字符串数组中的特定字符，同Remove中的重载1
        /// <summary>
        /// Remove方法配合字符串检索用于List,从指定位置提取指定的前后字符相匹配的字符串
        /// </summary>
        /// <param name="stringlist">传入的String List集合</param>
        /// <param name="start">开始相匹配的字符串</param>
        /// <param name="end">结束相匹配的字符串</param>
        /// <returns>返回删除后的字符串</returns>
        public static List<string> ListExtractString(List<string> stringlist, string start, string end)
        {
            List<string> retrunlist = new List<string>();
            foreach(string str in stringlist)
            {
                retrunlist.Add(ExtractString(str, start, end)[0]);
            }
            return retrunlist;
        }
        /// <summary>
        /// Remove方法用于List,从指定位置删除指定的字符，从后面开始检索
        /// </summary>
        /// <param name="stringlist">传入的String List集合</param>
        /// <param name="index">开始的位置</param>
        /// <param name="count">移除的指定字符数</param>
        /// <returns>返回删除后的字符串</returns>
        public static List<string> LastListRemove(List<string> stringlist, int index, int count)
        {
            List<string> testList = new List<string>();
            for (int i = 0; i < stringlist.Count; i++)
            {
                testList.Add(Reverse(Reverse(stringlist[i]).Remove(index, count)));
            }
            return testList;
        }
        /// <summary>
        /// Remove方法用于List,从指定位置删除指定的字符，从后面开始检索
        /// </summary>
        /// <param name="stringlist">传入的String List集合</param>
        /// <param name="index">开始的位置</param>
        /// <returns>返回删除后的字符串</returns>
        public static List<string> LastListRemove(List<string> stringlist, int index)
        {
            List<string> testList = new List<string>();
            for (int i = 0; i < stringlist.Count; i++)
            {
                testList.Add(Reverse(Reverse(stringlist[i]).Remove(index)));
            }
            return testList;
        }
        /// <summary>
        /// Remove方法用于string,从指定位置删除指定的字符，从后面开始检索
        /// </summary>
        /// <param name="stringlist">传入的String</param>
        /// <param name="index">开始的位置</param>
        /// <param name="count">移除的指定字符数</param>
        /// <returns>返回删除后的字符串</returns>
        public static string LastStringRemove(string stringlist, int index, int count)
        {
            string testList;
            testList = Reverse(Reverse(stringlist).Remove(index, count));
            return testList;
        }
        /// <summary>
        /// Remove方法用于string,从指定位置删除指定的字符，从后面开始检索
        /// </summary>
        /// <param name="stringlist">传入的String</param>
        /// <param name="index">开始的位置</param>
        /// <param name="count">移除的指定字符数</param>
        /// <returns>返回删除后的字符串</returns>
        public static string LastStringRemove(string stringlist, int index)
        {
            string testList;
            testList = Reverse(Reverse(stringlist).Remove(index));
            return testList;
        }
    }
}
