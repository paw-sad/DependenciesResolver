using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DependenciesResolver.NpmRepositoryIntegration
{
    public class NpmRepositoryCacheClient : INpmRepositoryClient
    {
        private readonly INpmRepositoryClient _httpClient;
        private readonly Dictionary<string, string> _cacheObject = new Dictionary<string, string>();

        public NpmRepositoryCacheClient(INpmRepositoryClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetMetadataForPackage(string packageName)
        {
            if (_cacheObject.ContainsKey(packageName))
            {
                return _cacheObject[packageName];
            }

            var response = await _httpClient.GetMetadataForPackage(packageName);

            try
            {
                _cacheObject.Add(packageName, response);
            }
            catch (Exception e)
            {
            }

            return response;
        }
    }
}