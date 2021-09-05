using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DependenciesResolver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Give package name");
            var packageName = Console.ReadLine();
            
            Console.WriteLine("Give package version");
            var packageVersion = Console.ReadLine();

            var dependenciesResolver =
                new DependenciesResolver(new NpmRepositoryClient(@"https://registry.npmjs.org", new HttpClient()), new ConsoleLogger());

            var dependenciesTree = await dependenciesResolver.BuildDependenciesTree(packageName, packageVersion);

            Console.WriteLine(JsonConvert.SerializeObject(dependenciesTree, Formatting.Indented));
            Console.ReadLine();
        }
    }
}
