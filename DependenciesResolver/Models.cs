using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DependenciesResolver
{
    public class Dependency
    {
        public string Name { get; set; }
        public string VersionRange { get; set; }
    }

    public class DependencyResolverTask
    {
        public Dependency Dependency { get; set; }
        public DependencyTreeNode Parent { get; set; }
    }

    public class DependencyTreeNode
    {
        public string Name { get; set; }
        public string Version { get; set; }
        [JsonIgnore]
        public DependencyTreeNode Parent { get; set; }
        public ConcurrentBag<DependencyTreeNode> Dependencies { get; set; } = new ConcurrentBag<DependencyTreeNode>();
    }

    public class PackageVersionInfo
    {
        public string Version { get; set; }
        public IEnumerable<Dependency> Dependencies { get; set; } = new List<Dependency>();
    }
}