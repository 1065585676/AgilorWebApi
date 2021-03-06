﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using AgilorWebApi.Models;
using Agilor.Interface;

using AgilorWebApi.Service;

namespace AgilorWebApi.Controllers
{
    [RoutePrefix("AgilorReader")]
    public class AgilorReaderController : ApiController
    {
        private const string ACI_SERVER_NAME = "AgilorReader";
        private const string ACI_SERVER_IP = "127.0.0.1";

        public static ACI agilorACI = ACI.Instance(ACI_SERVER_NAME, ACI_SERVER_IP);

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public AgilorResponseData Get()
        {
            AgilorResponseData response = new AgilorResponseData();

            response.responseMessage = "Hello, Agilor!";
            response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            response.responseBody = new string[]
            {
                "devices",
            };

            return response;
        }

        /// <summary>
        /// 获取全部设备的名称和状态
        /// </summary>
        /// <returns></returns>
        [Route("devices")]
        public AgilorResponseData GetAllDeviceNameAndStatus()
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                var result = agilorACI.getDevices();
                response.responseBody = result;
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
                response.responseMessage = "Get All Device Names And Status Success! Devices Count:" + result.Count.ToString();
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }


        /// <summary>
        /// 根据设备名查询所有点，返回点的简单信息：id和名称
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        [Route("devices/{deviceName}")]
        public AgilorResponseData GetAllTargetsIDAndNameByDeviceName(string deviceName)
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                var result = agilorACI.getTargetsByDevice(deviceName);
                response.responseBody = result;
                response.responseMessage = "Get All Targets By Device Name Success! Targets Count:" + result.Count.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }

        /// <summary>
        /// 根据点名查询点值，点名可以是多个，用‘；’隔开即可。
        /// </summary>
        /// <param name="targetNames"></param>
        /// <returns></returns>
        [Route("devices/{deviceName}/{targetNames}")]
        public AgilorResponseData GetTargetValueByTargetName(string targetNames)
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                var body = agilorACI.QuerySnapshots(targetNames.Split(';'));
                for(int i = 0; i < body.Count; i++) {
                    var val = body[i];
                    if(val.Type == Agilor.Interface.Val.Value.Types.STRING) {
                        
                        val.Val = ((string)val.Val).Replace("\0", "");
                    }
                }
                response.responseBody = body;
                response.responseMessage = "Get Target Values By Target Names Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }

        /// <summary>
        /// 设置点值，设置成功后返回设置前的点值
        /// </summary>
        /// <param name="targetName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("devices/{deviceName}/{targetName}/set")]
        [HttpPost]
        public AgilorResponseData SetTargetValue(string targetName, dynamic obj)
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                response.responseMessage = "Set Target Value Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;

                object val = null;
                try
                {
                    switch (agilorACI.GetTarget(targetName).Type)
                    {
                        case Agilor.Interface.Val.Value.Types.BOOL:
                            val = obj.targetValue.ToObject<bool>();
                            break;
                        case Agilor.Interface.Val.Value.Types.FLOAT:
                            val = obj.targetValue.ToObject<float>();
                            break;
                        case Agilor.Interface.Val.Value.Types.LONG:
                            val = obj.targetValue.ToObject<int>();
                            break;
                        case Agilor.Interface.Val.Value.Types.STRING:
                            val = obj.targetValue.ToObject<string>();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response.responseMessage = ex.ToString();
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_TARGET_VALUE_ERROR;
                    return response;
                }

                agilorACI.SetValue(new Agilor.Interface.Val.Value(targetName, val));
                response.responseBody = agilorACI.QuerySnapshots(targetName);
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }

        /// <summary>
        /// 根据点名获取点的详细属性信息
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        [Route("devices/{deviceName}/{targetName}/property")]
        public AgilorResponseData GetTargetPropertyByTargetName(string targetName)
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                response.responseBody = agilorACI.GetTarget(targetName);
                response.responseMessage = "Get Target Information By Target Name Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }

        /// <summary>
        /// 根据点名查看点历史值，默认一个月内历史记录
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        [Route("devices/{deviceName}/{targetName}/history")]
        public AgilorResponseData GetTargetHistoryByTargetName(string targetName)
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                response.responseBody = agilorACI.QueryTagHistory(targetName, DateTime.Now.AddMonths(-1), DateTime.Now);
                response.responseMessage = "Get Target History Information By Target Name Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }

        /// <summary>
        /// 检查 ACI 是否为空
        /// </summary>
        /// <returns></returns>
        private bool checkACIObject()
        {
            if (agilorACI != null) return true;

            try
            {
                agilorACI = ACI.Instance(ACI_SERVER_NAME, ACI_SERVER_IP);
                if (agilorACI != null) return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 订阅操作
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("watch")]
        [HttpPost]
        public AgilorResponseData watch(dynamic obj)
        {
            AgilorResponseData response = new AgilorResponseData();

            // 关闭清理线程
            SubscribeManager.StopSubscribeClearThread();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                List<string> targetsName = obj.targetsName.ToObject<List<string>>();
                int timeout = obj.timeout.ToObject<int>();
                response.responseBody = SubscribeManager.addSubscribe(targetsName, timeout);
                response.responseMessage = "Watch Success! Targets Count:" + targetsName.Count.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            // 开启订阅清理线程
            SubscribeManager.StartUpSubscribeClearThread();

            return response;
        }

        /// <summary>
        /// 获取订阅点值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("poll")]
        [HttpPost]
        public AgilorResponseData poll(dynamic obj)
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                string subscriberGuid = obj.guid.ToObject<string>();
                if (DateTime.Now.Subtract(SubscribeManager.subscribers[subscriberGuid].lastPollTime).Duration().TotalSeconds > SubscribeManager.subscribers[subscriberGuid].timeout)
                {
                    // 订阅过期
                    response.responseMessage = "Sorry, Your Subscribe Is Timeout!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_SUBSCRIBE_TIMEOUT_ERROR;
                    return response;
                }

                bool isRefresh = obj.isRefresh.ToObject<bool>();
                //response.responseBody = SubscribeManager.getSubscribeTargetsValue(subscriberGuid, isRefresh);
                var result = SubscribeManager.getSubscribeTargetsValue(subscriberGuid, isRefresh);
                response.responseBody = new Dictionary<string, object>
                {
                    { "SubscribeTargetValues" , result },
                    { "Timeout", SubscribeManager.subscribers[subscriberGuid].timeout }
                };

                response.responseMessage = "Get Subscribe Targets Value Success! Targets Count:" + result.Count.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }

        /// <summary>
        /// 添加单个点
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("targets/addone")]
        [HttpPut]
        public AgilorResponseData addOneTarget(dynamic obj)
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                //target.Name = obj.Name.ToObject<string>();
                //target.SourceName = obj.SourceName.ToObject<string>();
                //target.Device = obj.Device.ToObject<string>();
                //target.Type = Agilor.Interface.Val.Value.Types.FLOAT;
                //target.Scan = obj.Device.ToObject<Target.Status>();
                //target.Descriptor = "";
                //target.SourceGroup = "";
                //target.Id = 12;
                //target.Archiving = true;
                //target.Compressing = true;
                //target.DateCreated = DateTime.Now;
                //target.HihiLimit = 12.5f;
                //target.HiLimit = 12.6f;
                //target.LastTime = DateTime.Now;
                //target.LoLimit = 12.5f;
                //target.LoloLimit = 12.5f;

                Target target = null;

                try
                {
                    target = new Target(obj.Type.ToObject<Agilor.Interface.Val.Value.Types>());
                }
                catch
                {
                    response.responseMessage = "Miss Target Type!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }

                try
                {
                    target.Name = obj.Name.ToObject<string>();
                }
                catch
                {
                    response.responseMessage = "Miss Target Name!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }

                try
                {
                    target.SourceName = obj.SourceName.ToObject<string>();
                }
                catch
                {
                    response.responseMessage = "Miss Target SourceName!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }

                try
                {
                    target.Device = obj.Device.ToObject<string>();
                }
                catch
                {
                    response.responseMessage = "Miss Target Device!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }

                try
                {
                    target.Scan = obj.Scan.ToObject<Target.Status>();
                }
                catch
                {
                    response.responseMessage = "Miss Target Scan!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }

                // 可选
                try
                {
                    target.Descriptor = obj.Descriptor.ToObject<string>();
                }
                catch { }
                try
                {
                    target.SourceGroup = obj.SourceGroup.ToObject<string>();
                }
                catch { }
                try
                {
                    target.Id = obj.Id.ToObject<int>();
                }
                catch { }
                try
                {
                    target.Archiving = obj.Archiving.ToObject<bool>();
                }
                catch { }
                try
                {
                    target.Compressing = obj.Compressing.ToObject<bool>();
                }
                catch { }

                target.DateCreated = DateTime.Now;

                try
                {
                    target.HihiLimit = obj.HihiLimit.ToObject<float>();
                }
                catch { }
                try
                {
                    target.HiLimit = obj.HiLimit.ToObject<float>();
                }
                catch { }

                target.LastTime = DateTime.Now;

                try
                {
                    target.LoLimit = obj.LoLimit.ToObject<float>();
                }
                catch { }
                try
                {
                    target.LoloLimit = obj.LoloLimit.ToObject<float>();
                }
                catch { }

                bool isOverride = true;
                try
                {
                    isOverride = obj.isOverride.ToObject<bool>();
                }
                catch { }

                agilorACI.addTarget(target, isOverride);
                response.responseBody = target;
                response.responseMessage = "Add New Target Success! 'isOverride' IS " + isOverride.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }

        /// <summary>
        /// 添加多个点
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("targets/add")]
        [HttpPut]
        public AgilorResponseData addTargets(dynamic obj)
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                bool isOverride = true;
                try
                {
                    isOverride = obj.isOverride.ToObject<bool>();
                }
                catch { }

                Target[] targets = null;
                try
                {
                    targets = obj.targets.ToObject<Target[]>();
                }
                catch
                {
                    response.responseMessage = "Miss targets!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }

                foreach (var target in targets)
                {
                    agilorACI.addTarget(target, isOverride);
                }

                response.responseBody = new Dictionary<string, object>
                {
                    { "NewTargetsCount", targets.Length },
                    { "isOverride", isOverride }
                };
                response.responseMessage = "Add New Targets Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }

        /// <summary>
        /// 删除点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("targets/delete/{id}")]
        [HttpDelete]
        public AgilorResponseData removeTargets(int id)
        {
            AgilorResponseData response = new AgilorResponseData();

            if (!checkACIObject())
            {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }

            try
            {
                agilorACI.removeTarget(id);
                response.responseMessage = "Remove Targets Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }

            return response;
        }

        /// <summary>
        /// 释放 ACI
        /// </summary>
        ~AgilorReaderController()
        {
            if (agilorACI != null)
            {
                agilorACI.Dispose();
            }
        }
    }
}
