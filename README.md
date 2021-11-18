# MetaLinq


## Release Notes

## Links
https://github.com/dotnet/runtime/tree/main/src/libraries/System.Linq/src/System/Linq
https://github.com/NetFabric/NetFabric.Hyperlinq

##OrderBy simple benchmark
|                                 Method |     N |           Mean |        Error |      StdDev | Ratio |   Gen 0 |  Gen 1 | Allocated |
|--------------------------------------- |------ |---------------:|-------------:|------------:|------:|--------:|-------:|----------:|
|                       OrderBy_Standard |    10 |       262.6 ns |      4.55 ns |     3.01 ns |  1.00 |  0.0815 |      - |     512 B |
|                    Sort_Map_Comparison |    10 |       142.1 ns |      1.52 ns |     0.91 ns |  0.54 |  0.0508 |      - |     320 B |
|         Sort_ArraySortHelper_TComparer |    10 |       103.7 ns |      1.70 ns |     1.12 ns |  0.40 |  0.0370 |      - |     232 B |
| Sort_ArraySortHelper_TComparer_WithMap |    10 |       114.7 ns |      1.13 ns |     0.67 ns |  0.44 |  0.0267 |      - |     168 B |
|                                        |       |                |              |             |       |         |        |           |
|                       OrderBy_Standard |   100 |     3,605.5 ns |     66.69 ns |    34.88 ns |  1.00 |  0.4234 |      - |   2,672 B |
|                    Sort_Map_Comparison |   100 |     1,896.4 ns |     21.78 ns |    14.41 ns |  0.53 |  0.2785 |      - |   1,760 B |
|         Sort_ArraySortHelper_TComparer |   100 |     1,329.6 ns |     21.87 ns |    14.47 ns |  0.37 |  0.2651 |      - |   1,672 B |
| Sort_ArraySortHelper_TComparer_WithMap |   100 |     1,309.0 ns |     34.21 ns |    22.63 ns |  0.36 |  0.1984 |      - |   1,248 B |
|                                        |       |                |              |             |       |         |        |           |
|                       OrderBy_Standard |  1000 |    73,889.9 ns |  1,147.24 ns |   600.03 ns |  1.00 |  3.7842 | 0.1221 |  24,272 B |
|                    Sort_Map_Comparison |  1000 |    31,153.6 ns |    197.79 ns |   130.83 ns |  0.42 |  2.5635 |      - |  16,160 B |
|         Sort_ArraySortHelper_TComparer |  1000 |    28,602.4 ns |    592.89 ns |   392.16 ns |  0.39 |  2.5330 |      - |  16,072 B |
| Sort_ArraySortHelper_TComparer_WithMap |  1000 |    27,365.9 ns |    505.33 ns |   334.24 ns |  0.37 |  1.8921 |      - |  12,048 B |
|                                        |       |                |              |             |       |         |        |           |
|                       OrderBy_Standard | 10000 | 1,079,450.5 ns | 12,099.05 ns | 8,002.77 ns |  1.00 | 37.1094 | 7.8125 | 240,273 B |
|                    Sort_Map_Comparison | 10000 |   747,002.5 ns | 10,098.86 ns | 6,679.77 ns |  0.69 | 25.3906 | 2.9297 | 160,160 B |
|         Sort_ArraySortHelper_TComparer | 10000 |   588,320.4 ns |  2,203.54 ns | 1,311.29 ns |  0.54 | 24.4141 | 0.9766 | 160,072 B |
| Sort_ArraySortHelper_TComparer_WithMap | 10000 |   595,108.3 ns |  7,566.95 ns | 5,005.07 ns |  0.55 | 18.5547 |      - | 120,049 B |