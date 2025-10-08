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
            if (_instance == null)
            {
                var instance = new IsolationConfig();
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("isolation.json", optional: true)
                    .Build();

                config.Bind(instance);
                _instance = instance;
            }

            return _instance;
        }
    }

    public string[] IsolationAssemblies { get; set; } = [];
}
