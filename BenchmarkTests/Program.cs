using BenchmarkDotNet.Running;

namespace BenchmarkTests;

internal class Program
{
    static void Main(string[] args)
    {
        var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
