using System;
using UnityEngine;

namespace Uqee.Utility
{
    public static class TimeUtils
    {
        private static long _serverTimeStamp = 0;
        private static float _gotRecord = 0f;

        private static DateTime _zeroDt;

        /// <summary>
        /// 系统时间；
        /// </summary>
        /// <param name="timestamp"></param>
        public static DateTime serverDateTime
        {
            get
            {
                return TimestampToDateTime(serverTimestamp);
            }
        }

        public static int GetLeftTime(long endTimeStamp)
        {
            return (int)(endTimeStamp - serverTimestamp);
        }
        public static long serverTimestamp
        {
            get
            {
                return (long)(Time.realtimeSinceStartup - _gotRecord) + _serverTimeStamp;
            }
            set
            {
                _serverTimeStamp = value;
                _gotRecord = Time.realtimeSinceStartup;
                _zeroDt = new DateTime(serverDateTime.Year, serverDateTime.Month, serverDateTime.Day, 0,0,0);
            }
        }

        /// <summary>
        /// 根据传入的年月日获取对应的服务器时间戳 2020.3.20
        /// </summary>
        /// <param name="serverTime"></param>
        public static long GetServerTimeFromYMD(int year, int month, int day)
        {
            long timeDiff = serverTimestamp - GetTimestamp();
            DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(year, month, day));
            return GetTimestamp(dateTime) + timeDiff;
        }

        /// <summary>
        /// 服务端的时间戳
        /// </summary>
        /// <param name="serverTime"></param>
        public static int GetServerTimePastInSeconds(int serverTime)
        {
            return (int)serverTimestamp - serverTime;
            //var timeDiff = TimestampToDateTime(CurrentServerTimeInSecond) - TimestampToDateTime(serverTime);
            //return (int)timeDiff.TotalSeconds;
        }

        public static DateTime TimestampToDateTime(long timestamp)
        {
            // DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime start = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return start.AddSeconds(timestamp);
        }

        // public static bool IsServerOpenOverDays(int days)
        // {
        //     DateTime enddate = TimeUtils.TimestampToDateTime(Uqee.Caches.SimpleCache.serverOpenTime).AddHours(-8).AddDays(days);
        //     return (enddate > TimeUtils.serverDateTime);
        // }
        public static DateTime localTime1970 = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

        /// </summary>
        /// 获取当前时间戳(long秒)
        /// </summary>
        /// <returns>long</returns>
        public static long GetTimestamp()
        {
            return GetTimestamp(DateTime.Now);
        }

        public static long GetTimestampFromHMS(int h = 0, int m = 0, int s = 0)
        {
            if (h > 24 || h < 0) h = 0;
            if (m > 60 || m < 0) m = 0;
            if (s > 60 || s < 0) s = 0;
            return GetTimestamp(new DateTime(serverDateTime.Year, serverDateTime.Month, serverDateTime.Day, h, m, s));
        }
        /// <summary>
        /// 自零点后经历了多少分钟
        /// </summary>
        /// <param name="min"></param>
        /// <returns></returns>
        public static long GetTimeStampFromMinutes(int min)
        {
            var temp =  GetTimestamp(_zeroDt);
            return temp + min * 60 ;
        }

        public static long GetTimestamp(DateTime dataTime)
        {
            TimeSpan ts = dataTime - localTime1970;
            return Convert.ToInt64(ts.TotalSeconds);
        }

        public static long GetMilliseconds()
        {
            return GetMilliseconds(DateTime.Now);
        }

