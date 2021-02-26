


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MusicPlayer
{
    /// <summary>
    ///     响应对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T> : ApiResponse
    {
        /// <summary>
        /// </summary>
        public ApiResponse()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="apiResponse"></param>
        public ApiResponse(ApiResponse apiResponse)
        {
            Success = apiResponse.Success;
            ErrorMessage = apiResponse.ErrorMessage;
        }

        /// <summary>
        ///     响应内容，如果请求成功，则可能会返回响应的内容
        /// </summary>
        public T Context { get; set; }
    }

    /// <summary>
    ///     响应对象
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        ///     响应结果
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     响应状态码
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        ///     当请求失败时，返回的失败信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 反序列号错误信息
        /// </summary>
        /// <typeparam name="TError">错误信息类型</typeparam>
        /// <returns>错误信息</returns>
        public TError DeserializeErrorMessage<TError>()
        {
            return string.IsNullOrEmpty(ErrorMessage) ? default(TError) : JsonConvert.DeserializeObject<TError>(ErrorMessage);
        }

        /// <summary>
        /// 服务器时间(默认是UTC时间，需要的话请转local)
        /// </summary>
        public DateTimeOffset? HttpServiceDateTime { get; set; }

    }
}
