using System.Threading.Tasks;

namespace DependenciesResolver.NpmRepositoryIntegration
{
    public class PackageRepositoryFacade
    {
        private readonly INpmRepositoryClient _repositoryClient;

        public PackageRepositoryFacade(INpmRepositoryClient repositoryClient)
        {
            _repositoryClient = repositoryClient;
        }

        public async Task<PackageVersionInfo> PackageVersionInfo(Dependency dependency)
        {
            var jsonString = await _repositoryClient.GetMetadataForPackage(dependency.Name);
            var dependencyInfo = NpmRepositoryJsonParser.GetPackageVersionsInfo(jsonString);
            var packageInfo = SemanticVersionWrapper.GetMaxSatisfyingPackageVersionInfo(dependency, dependencyInfo);
            return packageInfo;
        }
    }
}