        public static long GetMilliseconds(DateTime dataTime)
        {
            TimeSpan ts = dataTime - localTime1970;
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        public static string GetNowTimeStr(string fmt)
        {
            return DateTime.Now.ToString(fmt);
        }

        public static string FormatMMSS(int seconds)
        {
            int mm;
            int ss;
            var timeSpan = TimeSpan.FromSeconds(seconds);
            mm = timeSpan.Minutes;
            ss = timeSpan.Seconds;
            //if (timeSpan.Minutes < timeSpan.TotalMinutes)//少于1分钟.会走提示
            //如果超过1小时.就算过大..
            if(timeSpan.TotalHours>1.0)
            {
                Uqee.Debug.LogWarning("FormatMMSS:Too Large" + seconds);
            }
            return string.Format("{0,2:00}:{1,2:00}", mm, ss);
        }

        public static string FormatHHMMSS(int seconds)
        {
            int hh;
            int mm;
            int ss;
            var timeSpan = TimeSpan.FromSeconds(seconds);
            hh = (int)timeSpan.TotalHours;
            mm = timeSpan.Minutes;
            ss = timeSpan.Seconds;
            
            if (timeSpan.TotalHours > 99)
            {
                return string.Format("{0}:{1,2:00}:{2,2:00}", hh, mm, ss);
            }
            else
            {
                return string.Format("{0,2:00}:{1,2:00}:{2,2:00}", hh, mm, ss);
            }
           
        }
        //小时和分
        public static string FormatHHMM(int seconds)
        {
            int hh;
            int mm;
            int ss;
            var timeSpan = TimeSpan.FromSeconds(seconds);
            hh = (int)timeSpan.TotalHours;
            mm = timeSpan.Minutes;
            ss = timeSpan.Seconds;
            if (ss > 0 && mm == 0) //最后一分钟
            {
                mm = 1;
            }
           return string.Format("{0,2:00}:{1,2:00}", hh, mm);
        }
        public static string FormatTime(int seconds, string format = "mm\\:ss")
        {
            return TimeSpan.FromSeconds(seconds).ToString(format);
        }

        /// </summary>
        /// 根据format获取传入时间戳的字符串
        /// </summary>
        /// <returns>string</returns>
        public static string GetFormatTimeStr(string format, long timestamp)
        {
            DateTime dt = TimeUtils.TimestampToDateTime(timestamp);
            //string.Format("{0:HH\\:mm\\:ss}", dt);
            return string.Format(format, dt);
        }

        /// </summary>
        /// 根据传入时间戳或秒数，生成字符串
        /// type=1, x小时 或 x分钟 或 x秒
        /// type=2, x天前 或 x小时前 。。。
        /// </summary>
        /// <returns>string</returns>
        public static string GetTimeStr(long timestamp, int type = 1)
        {
            if (type == 1)
            {
                var timeSpan = TimeSpan.FromSeconds(timestamp);
                if (timestamp >= 3600 * 24)
                {
                    return (int)timeSpan.TotalHours + "小时";
                }
                else if (timeSpan.Hours > 0)
                {
                    return timeSpan.Hours + "小时";
                }
                else if (timeSpan.Minutes > 0)
                {
                    return timeSpan.Minutes + "分钟";
                }
                return timeSpan.Seconds + "秒";
            }
            if (type == 2)
            {
                int s = (int)(timestamp % 60);
                int m = (int)(timestamp / 60);
                int h = m / 60;
                int d = h / 24;
                if (d > 0) return d + "天前";
                if (h > 0) return h + "小时前";
                if (m > 0) return m + "分钟前";
                if (s > 0) return s + "秒前";
            }
            if (type == 3)
            {
                //int s = (int)(timestamp % 60);
                int m = (int)(timestamp / 60);
                int h = m / 60;
                int d = h / 24;

                int hh = h % 24;
                int mm = m % 60;
                int ss = (int)timestamp % 60;
                return string.Format("{0,2:00}天{1,2:00}小时{2,2:00}分{3,2:00}秒", d, hh, mm, ss);
            }
            if (type == 4)
            {
                int s = (int)(timestamp % 60);
                int m = (int)(timestamp / 60);
                int h = m / 60;
                int d = h / 24;

                int hh = h % 24;
                int mm = m % 60;

                if (timestamp > 3600 * 24)
                {
                    return string.Format("{0}天{1,2:00}小时{2,2:00}分{3,2:00}秒", d, hh, mm, s);
                }
                else if (timestamp > 3600)
                {
                    return string.Format("{0,2:00}小时{1,2:00}分{2,2:00}秒", hh, mm, s);
                }
                else if (timestamp > 60)
                {
                    return string.Format("{0,2:00}分{1,2:00}秒", mm, s);
                }
                else
                {
                    return string.Format("{0,2:00}秒", s);
                }
            }
            if (type == 5)
            {
                int s = (int)(timestamp % 60);
                int m = (int)(timestamp / 60);
                int h = m / 60;
                int d = h / 24;
                if (d > 0) return d + "天";
                if (h > 0) return h + "小时";
                if (m > 0) return m + "分钟";
                if (s > 0) return s + "秒";
            }
            if (type == 6)
            {
                int s = (int)(timestamp % 60);
                int m = (int)(timestamp / 60) % 60;
                int h = (int)(timestamp / 60) / 60;
                return string.Format("{0, 2:00}:{1, 2:00}:{2, 2:00}", h, m, s);
            }
            else if (type == 7)//xx天xx小时
            {
                string str = "";
                var timeSpan = TimeSpan.FromSeconds(timestamp);
                if (timeSpan.Days > 0)
                {
                    str = StrOpe.i + timeSpan.Days + "天";
                }
                if (timeSpan.Hours > 0)
                {
                    str += StrOpe.i + timeSpan.Hours + "小时";
                }
                return str;
            }
            return "";
        }

        /// <summary> 需要涵盖时间大于24小时的情况 </summary>
        public static string GetHMSTime(int count)
        {
            var timeSpan = TimeSpan.FromSeconds(count);
            if (timeSpan.TotalHours >=1.0)
            {
                return ((int)timeSpan.TotalHours).ToString() + "小时";
            }
            else if (timeSpan.Minutes > 0)
            {
                return timeSpan.Minutes.ToString() + "分钟";
            }
            return timeSpan.Seconds.ToString() + "秒";
        }

        public static DateTime GetDayOfWeek(int addDay = 0, int addHour = 0)
        {
            //得到星期几 星期天为7
            int dayOfWeek = (int)(DateTime.Now.DayOfWeek) < 1 ? 7 : (int)(DateTime.Now.DayOfWeek);

            //本周一
            DateTime monday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1 - dayOfWeek);

            //本周 星期天
            DateTime dt = new DateTime(monday.Year, monday.Month, monday.Day).AddDays(addDay).AddHours(addHour);

            //return monday.AddDays(addDay);

            return dt;
        }

