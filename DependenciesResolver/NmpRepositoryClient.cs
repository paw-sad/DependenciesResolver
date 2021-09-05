using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace DependenciesResolver
{
    public interface INmpRepositoryClient
    {
        Task<string> GetMetadataForPackage(string packageName, string packageVersion);
        Task<string> GetMetadataForPackage(string packageName);
    }

    public class NmpRepositoryClient : INmpRepositoryClient
    {
        private readonly string _repositoryUrl;
        private readonly HttpClient _httpClient;

        public NmpRepositoryClient(string repositoryUrl, HttpClient httpClient)
        {
            _repositoryUrl = repositoryUrl;
            _httpClient = httpClient;
        }

        public async Task<string> GetMetadataForPackage(string packageName, string packageVersion)
        {
            var combinedUrl = $"{_repositoryUrl}/{HttpUtility.UrlEncode(packageName)}/{HttpUtility.UrlEncode(packageVersion)}";

            var response = await _httpClient.GetAsync(combinedUrl);

            return await response.Content.ReadAsStringAsync();
        }


        public async Task<string> GetMetadataForPackage(string packageName)
        {
            var combinedUrl = $"{_repositoryUrl}/{HttpUtility.UrlEncode(packageName)}";

            var response = await _httpClient.GetAsync(combinedUrl);

            return await response.Content.ReadAsStringAsync();
        }
    }
}