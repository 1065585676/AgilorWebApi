using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using AgilorWebApi.Models;
using Agilor.Interface;
using AgilorWebApi.Service;
using System.Configuration;

namespace AgilorWebApi.Controllers
{
    [RoutePrefix("Agilor")]
    public class AgilorController : ApiController
    {
        private static string ACI_SERVER_NAME = ConfigurationManager.AppSettings["AgilorServerName"];
        private static string ACI_SERVER_IP = ConfigurationManager.AppSettings["AgilorServerIp"];
        public static ACI agilorACI = ACI.Instance(ACI_SERVER_NAME, ACI_SERVER_IP);
        public static List<ACI> agilorSlaveACIs = new List<ACI>() { };

        static AgilorController()
        {
            string[] slaveIps = ConfigurationManager.AppSettings["AgilorServerSlaveIp"].Split(';');
            string slaveName = ConfigurationManager.AppSettings["AgilorServerSlaveName"];
            foreach (string slaveIp in slaveIps)
            {
                if (slaveIp.Trim() == "") continue;
                agilorSlaveACIs.Add(ACI.Instance(slaveName + slaveIp.Trim(), slaveIp.Trim()));
            }
        }

        AgilorController() { }

        /// <summary>
        /// 检查 ACI 是否为空
        /// </summary>
        /// <returns></returns>
        private bool checkACIObject()
        {
            if (agilorACI != null) return true;
            try
            {
                ACI_SERVER_NAME = ConfigurationManager.AppSettings["AgilorServerName"];
                ACI_SERVER_IP = ConfigurationManager.AppSettings["AgilorServerIp"];
                agilorACI = ACI.Instance(ACI_SERVER_NAME, ACI_SERVER_IP);
                if (agilorACI != null) return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public AgilorResponseData Get()
        {
            AgilorResponseData response = new AgilorResponseData();
            response.responseMessage = "Hello, I'm " + ACI_SERVER_NAME + ":" + ACI_SERVER_IP;
            response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            response.responseBody = new string[]
            {
                "devices",
                "targets",
                "watch",
                "poll"
            };
            return response;
        }

        /// <summary>
        /// 获取全部设备的名称和状态
        /// </summary>
        /// <returns></returns>
        [Route("devices")]
        [HttpGet]
        public AgilorResponseData GetAllDeviceNameAndStatus()
        {
            AgilorResponseData response = new AgilorResponseData();
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try {
                var result = agilorACI.getDevices();
                response.responseBody = result;
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
                response.responseMessage = "Get All Device Names And Status Success! Devices Count:" + result.Count.ToString();
            }
            catch (Exception ex) {
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
        [HttpGet]
        public AgilorResponseData GetAllTargetsIDAndNameByDeviceName(string deviceName)
        {
            AgilorResponseData response = new AgilorResponseData();
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try {
                var result = agilorACI.getTargetsByDevice(deviceName);
                response.responseBody = result;
                response.responseMessage = "Get All Targets By Device Name Success! Targets Count:" + result.Count.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex) {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }
            return response;
        }

        /// <summary>
        /// 获取全部点值
        /// </summary>
        /// <returns></returns>
        [Route("targets")]
        [HttpGet]
        public AgilorResponseData GetAllTargets()
        {
            AgilorResponseData response = new AgilorResponseData();
            if (!checkACIObject())
            {
                response.responseMessage = "Get All Targets ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try
            {
                var result = agilorACI.getTargetsbyNameMask("*");
                response.responseBody = result;
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
                response.responseMessage = "Get All Targets Success! Targets Count:" + result.Count.ToString();
            }
            catch (Exception ex)
            {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }
            return response;
        }

        /// <summary>
        /// 根据mask点名查询点值，点名可以是多个，用‘；’隔开即可。
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        [Route("devices/{deviceName}/mask/{targetName}")]
        [Route("targets/mask/{targetName}")]
        [HttpGet]
        public AgilorResponseData GetTargetsByMaskName(string targetName)
        {
            AgilorResponseData response = new AgilorResponseData();
            if (!checkACIObject())
            {
                response.responseMessage = "Get All Targets ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try
            {
                var result = agilorACI.getTargetsbyNameMask(targetName);
                response.responseBody = result;
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
                response.responseMessage = "Get All Targets Success! Targets Count:" + result.Count.ToString();
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
        [Route("targets/{targetNames}")]
        [HttpGet]
        public AgilorResponseData GetTargetValueByTargetName(string targetNames)
        {
            AgilorResponseData response = new AgilorResponseData();
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try {
                var body = agilorACI.QuerySnapshots(targetNames.Split(';'));
                for (int i = 0; i < body.Count; i++) {
                    if (body[i].Type == Agilor.Interface.Val.Value.Types.STRING) {
                        body[i].Val = ((string)body[i].Val).Remove(((string)body[i].Val).IndexOf("\0"));
                    }
                }
                response.responseBody = body;
                response.responseMessage = "Get Target Values By Target Names Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex) {
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
        [Route("targets/{targetName}/set")]
        [HttpPost]
        public AgilorResponseData SetTargetValue(string targetName, dynamic obj)
        {
            AgilorResponseData response = new AgilorResponseData();
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try
            {
                response.responseMessage = "Set Target Value Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
                object val = null;
                try {
                    switch (agilorACI.GetTarget(targetName).Type) {
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
                catch (Exception ex) {
                    response.responseMessage = ex.ToString();
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_TARGET_VALUE_ERROR;
                    return response;
                }
                agilorACI.SetValue(new Agilor.Interface.Val.Value(targetName, val));

                foreach (ACI slaveAci in agilorSlaveACIs) {
                    try { slaveAci.SetValue(new Agilor.Interface.Val.Value(targetName, val)); } catch { }
                }
                response.responseBody = agilorACI.QuerySnapshots(targetName);
            }
            catch (Exception ex) {
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
        [Route("targets/{targetName}/property")]
        [HttpGet]
        public AgilorResponseData GetTargetPropertyByTargetName(string targetName)
        {
            AgilorResponseData response = new AgilorResponseData();
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try {
                response.responseBody = agilorACI.GetTarget(targetName);
                response.responseMessage = "Get Target Information By Target Name Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex) {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }
            return response;
        }

        /// <summary>
        /// 根据点名查看点历史值，默认1分钟内历史记录
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        [Route("devices/{deviceName}/{targetName}/history")]
        [Route("targets/{targetName}/history")]
        [HttpGet]
        public AgilorResponseData GetTargetHistoryByTargetName(string targetName, string start = null, string end = null, int step = 0)
        {
           AgilorResponseData response = new AgilorResponseData();
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try {
                DateTime start_t, end_t;
                if (!DateTime.TryParse(end, out end_t)) {
                    end_t = DateTime.Now;
                }
                if (!DateTime.TryParse(start, out start_t)) {
                    int minutes = 1;
                    int.TryParse(ConfigurationManager.AppSettings["AgilorQueryHistoryDefaultIntervalMinute"], out minutes);
                    start_t = end_t.AddMinutes(-1 * minutes);
                }
                if (end_t >= start_t) {
                    var body = agilorACI.QueryTagHistory(targetName, start_t, end_t, step);
                    if (body.Count > 0 && body[0].Type == Agilor.Interface.Val.Value.Types.STRING) {
                        for (int i = 0; i < body.Count; i++) {
                            body[i].Val = ((string)body[i].Val).Remove(((string)body[i].Val).IndexOf("\0"));
                        }
                    }
                    body.Reverse();
                    response.responseBody = body;
                }
                response.responseMessage = "Get Target History Information By Target Name Success! start: " + start_t.ToString() + ", end: " + end_t.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex) {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }
            return response;
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
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try {
                List<string> targetsName = obj.targetsName.ToObject<List<string>>();
                int timeout = obj.timeout.ToObject<int>();
                response.responseBody = SubscribeManager.addSubscribe(targetsName, timeout);
                response.responseMessage = "Watch Success! Targets Count:" + targetsName.Count.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex) {
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
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try {
                string subscriberGuid = obj.guid.ToObject<string>();
                if (!SubscribeManager.subscribers.ContainsKey(subscriberGuid)
                    || DateTime.Now.Subtract(SubscribeManager.subscribers[subscriberGuid].lastPollTime).Duration().TotalSeconds > SubscribeManager.subscribers[subscriberGuid].timeout) {
                    // 订阅过期
                    response.responseMessage = "Sorry, Your Subscribe Is Timeout!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_SUBSCRIBE_TIMEOUT_ERROR;
                    return response;
                }
                bool isRefresh = obj.isRefresh.ToObject<bool>();
                var result = SubscribeManager.getSubscribeTargetsValue(subscriberGuid, isRefresh);
                response.responseBody = new Dictionary<string, object>
                {
                    { "SubscribeTargetValues" , result },
                    { "Timeout", SubscribeManager.subscribers[subscriberGuid].timeout }
                };
                response.responseMessage = "Get Subscribe Targets Value Success! Targets Count:" + result.Count.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex) {
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
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try {
                Target target = null;
                try {
                    target = new Target(obj.Type.ToObject<Agilor.Interface.Val.Value.Types>());
                }
                catch {
                    response.responseMessage = "Miss Target Type!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }
                try {
                    target.Name = obj.Name.ToObject<string>();
                } catch {
                    response.responseMessage = "Miss Target Name!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }
                try {
                    target.SourceName = obj.SourceName.ToObject<string>();
                } catch {
                    response.responseMessage = "Miss Target SourceName!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }
                try {
                    target.Device = obj.Device.ToObject<string>();
                } catch {
                    response.responseMessage = "Miss Target Device!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }
                try {
                    target.Scan = obj.Scan.ToObject<Target.Status>();
                }
                catch {
                    response.responseMessage = "Miss Target Scan!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }
                // 可选
                bool isOverride = true;
                target.LastTime = DateTime.Now;
                target.DateCreated = DateTime.Now;
                try {
                    target.Descriptor = obj.Descriptor.ToObject<string>();
                } catch { }
                try {
                    target.SourceGroup = obj.SourceGroup.ToObject<string>();
                } catch { }
                try {
                    target.Id = obj.Id.ToObject<int>();
                } catch { }
                try {
                    target.Archiving = obj.Archiving.ToObject<bool>();
                } catch { }
                try {
                    target.Compressing = obj.Compressing.ToObject<bool>();
                } catch { }
                try {
                    target.HihiLimit = obj.HihiLimit.ToObject<float>();
                } catch { }
                try {
                    target.HiLimit = obj.HiLimit.ToObject<float>();
                } catch { }
                try {
                    target.LoLimit = obj.LoLimit.ToObject<float>();
                } catch { }
                try {
                    target.LoloLimit = obj.LoloLimit.ToObject<float>();
                } catch { }
                try {
                    isOverride = obj.isOverride.ToObject<bool>();
                } catch { }
                agilorACI.addTarget(target, isOverride);
                response.responseBody = target;
                response.responseMessage = "Add New Target Success! 'isOverride' IS " + isOverride.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex) {
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
            try {
                bool isOverride = true;
                try {
                    isOverride = obj.isOverride.ToObject<bool>();
                } catch { }
                Target[] targets = null;
                try {
                    targets = obj.targets.ToObject<Target[]>();
                } catch {
                    response.responseMessage = "Miss targets!";
                    response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_MISS_TARGET_PROPERTY_ERROR;
                    return response;
                }
                foreach (var target in targets) {
                    agilorACI.addTarget(target, isOverride);
                }
                response.responseBody = new Dictionary<string, object> {
                    { "NewTargetsCount", targets.Length },
                    { "isOverride", isOverride }
                };
                response.responseMessage = "Add New Targets Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            }
            catch (Exception ex) {
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
            if (!checkACIObject()) {
                response.responseMessage = "Get All Device Names And Status ERROR: ACI IS NULL, Server Need Be Restart!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
                return response;
            }
            try {
                agilorACI.removeTarget(id);
                response.responseMessage = "Remove Targets Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
            } catch (Exception ex) {
                response.responseMessage = ex.ToString();
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_UNKNOWN_ERROR;
            }
            return response;
        }

    }
}
