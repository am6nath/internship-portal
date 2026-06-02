using InternshipPortal.API.Services.Interfaces;

namespace InternshipPortal.API.Services.File
{
    public class FileUrlService : IFileUrlService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileUrlService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string ToPublicUrl(string? relativeOrAbsolutePath)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath))
            {
                return string.Empty;
            }

            if (relativeOrAbsolutePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                relativeOrAbsolutePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return relativeOrAbsolutePath;
            }

            var path = relativeOrAbsolutePath.StartsWith('/')
                ? relativeOrAbsolutePath
                : $"/{relativeOrAbsolutePath}";

            return $"{GetBaseUrl().TrimEnd('/')}{path}";
        }

        public string? ToStoredPath(string? publicUrl)
        {
            if (string.IsNullOrWhiteSpace(publicUrl))
            {
                return null;
            }

            if (!publicUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !publicUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return publicUrl.StartsWith('/') ? publicUrl : $"/{publicUrl}";
            }

            var baseUrl = GetBaseUrl().TrimEnd('/');
            if (publicUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
            {
                return publicUrl[baseUrl.Length..];
            }

            try
            {
                var uri = new Uri(publicUrl);
                return uri.AbsolutePath;
            }
            catch
            {
                return publicUrl;
            }
        }

        private string GetBaseUrl()
        {
            var configured = _configuration["ApiSettings:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured.TrimEnd('/');
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                return $"{request.Scheme}://{request.Host}";
            }

            return "http://localhost:5080";
        }
    }
}
