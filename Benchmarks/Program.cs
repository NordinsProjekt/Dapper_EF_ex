using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("?????????????????????????????????????????????????????????????????????");
        Console.WriteLine("?       SQL String Benchmarks: const vs regular strings            ?");
        Console.WriteLine("?                                                                   ?");
        Console.WriteLine("?  This benchmark suite tests the performance and memory           ?");
        Console.WriteLine("?  characteristics of using const vs regular strings for           ?");
        Console.WriteLine("?  SQL queries in Dapper repositories.                             ?");
        Console.WriteLine("?????????????????????????????????????????????????????????????????????");
        Console.WriteLine();
        Console.WriteLine("Select benchmark suite to run:");
        Console.WriteLine("1. SQL String Benchmarks (const vs field vs local)");
        Console.WriteLine("2. String Memory Benchmarks (allocation patterns)");
        Console.WriteLine("3. Dapper Repository Benchmarks (real-world scenarios)");
        Console.WriteLine("4. Run All Benchmarks");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
        Console.Write("Enter choice (0-4): ");
        
        var choice = Console.ReadLine();
        
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        switch (choice)
        {
            case "1":
                Console.WriteLine("\n?? Running SQL String Benchmarks...\n");
                BenchmarkRunner.Run<SqlStringBenchmarks>(config);
                break;
            case "2":
                Console.WriteLine("\n?? Running String Memory Benchmarks...\n");
                BenchmarkRunner.Run<StringMemoryBenchmarks>(config);
                break;
            case "3":
                Console.WriteLine("\n?? Running Dapper Repository Benchmarks...\n");
                BenchmarkRunner.Run<DapperRepositoryBenchmarks>(config);
                break;
            case "4":
                Console.WriteLine("\n?? Running All Benchmarks...\n");
                BenchmarkRunner.Run<SqlStringBenchmarks>(config);
                BenchmarkRunner.Run<StringMemoryBenchmarks>(config);
                BenchmarkRunner.Run<DapperRepositoryBenchmarks>(config);
                break;
            case "0":
                Console.WriteLine("Goodbye!");
                return;
            default:
                Console.WriteLine("Invalid choice. Running Dapper Repository Benchmarks (most relevant)...");
                BenchmarkRunner.Run<DapperRepositoryBenchmarks>(config);
                break;
        }

        Console.WriteLine("\n?????????????????????????????????????????????????????????????????????");
        Console.WriteLine("?                     Benchmark Complete!                          ?");
        Console.WriteLine("?                                                                   ?");
        Console.WriteLine("?  Results are saved in: BenchmarkDotNet.Artifacts/results/        ?");
        Console.WriteLine("?????????????????????????????????????????????????????????????????????");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
