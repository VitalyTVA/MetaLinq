using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using MetaLinq;
using MetaLinq.Internal;
using MetaLinqSpikes;
using System.Linq;

namespace MetaLinqBenchmark;

class Program {
    static void Main(string[] args) {
        BenchmarkRunner.Run<Select_MinNBenchmarks>();
        //BenchmarkRunner.Run<Select_MinBenchmarks>();
        //BenchmarkRunner.Run<Select_SumNBenchmarks>();
        //BenchmarkRunner.Run<Select_SumBenchmarks>();

        //new Where_LastBenchmarks { N = 10 }.Standard_();
        //BenchmarkRunner.Run<Where_LastOrDefaultBenchmarks>();
        //BenchmarkRunner.Run<OrderBy_SingleBenchmarks>();
        //BenchmarkRunner.Run<OrderBy_AggregateBenchmarks>();
        //BenchmarkRunner.Run<OrderBy_AnyBenchmarks>();
        //BenchmarkRunner.Run<OrderBy_FirstBenchmarks>();

        //BenchmarkRunner.Run<ListIterationsBenchmarks>();

        //BenchmarkRunner.Run<Where_ToDictionaryBenchmarks>();
        //BenchmarkRunner.Run<Select_ToDictionaryBenchmarks>();

        //BenchmarkRunner.Run<Where_ToHashSetBenchmarks>();
        //BenchmarkRunner.Run<Select_ToHashSetBenchmarks>();

        //BenchmarkRunner.Run<OrderBy_ThenBy_ThenByBenchmarks>();
        //BenchmarkRunner.Run<Array_Where_OrderBy_Select_Where_OrderByDescendingBenchmarks>();
        //BenchmarkRunner.Run<Where_OrderByBenchmarks>();
        //BenchmarkRunner.Run<Select_OrderByBenchmarks>();
        //BenchmarkRunner.Run<SelectBenchmarks>();
        //BenchmarkRunner.Run<SortBenchmarks>();
        //BenchmarkRunner.Run<OrderByBenchmarks>();
        //BenchmarkRunner.Run<Benchmarks>();
    }
}
public class TestData {
    public TestData(int[] ints, int value) {
        Ints = ints;
        Value = value;
    }
    public int[] Ints { get; }
    public int Value;
    public int Value2;
    public int Value3;
}
