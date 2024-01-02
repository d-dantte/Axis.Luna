// See https://aka.ms/new-console-template for more information


using Axis.Luna.Unions.Benchmarks;
using BenchmarkDotNet.Running;

_ = BenchmarkRunner.Run<UnionTypeMetadataBenchmark>();
