using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SemanticVersioning;

namespace DependenciesResolver
{
    public class DependenciesResolver
    {
        private readonly INmpRepositoryClient _repositoryClient;
        private readonly ILogger _logger;

        public DependenciesResolver(INmpRepositoryClient repositoryClient)
        {
            _repositoryClient = repositoryClient;
        }

        public DependenciesResolver(INmpRepositoryClient repositoryClient, ILogger logger)
        {
            _repositoryClient = repositoryClient;
            _logger = logger;
        }


        public async Task<Package> BuildDependenciesTree(string packageName, string packageVersion)
        {
            var startingPackage = new Package
            {
                Name = packageName,
                Version = packageVersion
            };

            await BuildTree(startingPackage);

            return startingPackage;
        }

        private async Task BuildTree(Package package)
        {
            _logger?.Log($"Building tree for a package: {package.Name} {package.Version}");

            var packageDependencies = await GetDependencies(package.Name, package.Version);
            foreach (var dependency in packageDependencies)
            {
                var highestVersion = await GetHighestVersionOfPackageThatFulfillsVersionString(dependency.Name, dependency.VersionRange);
                var dep = new Package { Name = dependency.Name, Version = highestVersion };

                await BuildTree(dep);
                package.Dependencies.Add(dep);
            }
        }

        public async Task<IEnumerable<Dependency>> GetDependencies(string packageName, string packageVersion)
        {
            var jsonString = await _repositoryClient.GetMetadataForPackage(packageName, packageVersion);
            var dependenciesJObject = JObject.Parse(jsonString)["dependencies"] as JObject;
            if (dependenciesJObject == null)
            {
                return new Dependency[0];
            }

            var dependencies = dependenciesJObject.Properties()
                .Select(x => new Dependency
                {
                    Name = x.Name,
                    VersionRange = x.Value.ToObject<string>()
                }).ToList();

            return dependencies;
        }

        public async Task<string> GetHighestVersionOfPackageThatFulfillsVersionString(string packageName, string versionString)
        {
            var jsonString = await _repositoryClient.GetMetadataForPackage(packageName);
            var versionsJObject = JObject.Parse(jsonString)["versions"] as JObject;
            var packageVersions = versionsJObject.Properties()
                .Select(x => x.Name).ToList();
            
            var range = new SemanticVersioning.Range(versionString);
            return range.MaxSatisfying(packageVersions);
        }
    }

}