        /// <summary>
        /// 根据服务器时间返回当前是星期几
        /// 7: 周日
        /// 1: 周一
        /// 2：周二
        /// 3：周三
        /// 4：周四
        /// 5：周五
        /// 6：周六
        /// </summary>
        /// <returns></returns>
        public static int GetDayOfWeekByServerTime()
        {
            var v = Convert.ToInt32(serverDateTime.DayOfWeek);
            return v == 0 ? 7 : v;
        }

        //获取即将到来的最近一次t点的时间戳
        public static long GetNextDayTime(int t)
        {
            //当前日期
            var cdate = TimestampToDateTime(serverTimestamp);
            //今日t点
            var stamp = serverTimestamp - cdate.Hour * 3600 - cdate.Minute * 60 - cdate.Second + t * 3600;
            if(serverTimestamp >= stamp)
            {
                stamp += 24 * 3600;
            }
            return stamp;
        }

        public static long GetNextRefreshTime(int _refreshHour,int _refreshMinute)
        {
            var serverDate = TimeUtils.serverDateTime;
            var h = serverDate.Hour;
            var m = serverDate.Minute;
            var s = serverDate.Second;
            var dis = 0;
            if (h >= _refreshHour)
            {
                dis = (24 - (h - _refreshHour)) * 60 * 60;

            }
            else
            {
                dis = (_refreshHour - h) * 60 * 60;
            }

            if (m > _refreshMinute)
            {
                dis -= (m - _refreshMinute) * 60;
            }
            else
            {
                dis += (_refreshMinute - m) * 60;
            }
            dis -= s;
            return (TimeUtils.serverTimestamp + dis);
        }

        public static long GetNextRefreshTime(int weekDay, int _refreshHour, int _refreshMinute)
        {
            var serverDate = TimeUtils.serverDateTime;
            var h = serverDate.Hour;
            var m = serverDate.Minute;
            var s = serverDate.Second;
            var dis = 0;
            if (h >= _refreshHour)
            {
                dis = (24 - (h - _refreshHour)) * 60 * 60;
            }
            else
            {
                dis = (_refreshHour - h) * 60 * 60;
            }

            if (m > _refreshMinute)
            {
                dis -= (m - _refreshMinute) * 60;
            }
            else
            {
                dis += (_refreshMinute - m) * 60;
            }
            dis -= s;

            var newDate = serverDate.AddSeconds(dis);
            var wd = (int)newDate.DayOfWeek;
            if (wd >= weekDay)
            {
                dis += (6 - (wd - weekDay)) * 24 * 60 * 60;
            }
            else
            {
                dis += (weekDay - wd) * 24 * 60 * 60;
            }

            return (TimeUtils.serverTimestamp + dis);
        }

    }
}