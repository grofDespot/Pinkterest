using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Pinkterest.Benchmarks.Support;

public sealed class BenchmarkHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Production;

    public string ApplicationName { get; set; } = "Pinkterest.Benchmarks";

    public string ContentRootPath { get; set; } = Path.GetTempPath();

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
