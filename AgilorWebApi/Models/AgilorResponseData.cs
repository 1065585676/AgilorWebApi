using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgilorWebApi.Models {
    public class AgilorResponseData {

        public enum RESPONSE_CODE {
            RESPONSE_NORMAL = 1000,
            RESPONSE_AGILOR_ACI_IS_NULL = 1001,
            RESPONSE_TARGET_VALUE_ERROR = 1002,
            RESPONSE_SUBSCRIBE_TIMEOUT_ERROR = 1003,
            RESPONSE_MISS_TARGET_PROPERTY_ERROR = 1004,

            RESPONSE_UNKNOWN_ERROR = 9999
        }

        public int responseCode { get; set; }           // 响应码，标识操作结果

        public string responseMessage { get; set; }     // 响应消息，标识操作结果描述

        public object reponseBody { get; set; }         // 响应体，存储响应数据等
    }
}