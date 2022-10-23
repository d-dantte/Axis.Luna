// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;

Console.WriteLine("Hello, World!");


_ = BenchmarkRunner.Run<Axis.Luna.Extensions.Benchmark.Enumerable>();