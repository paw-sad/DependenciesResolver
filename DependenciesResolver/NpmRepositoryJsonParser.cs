using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DependenciesResolver
{
    public class NpmRepositoryJsonParser 
    {
        public static IEnumerable<PackageVersionInfo> GetPackageVersionsInfo(string jsonString)
        {
            var versionsJObject = JObject.Parse(jsonString)["versions"] as JObject;
            var packageVersions = versionsJObject.Properties()
                .Select(x => new PackageVersionInfo
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