using Microsoft.Extensions.Configuration;
using System.IO;

namespace Xunit.Isolation;

internal class IsolationConfig
{
    private static IsolationConfig? _instance;
    public static IsolationConfig Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            var instance = new IsolationConfig();
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("isolation.json")
                .Build()
                .Bind(instance);

            _instance = instance;
            return instance;
        }
    }

    public string[] IsolationAssemblies { get; set; } = [];
}
