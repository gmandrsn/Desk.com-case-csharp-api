using log4net;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace DeskApiClient
{
    public class DeskApiClient
    {
        private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private enum AuthenticationType
        {
            Basic,
            OAuth
        }
        /// <summary>
        /// returns an Oauth client with the default API values set
        /// </summary>
        public DeskApiClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            AuthType = AuthenticationType.OAuth;
        }

        /// <summary>
        /// Constructor to initialize the desk.com api client using an agent username and password 
        /// </summary>
        /// <param name="apiUrlBase">the desk.com api base url endpoint</param>
        /// <param name="username">the desk.com agent username</param>
        /// <param name="password">the desk.com agent password</param>
        public DeskApiClient(string apiUrlBase, string username, string password)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ApiUrlBase = apiUrlBase;

            UserName = username;
            Password = password;

            AuthType = AuthenticationType.Basic;
        }

        /// <summary>
        /// Constructor to initialize the desk.com api client using OAuth API credentials.
        /// These values can be found/set at: https://<YOUR_DESK_INSTANCE>.desk.com/admin/settings/api-applications
        /// </summary>
        /// <param name="apiUrlBase">the desk.com api base url endpoint</param>
        /// <param name="apiKey">the desk.com generated API Application Key</param>
        /// <param name="apiSecret">the desk.com generated API Application Secret</param>
        /// <param name="apiToken">the desk.com generated API Application Access Token</param>
        /// <param name="apiTokenSecret">the desk.com generated API Application Access Token Secret</param>
        public DeskApiClient(string apiUrlBase, string apiKey, string apiSecret, string apiToken, string apiTokenSecret)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            // In certain scenarios where your desk instance uses an SSL cert, you may need to uncomment the below line when you receive SSL
            // cert errors such as "The underlying connection was closed: Could not establish trust relationship for the SSL/ TLS secure channel."
            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ApiUrlBase = apiUrlBase;
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            ApiToken = apiToken;
            ApiTokenSecret = apiTokenSecret;

            AuthType = AuthenticationType.OAuth;
        }

        private int rateLimit = 0;
        private int rateLimitRemaining = 0;
        private int secsUntilRateLimitReset = 0;

        private AuthenticationType AuthType { get; set; }

        protected string UserName { get; private set; }

        protected string Password { get; private set; }

        protected string ApiKey { get; private set; }

        protected string ApiSecret { get; private set; }

        protected string ApiToken { get; private set; }

        protected string ApiTokenSecret { get; private set; } 

        protected string ApiUrlBase { get; set; } = "https://<YOUR_DESK_INSTANCE>.desk.com/api/v2";

        /// <summary>
        /// use me, the other methods are booty!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="jsonBody"></param>
        /// <returns></returns>
        public T Execute<T>(RestRequest request, string jsonBody) where T : new()
        {
            IRestResponse<T> response;

            var restClient = GetClient();

            request.AddHeader("Content-Type", "application/json");

            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            request.AddBody(jsonBody);
            response = restClient.Execute<T>(request);
            CheckBurstLimits(response.Headers.OfType<RestSharp.Parameter>().ToList<RestSharp.Parameter>());

            if ((int)response.StatusCode == 429)
            {
                // we sent to many requests, we need to sleep some secs
                // based on the value of the wait response header
                logger.Warn($"Hit desk.com api call rate limit, must wait {this.secsUntilRateLimitReset}");
                System.Threading.Thread.Sleep(this.secsUntilRateLimitReset * 1000);
                //after we sleep reprocess the call
                response = restClient.Execute<T>(request);
            }

            if ((int)response.StatusCode == 422)
            {
                //log the raw response
                logger.Error($"code: 422, desk.com failed to validate resource. request json: {jsonBody}");
                return default(T);

            }

            if (!(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK))

            {
                string msg = $"response error http status code: {response.StatusCode} status desc: {response.StatusDescription} content msg: {response.Content}";
                logger.Error(msg);
                if (response.ErrorException != null)
                {
                    string message = $"{msg} Check inner details for more info.";
                    throw new Exception(message, response.ErrorException);
                }
                throw new Exception(msg);
            }

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public IRestResponse Call(string resource, Method method)
        {
            var request = GetRequest(method, resource);
            return this.Call(request);
        }

        public IRestResponse Call(IRestRequest request)
        {
            var client = GetClient();
            var response = client.Execute(request);
            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var ex = new Exception(message, response.ErrorException);
                throw ex;
            }

            CheckBurstLimits(response.Headers.OfType<RestSharp.Parameter>().ToList<RestSharp.Parameter>());

            if ((int)response.StatusCode == 429)
            {
                // we sent to many requests, we need to sleep some secs
                // based on the value of the wait response header
                logger.Warn($"Hit desk.com api call rate limit, must wait {this.secsUntilRateLimitReset}");
                System.Threading.Thread.Sleep(this.secsUntilRateLimitReset * 1000);
                //after we sleep reprocess the call
                response = client.Execute(request);
            }

            return response;
        }

        private RestClient GetClient()
        {
            var client = new RestClient();

            switch (AuthType)
            {
                case AuthenticationType.Basic:
                    client.Authenticator = new HttpBasicAuthenticator(UserName, Password);
                    break;
                default:
                    client.Authenticator = OAuth1Authenticator.ForProtectedResource(ApiKey, ApiSecret, ApiToken, ApiTokenSecret);
                    break;
            }

            client.BaseUrl = new System.Uri(ApiUrlBase);

            return client;
        }

        private RestRequest GetRequest(Method method, string resource)
        {
            var request = new RestRequest();
            request.Method = method;
            request.Resource = resource;
            request.RequestFormat = DataFormat.Json;

            return request;
        }


        /// <summary>
        /// Return the current date/time in the ISO-8601 format required by the API
        /// </summary>
        /// <returns></returns>
        public string FormatDateForApi(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssK");
        }

        /// <summary>
        /// Parse a date from desk.com in the ISO-8601 format, returning a standard DateTime
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime ParseIso8601Date(string date)
        {
            DateTime result;
            if (DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
            {
                return result;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Retrieve the specified integer value from the supplied header collection. Returns 0 if value not found.
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private int GetIntHeaderValue(IList<RestSharp.Parameter> headers, string name)
        {
            int result = 0;
            if (headers != null)
            {
                var httpHeader = headers.Cast<RestSharp.Parameter>().FirstOrDefault(x => x.Name == name);
                int.TryParse(httpHeader.Value.ToString(), out result);
            }

            return result;
        }

        /// <summary>
        /// Check the rate limiting details returned in the response headers,
        /// track these values internally so that the caller can check whether we are near the limit.
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="extData"></param>
        private void CheckBurstLimits(IList<RestSharp.Parameter> headers)
        {
            int limit = GetIntHeaderValue(headers, "X-Rate-Limit-Limit");
            if (limit == 0) return; // no valid header information to check

            this.rateLimit = limit;
            this.rateLimitRemaining = GetIntHeaderValue(headers, "X-Rate-Limit-Remaining");
            this.secsUntilRateLimitReset = GetIntHeaderValue(headers, "X-Rate-Limit-Reset");


            if (IsExceedingApiLimits())
            {
                logger.Error($"Exceeded desk.com API limits: calls remaining {this.rateLimitRemaining}/{this.rateLimit}, {this.secsUntilRateLimitReset} seconds remaining until reset.");
            }
            else if (IsNearingApiLimits())
            {
                logger.Warn($"Nearing desk.com API limits: calls remaining {this.rateLimitRemaining}/{this.rateLimit}, {this.secsUntilRateLimitReset} seconds remaining until reset.");
            }
        }

        /// <summary>
        /// Return true if we have exceeded our desk.com API limits, as of the last call made by this object.
        /// If we have no data available, make a simple API call to load first.
        /// </summary>
        /// <returns>true if desk.com limits have been exceeded</returns>
        public bool IsExceedingApiLimits()
        {
            if (this.rateLimit == 0)
            {
                this.Call("groups", Method.GET);
            }

            if (this.rateLimit > 0 && this.rateLimitRemaining <= 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return true if we are nearing our desk.com API limits, as of the last call made by this object.
        /// If we have no data available, make a simple API call to load first.
        /// </summary>
        /// <param name="remainingPercentageThreshold">optionally specify the percentage we're concerned with</param>
        /// <returns>true if our threshold has been exceeded</returns>
        public bool IsNearingApiLimits(int remainingPercentageThreshold = -1)
        {
            if (this.rateLimit == 0)
            {
                this.Call("groups", Method.GET);
            }

            int remainingThreshold = remainingPercentageThreshold > 0 ? remainingPercentageThreshold : 85;
            int burstRatePercent = (this.rateLimitRemaining * 100) / this.rateLimit;

            if (this.rateLimit > 0 && burstRatePercent <= remainingThreshold)
            {
                return true;
            }
            return false;
        }
    }
}
