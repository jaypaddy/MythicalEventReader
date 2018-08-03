using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;


namespace EventReader
{
    class WebClient
    {
        private String bearerToken;
        private String[] httpErrorCodes = {"400", "401", "403","404","405","406","409", "410", "411",
                                            "412", "413", "415", "416", "422", "429", "500", "501", "503",
                                            "504", "507", "509"};
        private String LastHttpStatusCode;
        private Boolean LastHttpCallStatus=false;

        public Boolean GetLastHttpStatus()
        {
            return LastHttpCallStatus;
        }

        public String GetLastHttpStatusCode()
        {
            return LastHttpStatusCode;
        }
        public WebClient(String bToken)
        {
            bearerToken = bToken;
        }

        public async Task<string> DEL(string endPoint)
        {
            string retMsg;

            LastHttpCallStatus = true;
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.bearerToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, endPoint);
                HttpResponseMessage response = await client.SendAsync(request);
                retMsg = await response.Content.ReadAsStringAsync();
                retMsg = await response.Content.ReadAsStringAsync();

                if (httpErrorCodes.Contains<string>(response.StatusCode.ToString()))
                {
                    LastHttpStatusCode = response.StatusCode.ToString();
                    LastHttpCallStatus = false;
                    throw new Exception(retMsg);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retMsg;
        }

        public async Task<string> GET(string endPoint)
        {
            string retMsg;
            LastHttpCallStatus = true;
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.bearerToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endPoint);
                HttpResponseMessage response = await client.SendAsync(request);
                retMsg = await response.Content.ReadAsStringAsync();
                if (httpErrorCodes.Contains<string>(response.StatusCode.ToString()))
                {
                    LastHttpStatusCode = response.StatusCode.ToString();
                    LastHttpCallStatus = false;
                    throw new Exception(retMsg);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retMsg;
        }

        public async Task<string> POST(string endPoint, StringContent payload)
        {
            string retMsg;

            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.bearerToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.PostAsync(endPoint, payload);
                retMsg = await response.Content.ReadAsStringAsync();
                retMsg = await response.Content.ReadAsStringAsync();
                if (httpErrorCodes.Contains<string>(response.StatusCode.ToString()))
                    throw new Exception(retMsg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retMsg;
        }

        public async Task<string> POST_CST(string endPoint, StringContent payload)
        {
            string retMsg;

            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.bearerToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Prefer", "outlook.timezone=\"Central Standard Time\"");
                // Send the `POST subscriptions` request and parse the response.
                HttpResponseMessage response = await client.PostAsync(endPoint, payload);
                retMsg = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                retMsg = ex.ToString();
            }
            return retMsg;
        }
    }

  
}
