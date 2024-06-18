using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PiHire.BAL.Common.Http
{
    public class HttpApi : HttpClient
    {
        public HttpApi(string address)
        {
            base.BaseAddress = new Uri(address);
            base.DefaultRequestHeaders.Accept.Clear();
            base.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public HttpApi(string address, string token)
        {
            base.BaseAddress = new Uri(address);
            base.DefaultRequestHeaders.Accept.Clear();
            base.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            base.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        }
        public HttpApi(string address, string token,string od, string od1, string od2)
        {
            base.BaseAddress = new Uri(address);
            base.DefaultRequestHeaders.Accept.Clear();
            base.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            base.DefaultRequestHeaders.Add("access_token", token);
        }        
        public HttpApi(string address, string accept, string token)
        {
            base.BaseAddress = new Uri(address);
            base.DefaultRequestHeaders.Accept.Clear();
            base.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
            base.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        }
        public HttpApi(string address, string odDb, string odUsername, string odPassword)
        {
            base.BaseAddress = new Uri(address);
            base.DefaultRequestHeaders.Accept.Clear();
            base.DefaultRequestHeaders.Add("db", odDb);
            base.DefaultRequestHeaders.Add("login", odUsername);
            base.DefaultRequestHeaders.Add("password", odPassword);
            base.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }

    public class HttpClientService : IDisposable
    {
        public HttpResponseMessage Put<T>(string baseAddress, string url, T data)
        {
            using (HttpApi client = new HttpApi(baseAddress))
            {
                var dataStr = JsonConvert.SerializeObject(data);
                var httpContent = new StringContent(dataStr, Encoding.UTF8, "application/json");
                var response = client.PutAsync(url, httpContent).Result;
                return response;
            }
        }

        public HttpResponseMessage Put<T>(string baseAddress, string url, string token, T data)
        {
            using (HttpApi client = new HttpApi(baseAddress, token))
            {
                var dataStr = JsonConvert.SerializeObject(data);
                var httpContent = new StringContent(dataStr, Encoding.UTF8, "application/json");
                var response = client.PutAsync(url, httpContent).Result;
                return response;
            }
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string baseAddress, string url, T data)
        {
            using (HttpApi client = new HttpApi(baseAddress))
            {
                var dataStr = JsonConvert.SerializeObject(data);
                var httpContent = new StringContent(dataStr, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, httpContent);
                return response;
            }
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string baseAddress, string url, string token, T data)
        {
            using (HttpApi client = new HttpApi(baseAddress, token))
            {
                var dataStr = JsonConvert.SerializeObject(data);
                var httpContent = new StringContent(dataStr, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, httpContent);
                return response;
            }
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string baseAddress, string url, string token, T data, string Odoo)
        {
            using (HttpApi client = new HttpApi(baseAddress, token, string.Empty,string.Empty,string.Empty))
            {
                var dataStr = JsonConvert.SerializeObject(data);
                var httpContent = new StringContent(dataStr, Encoding.UTF8, "text/plain");
                var response = await client.PostAsync(url, httpContent);
                return response;
            }
        }

        public HttpResponseMessage Get(string baseAddress, string url, string odDb, string odUsername, string odPassword)
        {
            using (HttpApi client = new HttpApi(baseAddress, odDb, odUsername, odPassword))
            {
                var response = client.GetAsync(url).Result;
                return response;
            }
        }

        public HttpResponseMessage Get(string baseAddress, string url)
        {
            using (HttpApi client = new HttpApi(baseAddress))
            {
                var response = client.GetAsync(url).Result;
                return response;
            }
        }

        public HttpResponseMessage Get(string baseAddress, string url, string token)
        {
            using (HttpApi client = new HttpApi(baseAddress, token))
            {
                var response = client.GetAsync(url).Result;
                return response;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string baseAddress, string url)
        {
            using (HttpApi client = new HttpApi(baseAddress))
            {
                var response = await client.DeleteAsync(url);
                return response;
            }
        }


        public async Task<HttpResponseMessage> DeleteAsync(string baseAddress, string url, string token)
        {
            using (HttpApi client = new HttpApi(baseAddress, token))
            {
                var response = await client.DeleteAsync(url);
                return response;
            }
        }

        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
            }

            // Free any unmanaged objects here.
            disposed = true;
        }

        ~HttpClientService()
        {
            Dispose(false);
        }
    }
}
