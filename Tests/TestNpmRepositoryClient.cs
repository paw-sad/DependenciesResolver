using System.IO;
using System.Threading.Tasks;
using DependenciesResolver;

namespace Tests
{
    public partial class Tests
    {
        public class TestNpmRepositoryClient : INmpRepositoryClient
        {
            public  async Task<string> GetMetadataForPackage(string packageName, string packageVersion)
            {
                return await File.ReadAllTextAsync($"./NpmClientResponses/{packageName}@{packageVersion}.json");
            }

            public async Task<string> GetMetadataForPackage(string packageName)
            {
                return await File.ReadAllTextAsync($"./NpmClientResponses/{packageName}.json");
            }
        }
    }
}
