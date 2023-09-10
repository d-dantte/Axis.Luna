// See https://aka.ms/new-console-template for more information
using Axis.Luna.Extensions.Benchmark;
using BenchmarkDotNet.Running;
using Axis.Luna.Extensions;

Console.WriteLine("Hello, World!");


//_ = BenchmarkRunner.Run<Axis.Luna.Extensions.Benchmark.Enumerable>();
_ = BenchmarkRunner.Run<Axis.Luna.Extensions.Benchmark.TypeExtension>();