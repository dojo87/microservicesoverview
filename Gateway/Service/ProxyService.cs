using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Service
{
    public class ProxyService
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly RequestDelegate nextMiddleware;
        private readonly IOptions<Proxies> proxies;


        public ProxyService(RequestDelegate nextMiddleware, IOptions<Proxies> proxies)
        {
            this.nextMiddleware = nextMiddleware;
            this.proxies = proxies;
        }

        public async Task Invoke(HttpContext context, IAuthService authService)
        {
            var targetUri = GetTargetUri(context.Request);

            if (targetUri != null)
            {
                await RequestTarget(context, targetUri, authService);
                return;
            }

            await nextMiddleware(context);
        }

        private async Task RequestTarget(HttpContext context, TargetUri targetUri, IAuthService authService)
        {
            try
            {
                var targetRequestMessage = CreateTargetMessage(context, targetUri.Target);

                string token = authService.Authenticate(context)?.Token;

                targetRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
                using (var responseMessage = await httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                {
                    context.Response.StatusCode = (int)responseMessage.StatusCode;
                    CopyResponseHeadersFromTarget(context, responseMessage);
                    await responseMessage.Content.CopyToAsync(context.Response.Body);
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes($"API '{targetUri.TargetName}' called by the URL had issues: {ex.Message}"));
            }
        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyRequest(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);

            return requestMessage;
        }

        private void CopyRequest(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
              !HttpMethods.IsHead(requestMethod) &&
              !HttpMethods.IsDelete(requestMethod) &&
              !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private void CopyResponseHeadersFromTarget(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            context.Response.Headers.Remove("transfer-encoding");
        }

        private static HttpMethod GetMethod(string method)
        {
            return new HttpMethod(method);
        }

        private TargetUri GetTargetUri(HttpRequest request)
        {
            TargetUri targetUri = null;

            if (request.Path.StartsWithSegments("/api", out var remainingPath))
            {
                
                var apiName = remainingPath.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                PathString apiCall = null;
                remainingPath.StartsWithSegments($"/{apiName}", out apiCall);
                var proxy = this.proxies.Value.List.FirstOrDefault(p => apiName.Equals(p.Api, StringComparison.InvariantCultureIgnoreCase));

                if (proxy != null)
                {
                    targetUri = new TargetUri
                    {
                        Target = new Uri(proxy.Url + apiCall.Value),
                        Original = request.Path,
                        TargetName = apiName
                    };
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Proxy discovered an gateway API call, but API {apiName} is not configured in the proxy config.");
                }
            }

            return targetUri;
        }

        private class TargetUri
        {
            public Uri Target {get;set;}
            public string Original { get; set; }
            public string TargetName { get; set; }

        }
    }
}

