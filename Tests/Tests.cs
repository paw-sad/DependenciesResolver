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
        public async Task GetDependenciesForSpecificPackageVersion()
        {
            // arrange
            var packageName = "registry-url";
            var packageVersion = "5.0.0";

            var npmRepoClientMock = new Mock<INmpRepositoryClient>();
            var responseContent = await File.ReadAllTextAsync("./NpmClientResponses/registry-url@5.0.0.json");
            npmRepoClientMock.Setup(x => x.GetMetadataForPackage(packageName, packageVersion))
                .Returns(Task.FromResult(responseContent));

            // act 
            var dependencies = await
                new DependenciesResolver.DependenciesResolver(npmRepoClientMock.Object).GetDependencies(packageName, packageVersion);

            // assert
            dependencies.ShouldBeEquivalentTo(new List<Dependency>
            {
                new Dependency
                {
                    Name = "rc",
                    VersionRange = "^1.2.8"
                }
            });
        }

        [Fact]
        public async Task GetHighestVersionOfPackageThatFulfillsVersionString()
        {
            // arrange
            var packageName = "rc";
            var packageVersion = "^1.0.1";

            var npmRepoClientMock = new Mock<INmpRepositoryClient>();
            var responseContent = await File.ReadAllTextAsync("./NpmClientResponses/rc.json");
            npmRepoClientMock.Setup(x => x.GetMetadataForPackage(packageName))
                .Returns(Task.FromResult(responseContent));
            
            // act 
            var version = await
                new DependenciesResolver.DependenciesResolver(npmRepoClientMock.Object).GetHighestVersionOfPackageThatFulfillsVersionString(packageName, packageVersion);

            // assert
            version.ShouldBe("1.2.8");
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
