using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using DependenciesResolver.NpmRepositoryIntegration;
using Newtonsoft.Json;

namespace DependenciesResolver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Console.WriteLine("Give package name");
            //var packageName = Console.ReadLine();
            
            //Console.WriteLine("Give package version");
            //var packageVersion = Console.ReadLine();
            var packageName = "snyk";
            var packageVersion = "1.681.0";
            var dependenciesResolver =
                new DependenciesResolver(new PackageRepositoryFacade(new NpmRepositoryCacheClient(new NpmRepositoryClient(@"https://registry.npmjs.org", new HttpClient()))), new ConsoleLogger());
            var sw = new Stopwatch();
            
            sw.Start();
            var dependenciesTree = await dependenciesResolver.BuildDependenciesTree(packageName, packageVersion, 16);
            sw.Stop();

            Console.WriteLine(JsonConvert.SerializeObject(dependenciesTree, Formatting.Indented));
            Console.WriteLine($"Elapsed {sw.Elapsed}");
            Console.ReadLine();
        }
    }
}
