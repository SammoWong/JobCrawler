using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JobCrawler.Common
{
    public static class RedisHelper
    {
        private static readonly string Connstr = ConfigurationManager.GetSection("RedisConnection");

        private static object _locker = new Object();
        private static ConnectionMultiplexer _instance = null;

        /// <summary>
        /// 返回已连接的实例， 
        /// </summary>
        private static ConnectionMultiplexer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        if (_instance == null || !_instance.IsConnected)
                        {
                            _instance = ConnectionMultiplexer.Connect(Connstr);
                        }
                    }
                }
                //注册如下事件
                //_instance.ConnectionFailed += MuxerConnectionFailed;
                //_instance.ConnectionRestored += MuxerConnectionRestored;
                //_instance.ErrorMessage += MuxerErrorMessage;
                //_instance.ConfigurationChanged += MuxerConfigurationChanged;
                //_instance.HashSlotMoved += MuxerHashSlotMoved;
                //_instance.InternalError += MuxerInternalError;
                return _instance;
            }
        }
        static RedisHelper()
        {
        }

        /// <summary>
        /// 获取一个连接实例
        /// </summary>
        /// <returns></returns>
        public static IDatabase GetDatabase()
        {
            return Instance.GetDatabase();
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        /// <param name="Min">分钟</param>
        /// <returns></returns>
        private static TimeSpan ExpireTimeSpan(double Min)
        {
            bool isUserRedis = bool.Parse(ConfigurationManager.GetSection("IsUseRedis"));

            if (isUserRedis)
                return TimeSpan.FromMinutes(Min);

            return TimeSpan.FromMilliseconds(1);
        }

        /// <summary>
        /// 清除 包含特定字符的所有缓存
        /// </summary>
        public static void RemoveSpeStr(string keyStr)
        {
            List<string> listKeys = GetAllKeys();
            foreach (string k in listKeys)
            {
                if (k.Contains(keyStr))
                {
                    Remove(k);
                }
            }
        }

        /// <summary>
        /// 判断在缓存中是否存在该key的缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exists(string key)
        {
            return GetDatabase().KeyExists(key);  //可直接调用
        }

        /// <summary>
        /// 移除指定key的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove(string key)
        {
            if (Exists(key))
            {
                return GetDatabase().KeyDelete(key);
            }
            return false;
        }

        /// <summary>
        ///  Set
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="t">值</param>
        /// <param name="timeout">多少分钟后过期</param>
        /// <returns></returns>
        public static bool Set<T>(string key, T t, double minOut = 60)
        {
            return GetDatabase().StringSet(key, JsonConvert.SerializeObject(t), ExpireTimeSpan(minOut));
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            return JsonConvert.DeserializeObject<T>(GetDatabase().StringGet(key));
        }

        /// <summary>
        /// DataSet 缓存
        /// </summary> 
        public static bool SetData(string key, DataSet ds, double minOut = 60 * 3)
        {
            return GetDatabase().StringSet(key, JsonConvert.SerializeObject(ds), ExpireTimeSpan(minOut));
        }

        /// <summary>
        /// 获取 DataSet 
        /// </summary> 
        public static DataSet GetDataSet(string key)
        {
            return JsonConvert.DeserializeObject<DataSet>(GetDatabase().StringGet(key));
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        public static void FlushAll()
        {
            var endpoints = Instance.GetEndPoints();
            var server = Instance.GetServer(endpoints.First());

            server.FlushDatabase(); // to wipe a single database, 0 by default
            //server.FlushAllDatabases(); // to wipe all databases
        }

        /// <summary>
        /// 得到所有缓存键值
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllKeys()
        {
            List<string> lstKey = new List<string>();
            var endpoints = Instance.GetEndPoints();
            var server = Instance.GetServer(endpoints.First());
            var keys = server.Keys();
            foreach (var key in keys)
            {
                lstKey.Add(key);
            }
            return lstKey;
        }
    }
}
