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
        public static ACI agilorACI = ACI.Instance("AgilorReader", "127.0.0.1");


        [Route("")]
        public AgilorResponseData Get() {
            AgilorResponseData response = new AgilorResponseData();

            response.responseMessage = "Hello, Agilor!";
            response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;

            return response;
        }

        /// <summary>
        /// 根据设备名查询所有点，返回点的简单信息：id和名称
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        [Route("targets/byDeviceName/{deviceName}")]
        public AgilorResponseData GetAllTargetsByDeviceName(string deviceName) {
            AgilorResponseData response = new AgilorResponseData();

            response.responseMessage = "Get All Targets By Device Name Success!";
            response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;

            if (agilorACI != null) {
                var result = agilorACI.getTargetsByDevice(deviceName);
                response.reponseBody = result;
            } else {
                response.responseMessage = "Get All Targets By Device Name Error: AgilorACI Is NULL!";
                response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_AGILOR_ACI_IS_NULL;
            }

            return response;
        }

        /// <summary>
        /// 根据点名获取点的详细信息
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        [Route("targets/byTargetName/{targetName}")]
        public AgilorResponseData GetTargetInfoByTargetName(string targetName) {
            AgilorResponseData response = new AgilorResponseData();

            response.reponseBody = agilorACI.GetTarget(targetName);
            response.responseMessage = "Get Target Information By Target Name Success!";
            response.responseCode = (int)AgilorResponseData.RESPONSE_CODE.RESPONSE_NORMAL;

            return response;
        }

        ~AgilorReaderController() {
            if (agilorACI != null) {
                agilorACI.Dispose();
            }
        }
    }
}
