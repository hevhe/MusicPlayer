using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer
{
    public class ApiError
    {
        /// <summary>
        /// 客户端ip
        /// </summary>
        [JsonProperty("clientIp")]
        public string ClientIp { get; set; }

        /// <summary>
        /// 错误编码
        /// </summary>
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// 返回状态
        /// </summary>
        [JsonProperty("httpStatus")]
        public string HttpStatus { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
