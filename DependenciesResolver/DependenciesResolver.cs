using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DependenciesResolver.NpmRepositoryIntegration;

namespace DependenciesResolver
{
    public class DependenciesResolver
    {
        private readonly ILogger _logger;

        private DependencyTreeNode _treeRoot;
        private readonly PackageRepositoryFacade _packageRepositoryFacade;

        private readonly BlockingCollection<DependencyResolverTask> _dependenciesQueue = new BlockingCollection<DependencyResolverTask>();
        private int _pendingTasks = 0;

        public DependenciesResolver(PackageRepositoryFacade packageRepositoryFacade)
        {
            _packageRepositoryFacade = packageRepositoryFacade;
        }

        public DependenciesResolver(PackageRepositoryFacade packageRepositoryFacade, ILogger logger)
        {
            _logger = logger;
            _packageRepositoryFacade = packageRepositoryFacade;
        }

        public async Task<DependencyTreeNode> BuildDependenciesTree(string packageName, string packageVersion, int concurrencyLevel)
        {
            EnqueueTask(new DependencyResolverTask
            {
                Parent = null,
                Dependency = new Dependency
                {
                    Name = packageName,
                    VersionRange = packageVersion
                }
            });

            var workerTasks = new List<Task>();

            for (int i = 0; i < concurrencyLevel; i++)
            {
                workerTasks.Add(CreateWorker(i + 1));
            }

            await Task.WhenAll(workerTasks.ToArray());

            return _treeRoot;
        }

        private async Task BuildTreeNode(Dependency dependency, DependencyTreeNode parent)
        {
            _logger?.Log($"Building tree for a package: {dependency.Name} {dependency.VersionRange}");

            var packageInfo = await _packageRepositoryFacade.PackageVersionInfo(dependency);
            var package = new DependencyTreeNode { Name = dependency.Name, Version = packageInfo.Version, Parent = parent };

            if (parent == null)
                _treeRoot = package;
            else
                parent.Dependencies.Add(package);

            if (packageInfo.Dependencies != null && packageInfo.Dependencies.Any())
            {
                foreach (var d in packageInfo.Dependencies)
                {
                    var task = new DependencyResolverTask
                    {
                        Parent = package,
                        Dependency = d
                    };
                    EnqueueTask(task);
                }
            }
        }

        private async Task CreateWorker(int id)
        {
            await Task.Run(async () =>
            {
                while (!_dependenciesQueue.IsCompleted)
                {
                    await TryConsumeTask(id);

                    if (_pendingTasks == 0)
                    {
                        _dependenciesQueue.CompleteAdding();
                    }
                }
            });
        }

        private void EnqueueTask(DependencyResolverTask task)
        {
            _pendingTasks++;
            _dependenciesQueue.Add(task);
        }

        private async Task TryConsumeTask(int consumerId)
        {
            DependencyResolverTask data = null;
            try // when the queue is completed by other consumer we can get an exception here
            {
                data = _dependenciesQueue.Take();
            }
            catch (InvalidOperationException) { }

            if (data != null)
            {
                _logger?.Log($"Consumer {consumerId} processing dependency: {data.Dependency.Name}");

                await BuildTreeNode(data.Dependency, data.Parent);
                _pendingTasks--;

                _logger?.Log(_pendingTasks.ToString());
            }
        }
    }
}