using Microsoft.AspNetCore.Http;

namespace NewsSite.Services
{
    public interface IApiConfigurationService
    {
        string GetApiBaseUrl();
        string GetApiUrl(string endpoint);
    }

    public class ApiConfigurationService : IApiConfigurationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public ApiConfigurationService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public string GetApiBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                // Fallback to configuration if no HttpContext
                return _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7128";
            }

            var host = request.Host.Host;
            var isLocalhost = host == "localhost" || host == "127.0.0.1";

            if (isLocalhost)
            {
                var port = _configuration["ApiSettings:LocalPort"] ?? "7128";
                return $"https://localhost:{port}";
            }
            else
            {
                // Production URL - matches the JavaScript configuration
                var productionUrl = _configuration["ApiSettings:ProductionUrl"] ?? "https://proj.ruppin.ac.il/cgroup4/test2/tar1";
                return productionUrl;
            }
        }

        public string GetApiUrl(string endpoint)
        {
            var baseUrl = GetApiBaseUrl();
            
            // Ensure endpoint starts with slash for absolute path
            var cleanEndpoint = endpoint.StartsWith('/') ? endpoint : $"/{endpoint}";
            
            return $"{baseUrl}{cleanEndpoint}";
        }
    }
}
