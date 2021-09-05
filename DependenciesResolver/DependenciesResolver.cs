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
        private readonly INpmRepositoryClient _repositoryClient;
        private readonly ILogger _logger;

        public DependenciesResolver(INpmRepositoryClient repositoryClient)
        {
            _repositoryClient = repositoryClient;
        }

        public DependenciesResolver(INpmRepositoryClient repositoryClient, ILogger logger)
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

            var packageInfo = await GetPackageVersionsInfo(packageName);
            var packageVersionInfo = packageInfo.FirstOrDefault(x => x.Version == packageVersion);

            foreach (var dependency in packageVersionInfo.Dependencies)
            {
               startingPackage.Dependencies.Add(await BuildTree(dependency));
            }

            return startingPackage;
        }

        private async Task<Package> BuildTree(Dependency dependency)
        {
            _logger?.Log($"Building tree for a package: {dependency.Name} {dependency.VersionRange}");

            var dependencyInfo = await GetPackageVersionsInfo(dependency.Name);
            var packageInfo = GetMaxSatisfyingPackageVersionInfo(dependency, dependencyInfo);
            var package = new Package {Name = dependency.Name, Version = packageInfo.Version };

            if (packageInfo.Dependencies != null && packageInfo.Dependencies.Any())
            {
                foreach (var d in packageInfo.Dependencies)
                {
                    package.Dependencies.Add(await BuildTree(d));
                }
            }

            return package;
        }

        private static PackageVersion GetMaxSatisfyingPackageVersionInfo(Dependency dependency, IEnumerable<PackageVersion> dependencyInfo)
        {
            var range = new SemanticVersioning.Range(dependency.VersionRange);
            var versions = dependencyInfo.Select(x => x.Version).ToList();
            var maxSatisfying =  range.MaxSatisfying(versions);

            return dependencyInfo.FirstOrDefault(x => x.Version == maxSatisfying);
        }

        public async Task<IEnumerable<PackageVersion>> GetPackageVersionsInfo(string packageName)
        {
            var jsonString = await _repositoryClient.GetMetadataForPackage(packageName);
            var versionsJObject = JObject.Parse(jsonString)["versions"] as JObject;
            var packageVersions = versionsJObject.Properties()
                .Select(x => new PackageVersion
                {
                    Version = x.Name,
                    Dependencies = ((x.Value as JObject)["dependencies"] as JObject)?.Properties()
                    .Select(d => new Dependency
                    {
                        Name = d.Name,
                        VersionRange = d.Value.ToObject<string>()
                    })
                    .ToList()
                }).ToList();

            return packageVersions;
        }
    }
}