# MetaLinq


## Release Notes

## Links
https://github.com/dotnet/runtime/tree/main/src/libraries/System.Linq/src/System/Linq
https://github.com/NetFabric/NetFabric.Hyperlinq

##OrderBy simple benchmark

|                 Method |     N |           Mean |        Error |      StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Allocated |
|----------------------- |------ |---------------:|-------------:|------------:|------:|--------:|--------:|-------:|----------:|
|       OrderBy_Standard |    10 |       275.7 ns |      3.95 ns |     2.61 ns |  1.00 |    0.00 |  0.0815 |      - |     512 B |
|    Sort_Map_Comparison |    10 |       142.7 ns |      1.81 ns |     1.20 ns |  0.52 |    0.01 |  0.0508 |      - |     320 B |
|      Sort_Map_Comparer |    10 |       160.4 ns |      3.03 ns |     2.01 ns |  0.58 |    0.01 |  0.0508 |      - |     320 B |
|   Sort_Direct_Comparer |    10 |       139.2 ns |      1.53 ns |     1.01 ns |  0.50 |    0.00 |  0.0267 |      - |     168 B |
| Sort_Direct_Comparison |    10 |       349.6 ns |      3.75 ns |     2.48 ns |  1.27 |    0.02 |  0.0405 |      - |     256 B |
|                        |       |                |              |             |       |         |         |        |           |
|       OrderBy_Standard |   100 |     3,642.0 ns |     34.49 ns |    22.82 ns |  1.00 |    0.00 |  0.4234 |      - |   2,672 B |
|    Sort_Map_Comparison |   100 |     1,993.0 ns |     45.24 ns |    29.92 ns |  0.55 |    0.01 |  0.2785 |      - |   1,760 B |
|      Sort_Map_Comparer |   100 |     2,349.8 ns |     42.83 ns |    28.33 ns |  0.65 |    0.01 |  0.2785 |      - |   1,760 B |
|   Sort_Direct_Comparer |   100 |     1,625.4 ns |     30.37 ns |    18.07 ns |  0.45 |    0.01 |  0.1984 |      - |   1,248 B |
| Sort_Direct_Comparison |   100 |     3,742.4 ns |     28.41 ns |    18.79 ns |  1.03 |    0.01 |  0.2098 |      - |   1,336 B |
|                        |       |                |              |             |       |         |         |        |           |
|       OrderBy_Standard |  1000 |    74,329.0 ns |    738.42 ns |   488.42 ns |  1.00 |    0.00 |  3.7842 | 0.1221 |  24,272 B |
|    Sort_Map_Comparison |  1000 |    31,499.9 ns |    279.20 ns |   166.15 ns |  0.42 |    0.00 |  2.5635 |      - |  16,160 B |
|      Sort_Map_Comparer |  1000 |    42,812.8 ns |    495.18 ns |   327.53 ns |  0.58 |    0.01 |  2.5635 |      - |  16,160 B |
|   Sort_Direct_Comparer |  1000 |    20,380.9 ns |    408.79 ns |   243.26 ns |  0.27 |    0.00 |  1.8921 |      - |  12,048 B |
| Sort_Direct_Comparison |  1000 |    64,892.7 ns |    646.67 ns |   427.73 ns |  0.87 |    0.01 |  1.8311 |      - |  12,136 B |
|                        |       |                |              |             |       |         |         |        |           |
|       OrderBy_Standard | 10000 | 1,111,047.8 ns | 13,935.83 ns | 8,292.99 ns |  1.00 |    0.00 | 37.1094 | 7.8125 | 240,273 B |
|    Sort_Map_Comparison | 10000 |   739,979.5 ns |  1,698.77 ns | 1,123.63 ns |  0.67 |    0.00 | 25.3906 | 2.9297 | 160,160 B |
|      Sort_Map_Comparer | 10000 |   803,316.6 ns |  3,508.01 ns | 2,087.56 ns |  0.72 |    0.01 | 25.3906 | 1.9531 | 160,160 B |
|   Sort_Direct_Comparer | 10000 |   551,019.4 ns |  5,837.84 ns | 3,861.37 ns |  0.50 |    0.01 | 18.5547 | 2.9297 | 120,048 B |
| Sort_Direct_Comparison | 10000 |   928,709.5 ns |  2,129.56 ns | 1,267.27 ns |  0.84 |    0.01 | 18.5547 | 1.9531 | 120,136 B |


|                         Method |     N |           Mean |        Error |      StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Allocated |
|------------------------------- |------ |---------------:|-------------:|------------:|------:|--------:|--------:|-------:|----------:|
|               OrderBy_Standard |    10 |       275.3 ns |      8.91 ns |     4.66 ns |  1.00 |    0.00 |  0.0815 |      - |     512 B |
|            Sort_Map_Comparison |    10 |       141.4 ns |      5.46 ns |     3.25 ns |  0.51 |    0.02 |  0.0508 |      - |     320 B |
| Sort_ArraySortHelper_TComparer |    10 |       105.3 ns |      4.01 ns |     2.65 ns |  0.38 |    0.01 |  0.0370 |      - |     232 B |
|                                |       |                |              |             |       |         |         |        |           |
|               OrderBy_Standard |   100 |     3,691.4 ns |     46.31 ns |    30.63 ns |  1.00 |    0.00 |  0.4234 |      - |   2,672 B |
|            Sort_Map_Comparison |   100 |     1,992.5 ns |     32.54 ns |    19.36 ns |  0.54 |    0.01 |  0.2785 |      - |   1,760 B |
| Sort_ArraySortHelper_TComparer |   100 |     1,618.3 ns |    612.10 ns |   404.87 ns |  0.44 |    0.11 |  0.2651 |      - |   1,672 B |
|                                |       |                |              |             |       |         |         |        |           |
|               OrderBy_Standard |  1000 |    73,839.2 ns |    579.79 ns |   345.03 ns |  1.00 |    0.00 |  3.7842 | 0.1221 |  24,272 B |
|            Sort_Map_Comparison |  1000 |    32,155.6 ns |    889.31 ns |   588.22 ns |  0.44 |    0.01 |  2.5635 |      - |  16,160 B |
| Sort_ArraySortHelper_TComparer |  1000 |    39,302.1 ns | 14,669.52 ns | 9,702.98 ns |  0.52 |    0.13 |  2.5330 |      - |  16,072 B |
|                                |       |                |              |             |       |         |         |        |           |
|               OrderBy_Standard | 10000 | 1,115,833.1 ns | 13,010.70 ns | 7,742.46 ns |  1.00 |    0.00 | 37.1094 | 7.8125 | 240,273 B |
|            Sort_Map_Comparison | 10000 |   749,904.3 ns |  9,388.20 ns | 5,586.76 ns |  0.67 |    0.01 | 25.3906 | 2.9297 | 160,160 B |
| Sort_ArraySortHelper_TComparer | 10000 |   607,952.1 ns |  5,145.62 ns | 3,403.51 ns |  0.55 |    0.01 | 24.4141 | 0.9766 | 160,072 B |