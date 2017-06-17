using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Agilor.Interface;
using Agilor.Interface.Val;
using AgilorWebApi.Controllers;

using System.Threading;

namespace AgilorWebApi.Service
{
    public class SubscribeManager
    {
        /// <summary>
        /// 订阅者信息结构
        /// </summary>
        public class OneSubscriber
        {
            public string subscriberGuid;
            public List<string> subscribeTargetNames;
            public int timeout;
            public DateTime lastPollTime;
        };

        /// <summary>
        /// 订阅点结构
        /// </summary>
        public class OneSubscribeTarget
        {
            public string targetName;
            public Value value;
            public int maxTimeout;
            public DateTime lastValueUpdateTime;
            public DateTime lastPollTime;
            public IWatcher handler;
        };

        // 订阅者集合
        public static Dictionary<string, OneSubscriber> subscribers = new Dictionary<string, OneSubscriber>();

        // 所有订阅的点集合
        public static Dictionary<string, OneSubscribeTarget> subscribeTargets = new Dictionary<string, OneSubscribeTarget>();

        public static Thread subscribeClearThread = new Thread(SubscribeClearThreadFunction);
        public static bool switchSubscribeClearThread = true;
        public static int subscribeClearIntervalSecond = 10;

        /// <summary>
        /// 订阅清理线程处理函数
        /// </summary>
        public static void SubscribeClearThreadFunction()
        {
            while (true)
            {
                if (switchSubscribeClearThread)
                {
                    // 清理订阅者
                    int i = 0;
                    while (i < subscribers.Count)
                    {
                        if (!switchSubscribeClearThread) break;
                        if (DateTime.Now.Subtract(subscribers.ElementAt(i).Value.lastPollTime).Duration().TotalSeconds > subscribers.ElementAt(i).Value.timeout)
                        {
                            subscribers.Remove(subscribers.ElementAt(i).Key);
                        }
                        else
                        {
                            i++;
                        }
                    }

                    // 清理订阅点
                    int j = 0;
                    while (j < subscribeTargets.Count)
                    {
                        if (!switchSubscribeClearThread) break;
                        if (DateTime.Now.Subtract(subscribeTargets.ElementAt(j).Value.lastPollTime).Duration().TotalSeconds > subscribeTargets.ElementAt(j).Value.maxTimeout)
                        {
                            AgilorReaderController.agilorACI.UnWatch(subscribeTargets.ElementAt(j).Key);
                            subscribeTargets.Remove(subscribeTargets.ElementAt(j).Key);
                        }
                        else
                        {
                            j++;
                        }
                    }
                }
                Thread.Sleep(subscribeClearIntervalSecond * 1000);
            }
        }

        /// <summary>
        /// 启动订阅清理线程
        /// </summary>
        /// <returns></returns>
        public static bool StartUpSubscribeClearThread()
        {
            if (subscribeClearThread.ThreadState == ThreadState.Unstarted)
            {
                subscribeClearThread.Start();
            }
            switchSubscribeClearThread = true;
            return true;
        }

        /// <summary>
        /// 停止订阅清理线程
        /// </summary>
        /// <returns></returns>
        public static bool StopSubscribeClearThread()
        {
            switchSubscribeClearThread = false;
            return true;
        }


        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="subscribeTargetsName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string addSubscribe(List<string> subscribeTargetsName, int timeout = 10)
        {
            string guidStr = getGuidString();

            // 添加订阅者
            OneSubscriber subscriber = new OneSubscriber();
            subscriber.subscriberGuid = guidStr;
            subscriber.subscribeTargetNames = subscribeTargetsName;
            subscriber.timeout = timeout;
            subscriber.lastPollTime = DateTime.Now;

            subscribers[guidStr] = subscriber;

            // 添加订阅点
            foreach (var targetName in subscribeTargetsName)
            {
                if (!subscribeTargets.ContainsKey(targetName))
                {
                    OneSubscribeTarget target = new OneSubscribeTarget();
                    target.targetName = targetName;
                    target.value = null;
                    target.maxTimeout = timeout;
                    target.handler = new SimpleWatch();
                    target.lastPollTime = DateTime.Now;
                    target.lastValueUpdateTime = DateTime.Now;

                    subscribeTargets[targetName] = target;

                    // 添加订阅事件
                    AgilorReaderController.agilorACI.Watch(targetName, target.handler);
                }
                else
                {
                    subscribeTargets[targetName].lastPollTime = DateTime.Now;
                    subscribeTargets[targetName].maxTimeout = Math.Max(timeout, subscribeTargets[targetName].maxTimeout);
                }
            }

            return guidStr;
        }

        /// <summary>
        /// 获取订阅点的值，isAll表示点值未更新也返回
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="isAll"></param>
        /// <returns></returns>
        public static List<Value> getSubscribeTargetsValue(string subscriberGuid, bool isAll)
        {
            DateTime lastSubPoll = subscribers[subscriberGuid].lastPollTime;
            subscribers[subscriberGuid].lastPollTime = DateTime.Now;

            List<Value> result = new List<Value>();
            var targetsName = subscribers[subscriberGuid].subscribeTargetNames;
            foreach (var name in targetsName)
            {
                subscribeTargets[name].lastPollTime = DateTime.Now;
                if (isAll || lastSubPoll < subscribeTargets[name].lastValueUpdateTime)
                {
                    result.Add(subscribeTargets[name].value);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取 GUID
        /// </summary>
        /// <returns></returns>
        public static string getGuidString() { return Guid.NewGuid().ToString(); }

        /// <summary>
        /// 订阅消息处理类
        /// </summary>
        class SimpleWatch : IWatcher
        {
            public void OnHiHiLimit(Value val)
            {
                throw new NotImplementedException();
            }

            public void OnHiLimit(Value val)
            {
                throw new NotImplementedException();
            }

            public void OnLoLimit(Value val)
            {
                throw new NotImplementedException();
            }

            public void OnLoLoLimit(Value val)
            {
                throw new NotImplementedException();
            }

            public void OnReceive(Value val)
            {
                // throw new NotImplementedException();
                SubscribeManager.subscribeTargets[val.Name].value = val;
                SubscribeManager.subscribeTargets[val.Name].lastValueUpdateTime = DateTime.Now;
            }
        }
    }
}