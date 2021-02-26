

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicPlayer
{
    /// <summary>
    ///     请求对象，带泛型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiRequest<T> : ApiRequest

    {
        /// <summary>
        /// </summary>
        public ApiRequest()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="apiRequest"></param>
        public ApiRequest(ApiRequest apiRequest)
        {
            ServerAddress = apiRequest.ServerAddress;
            ApiPath = apiRequest.ApiPath;
            Method = apiRequest.Method;
            AppKey = apiRequest.AppKey;
            UserDefined = apiRequest.UserDefined;
        }

        /// <summary>
        ///     请求参数
        /// </summary>
        public T Param { get; set; }
    }

    /// <summary>
    ///     请求对象
    /// </summary>
    public class ApiRequest
    {
        private const string EndMark = "/";

        private string _serverAddress;
        private string _apiPath;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ApiRequest()
        {
            UserDefined = new Dictionary<string, string>();
        }

        /// <summary>
        ///     服务器地址，如：http://192.168.1.1:8080/
        /// </summary>
        public string ServerAddress
        {
            get { return _serverAddress; }
            set
            {
                if (string.IsNullOrEmpty(value) || value.EndsWith(EndMark))
                {
                    _serverAddress = value;
                    return;
                }

                _serverAddress = value + EndMark;
            }
        }

        /// <summary>
        ///     请求地址，如：api/login
        /// </summary>
        public string ApiPath
        {
            get { return _apiPath; }
            set
            {
                if (string.IsNullOrEmpty(value) || (value.StartsWith(EndMark) == false))
                {
                    _apiPath = value;
                    return;
                }

                _apiPath = value.Substring(1, value.Length - 1);
            }
        }

        /// <summary>
        ///     请求方法
        /// </summary>
        public Method Method { get; set; }

        /// <summary>
        ///     AppKey
        /// </summary>
        public KeyValuePair<string, string> AppKey { get; set; }

        /// <summary>
        ///     ApiTimeOut
        /// </summary>
        public int? ApiTimeOut { get; set; }

        /// <summary>
        ///     自定义的请求头
        /// </summary>
        public Dictionary<string, string> UserDefined { get; set; }
    }
}
