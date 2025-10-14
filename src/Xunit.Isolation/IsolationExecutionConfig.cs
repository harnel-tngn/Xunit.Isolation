using System.IO;
using Microsoft.Extensions.Configuration;

namespace Xunit.Isolation;

/// <summary>
/// Class for configure execution of Xunit.Isolation.
/// </summary>
internal class IsolationExecutionConfig
{
    private static IsolationExecutionConfig? _instance;
    public static IsolationExecutionConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                var instance = new IsolationExecutionConfig();
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
