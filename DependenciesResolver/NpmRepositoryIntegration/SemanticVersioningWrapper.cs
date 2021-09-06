using System.Collections.Generic;
using System.Linq;

namespace DependenciesResolver.NpmRepositoryIntegration
{
    public class SemanticVersionWrapper
    {
        public static PackageVersionInfo GetMaxSatisfyingPackageVersionInfo(Dependency dependency, IEnumerable<PackageVersionInfo> dependencyInfo)
        {
            var versionRange = new SemanticVersioning.Range(dependency.VersionRange);
            var availableVersions = dependencyInfo.Select(x => x.Version).ToList();
            var maxSatisfyingVersion = versionRange.MaxSatisfying(availableVersions);

            return dependencyInfo.FirstOrDefault(x => x.Version == maxSatisfyingVersion);
        }
    }
}