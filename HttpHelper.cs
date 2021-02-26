

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer
{

    public static class HttpHelper
    {
        private const string JsonMediaType = "application/json";
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;
        private static readonly JsonSerializerSettings MyJsonSerializerSettings;

    

        public static string Token { get; set; }

        /// <summary>
        /// Http请求超时时间
        /// </summary>
        public static int ApiTimeOut { get; set; }

        static HttpHelper()
        {
            MyJsonSerializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-dd HH:mm:ss.fff"
            };

            //默认不显示传入的JSON参数
            ShowJsonParametersOnConsole = true;

            //默认超时时间100秒
            ApiTimeOut = 100;

            ObjSerializeToJson = true;

            WriteLogToFile = true;
        }

        /// <summary>
        /// 在控制台上显示Json参数
        /// </summary>
        public static bool ShowJsonParametersOnConsole { get; set; }

        /// <summary>
        /// 对象是否序列化成Json格式
        /// </summary>
        public static bool ObjSerializeToJson { get; set; }

        /// <summary>
        ///     是否把请求的写到log
        /// </summary>
        public static bool WriteLogToFile { get; set; }

        /// <summary>
        /// 请求地址
        /// </summary>
        public static string ServerAddress { get; set; }



        /// <summary>
        ///     调用API，不带请求参数，不带响应数据
        /// </summary>
        /// <param name="apiRequest">Api请求</param>
        /// <param name="throwableException">当设置为true时，发生错误会抛出异常</param>
        /// <returns></returns>
        public static Task<ApiResponse> ExecuteNonResp(ApiRequest apiRequest, bool throwableException = false)
        {
            return Task<ApiResponse>.Factory.StartNew(() =>
                DoExecuteNonResp(new ApiRequest<object>(apiRequest), throwableException));
        }

        /// <summary>
        ///     调用API，带请求参数，不带响应数据
        /// </summary>
        /// <param name="apiRequest">Api请求</param>
        /// <param name="throwableException">当设置为true时，发生错误会抛出异常</param>
        /// <returns></returns>
        public static Task<ApiResponse> ExecuteNonResp<TRequest>(ApiRequest<TRequest> apiRequest,
            bool throwableException = false)
        {
            return Task<ApiResponse>.Factory.StartNew(() => DoExecuteNonResp(apiRequest, throwableException));
        }

        /// <summary>
        ///     调用API，不带请求参数，带响应数据
        /// </summary>
        /// <param name="apiRequest">Api请求</param>
        /// <param name="throwableException">当设置为true时，发生错误会抛出异常</param>
        /// <returns></returns>
        public static Task<ApiResponse<TRsponse>> Execute<TRsponse>(ApiRequest apiRequest,
            bool throwableException = false)
        {
            return Task<ApiResponse<TRsponse>>.Factory.StartNew(() =>
                DoExecute<object, TRsponse>(new ApiRequest<object>(apiRequest), throwableException));
        }

        /// <summary>
        /// 调用API，带请求参数，带响应数据
        /// </summary>
        /// <param name="apiRequest">Api请求</param>
        /// <param name="throwableException">当设置为true时，发生错误会抛出异常</param>
        /// <returns></returns>
        public static Task<ApiResponse<TRsponse>> Execute<TRequest, TRsponse>(ApiRequest<TRequest> apiRequest,
            bool throwableException = false)
        {
            return Task<ApiResponse<TRsponse>>.Factory.StartNew(() =>
                DoExecute<TRequest, TRsponse>(apiRequest, throwableException));
        }


        private static ApiResponse<TRsponse> DoExecute<TRequest, TRsponse>(ApiRequest<TRequest> apiRequest,
            bool throwableException = false)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                using (HttpClient client = CreateHttpClient(apiRequest))
                using (HttpResponseMessage responseMessage = GenerateResponseMessage(client, apiRequest))
                {
                    var response = new ApiResponse<TRsponse>
                    {
                        Success = responseMessage.IsSuccessStatusCode,
                        HttpStatusCode = responseMessage.StatusCode,
                        HttpServiceDateTime = responseMessage.Headers.Date
                    };

                    if (response.Success)
                    {
                        String result = responseMessage.Content.ReadAsStringAsync().Result;
                        var rsponseType = typeof(TRsponse);
                        if (rsponseType == typeof(string))
                        {
                            //基础类型要拆箱转换
                            response.Context = (TRsponse)Convert.ChangeType(result, typeof(TRsponse));
                        }
                        else
                        {
                            var rsponse = JsonConvert.DeserializeObject<TRsponse>(result);
                            response.Context = rsponse;
                        }
                    }
                    else
                    {
                        response.ErrorMessage = responseMessage.Content.ReadAsStringAsync().Result;
                        if (string.IsNullOrEmpty(response.ErrorMessage) == false)
                        {
                            try
                            {
                                var apiError = JsonConvert.DeserializeObject<ApiError>(response.ErrorMessage);
                                if (apiError != null && string.IsNullOrEmpty(apiError.Message) == false)
                                {
                                    response.ErrorMessage = apiError.Message;
                                }
                            }
                            catch (Exception ex)
                            {
                                var e = FindInnerException(ex);
                                response.ErrorMessage = e.Message + ";" + response.ErrorMessage;
                            }
                        }

                        if (throwableException)
                        {
                            sw.Stop();
                            WriteLog(apiRequest, sw.ElapsedMilliseconds,
                                msg: string.Format("方法异常，异常信息{0}", response.ErrorMessage));
                            throw new ApiResponseException(response.HttpStatusCode, response.ErrorMessage);
                        }
                    }

                    sw.Stop();
                    WriteLog(apiRequest, sw.ElapsedMilliseconds, response.ErrorMessage);
                    return response;
                }
            }
            catch (ApiResponseException ex)
            {
                throw new ApiResponseException(ex.HttpStatusCode, FindInnerException(ex).Message);
            }
            catch (Exception ex)
            {
                WriteLog(apiRequest, 0, msg: string.Format("方法异常，异常信息{0}", ex.Message));
                throw FindInnerException(ex);
            }
        }


        private static ApiResponse DoExecuteNonResp<TRequest>(ApiRequest<TRequest> apiRequest,
            bool throwableException = false)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                using (HttpClient client = CreateHttpClient(apiRequest))
                using (HttpResponseMessage responseMessage = GenerateResponseMessage(client, apiRequest))
                {
                    var response = new ApiResponse
                    {
                        Success = responseMessage.IsSuccessStatusCode,
                        HttpStatusCode = responseMessage.StatusCode
                    };

                    if (response.Success == false)
                    {
                        response.ErrorMessage = responseMessage.Content.ReadAsStringAsync().Result;
                        if (string.IsNullOrEmpty(response.ErrorMessage) == false)
                        {
                            try
                            {
                                var apiError = JsonConvert.DeserializeObject<ApiError>(response.ErrorMessage);
                                if (apiError != null && string.IsNullOrEmpty(apiError.Message) == false)
                                {
                                    response.ErrorMessage = apiError.Message;
                                }
                            }
                            catch (Exception ex)
                            {
                                var e = FindInnerException(ex);
                                response.ErrorMessage = e.Message + ";" + response.ErrorMessage;
                            }
                        }

                        if (throwableException)
                        {
                            sw.Stop();
                            WriteLog(apiRequest, sw.ElapsedMilliseconds,
                                msg:
                                string.Format("方法异常，http返回编号{0},异常信息{1}", response.HttpStatusCode,
                                    response.ErrorMessage));
                            throw new ApiResponseException(response.HttpStatusCode, response.ErrorMessage);
                        }
                    }

                    sw.Stop();
                    WriteLog(apiRequest, sw.ElapsedMilliseconds, response.ErrorMessage);
                    return response;
                }
            }
            catch (ApiResponseException ex)
            {
                throw new ApiResponseException(ex.HttpStatusCode, FindInnerException(ex).Message);
            }
            catch (Exception ex)
            {
                WriteLog(apiRequest, 0, msg: string.Format("方法异常，异常信息{0}", ex.Message));
                throw FindInnerException(ex);
            }
        }

        /// <summary>
        ///     初始化HttpClient
        /// </summary>
        /// <param name="apiRequest"></param>
        private static HttpClient CreateHttpClient(ApiRequest apiRequest)
        {
            var client = new HttpClient { BaseAddress = new Uri(apiRequest.ServerAddress) };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));

            if (string.IsNullOrEmpty(apiRequest.AppKey.Key) == false)
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(apiRequest.AppKey.Key, apiRequest.AppKey.Value);

            if (apiRequest.UserDefined != null && apiRequest.UserDefined.Any())
            {
                foreach (KeyValuePair<string, string> keyValuePair in apiRequest.UserDefined)
                {
                    client.DefaultRequestHeaders.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            client.Timeout = TimeSpan.FromSeconds(apiRequest.ApiTimeOut ?? ApiTimeOut);
            client.DefaultRequestHeaders.ExpectContinue = false;
            return client;
        }

        /// <summary>
        ///     获取响应信息
        /// </summary>
        /// <param name="client">HttpClient</param>
        /// <param name="apiRequest"></param>
        /// <returns></returns>
        private static HttpResponseMessage GenerateResponseMessage<TRequest>(HttpClient client,
            ApiRequest<TRequest> apiRequest)
        {
            Method method = apiRequest.Method;
            string apiPath = apiRequest.ApiPath;
            TRequest requestParam = apiRequest.Param;

            if (ShowJsonParametersOnConsole)
                Trace.WriteLine("API -> url : " + apiPath);
            switch (method)
            {
                case Method.Get:
                    return client.GetAsync(apiPath).Result;
                case Method.Post:
                    return client.PostAsync(apiPath, SerializeObjectToJson(requestParam)).Result;
                case Method.Put:
                    return client.PutAsync(apiPath, SerializeObjectToJson(requestParam)).Result;
                case Method.Delete:
                    //return client.DeleteAsync(apiPath).Result;
                    return client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, apiPath)
                    { Content = SerializeObjectToJson(requestParam) }).Result;
                default:
                    throw new ArgumentOutOfRangeException("apiRequest", "apiRequest.Method 类型错误");
            }
        }

        /// <summary>
        ///     序列化对象为JSON字符串
        /// </summary>
        /// <param name="obj">需要转换成JSON的对象</param>
        /// <returns></returns>
        private static StringContent SerializeObjectToJson(object obj)
        {
            var content = ObjSerializeToJson
                ? JsonConvert.SerializeObject(obj, MyJsonSerializerSettings)
                : obj.ToString();

            //显示提交的JSON数据到控制台
            if (ShowJsonParametersOnConsole)
                Trace.WriteLine("API Obj -> Json : " + content);
            var res = new StringContent(content, DefaultEncoding, JsonMediaType);
            return res;
        }

        private static void WriteLog<TRequest>(ApiRequest<TRequest> apiRequest, long elapsedMilliseconds,
            string msg = null)
        {
            if (WriteLogToFile)
            {
                var content = string.Empty;
                var printJson = false;
                if (apiRequest.Param != null)
                {
                    printJson = true;
                    content = JsonConvert.SerializeObject(apiRequest.Param, MyJsonSerializerSettings);
                }

                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("Url:" + apiRequest.ApiPath);
                sb.AppendLine("Method:" + apiRequest.Method);
                sb.AppendLine("Execution time:" + elapsedMilliseconds + " ms");
                if (printJson) sb.AppendLine("Request Json:" + content);
                if (!string.IsNullOrEmpty(msg))
                {
                    sb.AppendLine(msg);
                }

                sb.AppendLine("=================================================");
                Debug.Print(sb.ToString());
            }
        }

        private static Exception FindInnerException(Exception ex)
        {
            if (ex.InnerException != null)
                return FindInnerException(ex.InnerException);
            else
            {
                return ex;
            }
        }

        public static ApiRequest<T> GetCommonApiRequest<T>(Method method, string apiPath, T param, string svrAddr = "")
        {
            var result = new ApiRequest<T>();

            result.ServerAddress = string.IsNullOrEmpty(svrAddr) ? ServerAddress : svrAddr;
            result.Method = method;
            result.ApiPath = apiPath;
            result.AppKey = AppKeyVal;
            if (method == Method.Get)
            {
                var fields =  CommUtility .GetPropertysWithJsonProp(param);
                var paramStr = "";
                var paramStrs = new List<string>();
                foreach (var kv in fields)
                {
                    if (kv.Value != null && !string.IsNullOrEmpty(kv.Value.ToString()))
                    {
                        //paramStrs.Add(System.Web.HttpUtility.UrlEncode(kv.Key) + "=" + System.Web.HttpUtility.UrlEncode(kv.Value.ToString()));
                    }
                }
                paramStr = string.Join("&", paramStrs);
                if (!string.IsNullOrEmpty(paramStr))
                {
                    result.ApiPath += "?" + paramStr;
                }
            }
            else
            {
                result.Param = param;
            }

            return result;
        }

        public static ApiRequest GetCommonApiRequest(Method method, string apiPath, string svrAddr = "")
        {
            var result = new ApiRequest();

            result.ServerAddress = string.IsNullOrEmpty(svrAddr) ? ServerAddress : svrAddr;
            result.Method = method;
            result.ApiPath = apiPath;
            result.AppKey = AppKeyVal;
            return result;
        }

        //public static string UrlEncode(string str, params object[] paras)
        //{
        //    //string[] paraObj = paras.Select(v => System.Web.HttpUtility.UrlEncode(v.ToString())).ToArray();
        //    //return string.Format(str, paraObj);
        //}

        public static KeyValuePair<string, string> AppKeyVal { get; set; }

    }
}
