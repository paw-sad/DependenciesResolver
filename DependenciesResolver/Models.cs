using System.Collections;
using System.Collections.Generic;

namespace DependenciesResolver
{
    public class Dependency
    {
        public string Name { get; set; }
        public string VersionRange { get; set; }
    }

    public class Package
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public List<Package> Dependencies { get; set; } = new List<Package>();
    }

    public class PackageVersion
    {
        public string Version { get; set; }
        public IEnumerable<Dependency> Dependencies { get; set; } = new List<Dependency>();
    }
}