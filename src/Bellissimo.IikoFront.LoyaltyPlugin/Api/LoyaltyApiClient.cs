using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;
using Newtonsoft.Json;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Api
{
    public sealed class LoyaltyApiClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly PluginLogger logger;

        public LoyaltyApiClient(PluginSettings settings, PluginLogger logger)
        {
            this.logger = logger;
            httpClient = new HttpClient { BaseAddress = new Uri(settings.ApiBaseUrl), Timeout = settings.HttpTimeout };
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.BasicAuthLogin + ":" + settings.BasicAuthPassword));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

        public Task<LookupResponse> LookupAsync(LookupRequest request) => PostAsync<LookupRequest, LookupResponse>("/api/pos/incentives/lookup", request);
        public Task<PreviewResponse> PreviewAsync(PreviewRequest request) => PostAsync<PreviewRequest, PreviewResponse>("/api/pos/incentives/preview", request);
        public Task<ApplyResponse> ApplyAsync(ApplyRequest request) => PostAsync<ApplyRequest, ApplyResponse>("/api/pos/incentives/apply", request);
        public Task<CancelResponse> CancelAsync(CancelRequest request) => PostAsync<CancelRequest, CancelResponse>("/api/pos/incentives/cancel", request);

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var response = await httpClient.PostAsync(path, new StringContent(json, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new LoyaltyApiException("LOYALTY_SERVICE_UNAVAILABLE", $"HTTP {(int)response.StatusCode}: {body}");
                return JsonConvert.DeserializeObject<TResponse>(body);
            }
            catch (LoyaltyApiException) { throw; }
            catch (Exception ex)
            {
                logger.Error("Loyalty API network failure", ex);
                throw new LoyaltyApiException("LOYALTY_SERVICE_UNAVAILABLE", "Сервис лояльности недоступен");
            }
        }

        public void Dispose() => httpClient.Dispose();
    }
}
