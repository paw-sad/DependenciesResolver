using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DependenciesResolver;
using Moq;
using Shouldly;
using Xunit;

namespace Tests
{
    public partial class Tests
    {
        [Fact]
        public async Task GetPackageInfo()
        {
            // arrange
            var packageName = "some-package";

            var npmRepoClientMock = new Mock<INpmRepositoryClient>();
            var responseContent = await File.ReadAllTextAsync("./NpmClientResponses/some-package.json");
            npmRepoClientMock.Setup(x => x.GetMetadataForPackage(packageName))
                .Returns(Task.FromResult(responseContent));
            var expectedVersions = new List<PackageVersion>
            {
                new PackageVersion
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
                new PackageVersion
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
                new PackageVersion
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
            var versions = await
                new DependenciesResolver.DependenciesResolver(npmRepoClientMock.Object).GetPackageVersionsInfo(
                    packageName);

            // assert
            versions.ShouldBeEquivalentTo(expectedVersions);
        }

        [Fact]
        public async Task BuildDependenciesTreeForGivePackage()
        {
            // arrange
            var packageName = "registry-url";
            var packageVersion = "3.0.3";

            var expectedDependenciesTree = new Package
            {
                Name = "registry-url",
                Version = "3.0.3",
                Dependencies = new List<Package>()
                {
                    new Package
                    {
                        Name = "rc",
                        Version = "1.2.8",
                        Dependencies = new List<Package>()
                        {
                            new Package
                            {
                                Name = "deep-extend",
                                Version = "0.6.0"
                            },
                            new Package
                            {
                                Name = "ini",
                                Version = "1.3.8"
                            },
                            new Package
                            {
                                Name = "minimist",
                                Version = "1.2.5"
                            },
                            new Package
                            {
                                Name = "strip-json-comments",
                                Version = "2.0.1"
                            },
                        }
                    }
                }
            };

            var dependenciesResolver = new DependenciesResolver.DependenciesResolver(new TestNpmRepositoryClient());

            // act 
            var dependencies = await dependenciesResolver.BuildDependenciesTree(packageName, packageVersion);


            // assert
            dependencies.ShouldBeEquivalentTo(expectedDependenciesTree);
        }
    }
}
