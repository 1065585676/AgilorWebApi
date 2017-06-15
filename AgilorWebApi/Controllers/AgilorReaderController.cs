using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using AgilorWebApi.Models;
using Agilor.Interface;

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
            response.reponseBody = new string[]
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
                response.responseMessage = "Get All Device Names And Status Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
                response.reponseBody = agilorACI.getDevices();
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
                response.responseMessage = "Get All Targets By Device Name Success!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;
                response.reponseBody = agilorACI.getTargetsByDevice(deviceName);
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
                response.reponseBody = agilorACI.QuerySnapshots(targetNames.Split(';'));
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
                            val = (bool)obj.targetValue;
                            break;
                        case Agilor.Interface.Val.Value.Types.FLOAT:
                            val = (float)obj.targetValue;
                            break;
                        case Agilor.Interface.Val.Value.Types.LONG:
                            val = (int)obj.targetValue;
                            break;
                        case Agilor.Interface.Val.Value.Types.STRING:
                            val = (string)obj.targetValue;
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
                response.reponseBody = agilorACI.QuerySnapshots(targetName);
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
                response.reponseBody = agilorACI.GetTarget(targetName);
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
                response.reponseBody = agilorACI.QueryTagHistory(targetName, DateTime.Now.AddMonths(-1), DateTime.Now);
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
