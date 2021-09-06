using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DependenciesResolver;
using DependenciesResolver.NpmRepositoryIntegration;
using Shouldly;
using Xunit;

namespace Tests
{
    public partial class Tests
    {
        [Fact]
        public async Task CanParseNpmRepositoryJsonResponse()
        {
            // arrange
            var responseContent = await File.ReadAllTextAsync("./NpmClientResponses/some-package.json");
       
            var expectedVersions = new List<PackageVersionInfo>
            {
                new PackageVersionInfo
                {
                    Version = "0.0.0",
                    Dependencies = new List<Dependency>
                    {
                        new Dependency
                        {
                            Name = "tape",
                            VersionRange = "~1.0.4"
                        },
                        new Dependency
                        {
                            Name = "tap",
                            VersionRange = "~0.4.0"
                        }
                    }
                },
                new PackageVersionInfo
                {
                    Version = "0.2.0",
                    Dependencies = new List<Dependency>
                    {
                        new Dependency
                        {
                            Name = "tape",
                            VersionRange = "~1.0.4"
                        },
                        new Dependency
                        {
                            Name = "tap",
                            VersionRange = "~0.4.0"
                        }
                    }
                },
                new PackageVersionInfo
                {
                    Version = "1.0.0",
                    Dependencies = new List<Dependency>
                    {
                        new Dependency
                        {
                            Name = "tape",
                            VersionRange = "~1.0.4"
                        },
                        new Dependency
                        {
                            Name = "tap",
                            VersionRange = "~0.4.0"
                        }
                    }
                }
            };

            // act 
            var versions =
                NpmRepositoryJsonParser.GetPackageVersionsInfo(responseContent);

            // assert
            versions.ShouldBeEquivalentTo(expectedVersions);
        }

        [Fact]
        public async Task BuildDependenciesTreeForGivePackage()
        {
            // arrange
            var packageName = "registry-url";
            var packageVersion = "3.0.3";

            var expectedDependenciesTree = new DependencyTreeNode
            {
                Name = "registry-url",
                Version = "3.0.3",
            };
            var rcPackage = new DependencyTreeNode
            {
                Name = "rc",
                Version = "1.2.8",
                Parent = expectedDependenciesTree,
            };
            expectedDependenciesTree.Dependencies.Add(rcPackage);

            rcPackage.Dependencies.Add(
                new DependencyTreeNode
                {
                    Name = "deep-extend",
                    Version = "0.6.0",
                    Parent = rcPackage
                });
            rcPackage.Dependencies.Add(
                new DependencyTreeNode
                {
                    Name = "minimist",
                    Version = "1.2.5",
                    Parent = rcPackage
                });
            rcPackage.Dependencies.Add(
                new DependencyTreeNode
                {
                    Name = "ini",
                    Version = "1.3.8",
                    Parent = rcPackage
                });
            rcPackage.Dependencies.Add(
                new DependencyTreeNode
                {
                    Name = "strip-json-comments",
                    Version = "2.0.1",
                    Parent = rcPackage
                });

            var dependenciesResolver = new DependenciesResolver.DependenciesResolver(new PackageRepositoryFacade(new TestNpmRepositoryClient()));

            // act 
            var dependencies = await dependenciesResolver.BuildDependenciesTree(packageName, packageVersion, 2);

            // assert trees are equivalent (can differ in nodes order)
            dependencies.Name.ShouldBe(expectedDependenciesTree.Name);
            dependencies.Version.ShouldBe(expectedDependenciesTree.Version);
            dependencies.Dependencies.First().Name.ShouldBe(expectedDependenciesTree.Dependencies.First().Name);
            dependencies.Dependencies.First().Version.ShouldBe(expectedDependenciesTree.Dependencies.First().Version);
            dependencies.Dependencies.First().Dependencies.Select(x => new {x.Name, x.Version })
                .ShouldBeEquivalentTo(expectedDependenciesTree.Dependencies.First().Dependencies.Select(x => new { x.Name, x.Version }));
        }
    }
}
