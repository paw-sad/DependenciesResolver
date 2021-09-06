using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DependenciesResolver
{
    public class DependenciesResolver
    {
        private readonly INpmRepositoryClient _repositoryClient;
        private readonly ILogger _logger;
        private readonly BlockingCollection<DependencyResolverTask> _dependenciesQueue = new BlockingCollection<DependencyResolverTask>();
        private int _resolvedTasks = 0;

        public DependenciesResolver(INpmRepositoryClient repositoryClient)
        {
            _repositoryClient = repositoryClient;
        }

        public DependenciesResolver(INpmRepositoryClient repositoryClient, ILogger logger)
        {
            _repositoryClient = repositoryClient;
            _logger = logger;
        }

        public async Task<DependencyTreeNode> BuildDependenciesTree(string packageName, string packageVersion, int concurrencyLevel)
        {
            var startingDependencyTreeNode = new DependencyTreeNode
            {
                Name = packageName,
                Version = packageVersion
            };

            var packageInfo = await GetPackageVersionsInfo(packageName);
            var packageVersionInfo = packageInfo.FirstOrDefault(x => x.Version == packageVersion);

            foreach (var dependency in packageVersionInfo.Dependencies)
            {
                _resolvedTasks++;
                _dependenciesQueue.Add(new DependencyResolverTask
                {
                    Parent = startingDependencyTreeNode,
                    Dependency = dependency
                });
            }

            var tasks = new List<Task>();

            for (int i = 0; i < concurrencyLevel; i++)
            {
                tasks.Add(CreateWorker(i + 1));
            }

            Task.WaitAll(tasks.ToArray());

            return startingDependencyTreeNode;
        }

        private async Task BuildTree(Dependency dependency, DependencyTreeNode parent)
        {
            _logger?.Log($"Building tree for a package: {dependency.Name} {dependency.VersionRange}");

            var dependencyInfo = await GetPackageVersionsInfo(dependency.Name);
            var packageInfo = SemanticVersioningWrapper.GetMaxSatisfyingPackageVersionInfo(dependency, dependencyInfo);
            var package = new DependencyTreeNode { Name = dependency.Name, Version = packageInfo.Version, Parent = parent };

            parent.Dependencies.Add(package);

            if (packageInfo.Dependencies != null && packageInfo.Dependencies.Any())
            {
                foreach (var d in packageInfo.Dependencies)
                {
                    _resolvedTasks++;
                    _dependenciesQueue.Add(new DependencyResolverTask
                    {
                        Parent = package,
                        Dependency = d
                    });
                }
            }
        }

        public async Task<IEnumerable<PackageVersionInfo>> GetPackageVersionsInfo(string packageName)
        {
            var jsonString = await _repositoryClient.GetMetadataForPackage(packageName);
            return NpmRepositoryJsonParser.GetPackageVersionsInfo(jsonString);
        }

        private async Task CreateWorker(int id)
        {
            await Task.Run(async () =>
            {
                while (!_dependenciesQueue.IsCompleted)
                {
                    DependencyResolverTask data = null;
                    try // when the queue is completed by other consumer we can get an exception here
                    {
                        data = _dependenciesQueue.Take();
                    }
                    catch (InvalidOperationException) { }

                    if (data != null)
                    {
                        _logger?.Log($"Consumer {id} processing dependency: {data.Dependency.Name}");
                        await BuildTree(data.Dependency, data.Parent);

                        _resolvedTasks--;

                        _logger?.Log(_resolvedTasks.ToString());

                        if (_resolvedTasks == 0)
                        {
                            _dependenciesQueue.CompleteAdding();
                        }
                    }
                }
            });
        }
    }
}