using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace DependenciesResolver
{
    public interface INpmRepositoryClient
    {
        Task<string> GetMetadataForPackage(string packageName);
    }

    public class NpmRepositoryClient : INpmRepositoryClient
    {
        private readonly string _repositoryUrl;
        private readonly HttpClient _httpClient;

        public NpmRepositoryClient(string repositoryUrl, HttpClient httpClient)
        {
            _repositoryUrl = repositoryUrl;
            _httpClient = httpClient;
        }

        public async Task<string> GetMetadataForPackage(string packageName)
        {
            var combinedUrl = $"{_repositoryUrl}/{HttpUtility.UrlEncode(packageName)}";

            var response = await _httpClient.GetAsync(combinedUrl);

            return await response.Content.ReadAsStringAsync();
        }
    }
}