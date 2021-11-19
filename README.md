# MetaLinq


## Release Notes

## Links

https://github.com/dotnet/runtime/tree/main/src/libraries/System.Linq/src/System/Linq

https://github.com/NetFabric/NetFabric.Hyperlinq

https://github.com/kevin-montrose/LinqAF

##Select benchmark

|           Method |    N |        Mean |      Error |    StdDev |      Median | Ratio | RatioSD |  Gen 0 |  Gen 1 | Allocated |
|----------------- |----- |------------:|-----------:|----------:|------------:|------:|--------:|-------:|-------:|----------:|
|  Select_Standard |   10 |    39.73 ns |   1.170 ns |  0.774 ns |    39.65 ns |  1.00 |    0.00 | 0.0178 |      - |     112 B |
|      Select_Meta |   10 |    20.03 ns |   1.036 ns |  0.616 ns |    19.72 ns |  0.51 |    0.02 | 0.0102 |      - |      64 B |
| AsFastAsPossible |   10 |    19.35 ns |   0.458 ns |  0.303 ns |    19.20 ns |  0.49 |    0.01 | 0.0102 |      - |      64 B |
|     Select_Hyper |   10 |    26.53 ns |   0.571 ns |  0.340 ns |    26.59 ns |  0.67 |    0.02 | 0.0102 |      - |      64 B |
|                  |      |             |            |           |             |       |         |        |        |           |
|  Select_Standard |  100 |   199.39 ns |  73.404 ns | 48.552 ns |   167.94 ns |  1.00 |    0.00 | 0.0751 |      - |     472 B |
|      Select_Meta |  100 |   191.56 ns |  81.708 ns | 54.045 ns |   158.97 ns |  0.96 |    0.11 | 0.0675 |      - |     424 B |
| AsFastAsPossible |  100 |   208.98 ns | 107.340 ns | 70.999 ns |   164.30 ns |  1.03 |    0.14 | 0.0675 |      - |     424 B |
|     Select_Hyper |  100 |   227.12 ns | 115.956 ns | 76.698 ns |   171.54 ns |  1.12 |    0.13 | 0.0675 |      - |     424 B |
|                  |      |             |            |           |             |       |         |        |        |           |
|  Select_Standard | 1000 | 1,358.67 ns |  15.777 ns |  8.252 ns | 1,357.85 ns |  1.00 |    0.00 | 0.6485 | 0.0095 |   4,072 B |
|      Select_Meta | 1000 | 1,479.61 ns |  49.587 ns | 32.799 ns | 1,481.94 ns |  1.08 |    0.02 | 0.6409 | 0.0095 |   4,024 B |
| AsFastAsPossible | 1000 | 1,534.86 ns |  16.050 ns |  9.551 ns | 1,530.69 ns |  1.13 |    0.01 | 0.6409 | 0.0095 |   4,024 B |
|     Select_Hyper | 1000 | 1,473.89 ns |  22.671 ns | 14.996 ns | 1,469.46 ns |  1.09 |    0.02 | 0.6390 | 0.0095 |   4,024 B |

##OrderBy simple benchmark

|           Method |     N |           Mean |        Error |       StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|----------------- |------ |---------------:|-------------:|-------------:|------:|--------:|---------:|---------:|---------:|----------:|
| OrderBy_Standard |    10 |       275.7 ns |      6.95 ns |      4.13 ns |  1.00 |    0.00 |   0.0815 |        - |        - |     512 B |
|     OrderBy_Meta |    10 |       118.1 ns |      1.82 ns |      1.20 ns |  0.43 |    0.01 |   0.0267 |        - |        - |     168 B |
|       OrderBy_AF |    10 |       658.9 ns |     10.46 ns |      6.92 ns |  2.39 |    0.05 |   0.1411 |        - |        - |     888 B |
|                  |       |                |              |              |       |         |          |          |          |           |
| OrderBy_Standard |   100 |     3,528.4 ns |     86.26 ns |     57.05 ns |  1.00 |    0.00 |   0.4234 |        - |        - |   2,672 B |
|     OrderBy_Meta |   100 |     1,313.1 ns |     54.53 ns |     36.07 ns |  0.37 |    0.01 |   0.1984 |        - |        - |   1,248 B |
|       OrderBy_AF |   100 |     7,129.3 ns |    155.73 ns |    103.01 ns |  2.02 |    0.04 |   1.1444 |   0.0076 |        - |   7,216 B |
|                  |       |                |              |              |       |         |          |          |          |           |
| OrderBy_Standard |  1000 |    72,179.5 ns |    299.44 ns |    178.19 ns |  1.00 |    0.00 |   3.7842 |   0.2441 |        - |  24,272 B |
|     OrderBy_Meta |  1000 |    27,234.0 ns |    530.00 ns |    315.39 ns |  0.38 |    0.00 |   1.8921 |   0.0610 |        - |  12,048 B |
|       OrderBy_AF |  1000 |   117,390.0 ns |  4,596.31 ns |  3,040.18 ns |  1.63 |    0.04 |   9.1553 |   0.4883 |        - |  58,000 B |
|                  |       |                |              |              |       |         |          |          |          |           |
| OrderBy_Standard | 10000 | 1,073,226.3 ns | 11,482.63 ns |  6,005.64 ns |  1.00 |    0.00 |  37.1094 |   9.7656 |        - | 240,273 B |
|     OrderBy_Meta | 10000 |   591,733.8 ns |  9,275.95 ns |  6,135.47 ns |  0.55 |    0.01 |  18.5547 |   3.9063 |        - | 120,049 B |
|       OrderBy_AF | 10000 | 1,663,347.5 ns | 29,620.06 ns | 19,591.83 ns |  1.55 |    0.02 | 132.8125 | 132.8125 | 132.8125 | 769,214 B |

##Sort Benchmark

|                         Method |     N |           Mean |        Error |      StdDev | Ratio |   Gen 0 |  Gen 1 | Allocated |
|------------------------------- |------ |---------------:|-------------:|------------:|------:|--------:|-------:|----------:|
|               OrderBy_Standard |    10 |       260.3 ns |      4.12 ns |     2.72 ns |  1.00 |  0.0815 |      - |     512 B |
|            Sort_Map_Comparison |    10 |       138.7 ns |      0.94 ns |     0.56 ns |  0.53 |  0.0508 |      - |     320 B |
| Sort_ArraySortHelper_TComparer |    10 |       103.7 ns |      1.89 ns |     1.25 ns |  0.40 |  0.0370 |      - |     232 B |
|         SortHelper_SortToArray |    10 |       114.7 ns |      1.61 ns |     0.96 ns |  0.44 |  0.0267 |      - |     168 B |
|                                |       |                |              |             |       |         |        |           |
|               OrderBy_Standard |   100 |     3,616.5 ns |     49.29 ns |    32.60 ns |  1.00 |  0.4234 |      - |   2,672 B |
|            Sort_Map_Comparison |   100 |     1,987.4 ns |     50.26 ns |    33.24 ns |  0.55 |  0.2785 |      - |   1,760 B |
| Sort_ArraySortHelper_TComparer |   100 |     1,325.2 ns |     21.74 ns |    14.38 ns |  0.37 |  0.2651 |      - |   1,672 B |
|         SortHelper_SortToArray |   100 |     1,309.6 ns |      4.67 ns |     3.09 ns |  0.36 |  0.1984 |      - |   1,248 B |
|                                |       |                |              |             |       |         |        |           |
|               OrderBy_Standard |  1000 |    74,054.5 ns |  1,085.86 ns |   718.23 ns |  1.00 |  3.7842 | 0.1221 |  24,272 B |
|            Sort_Map_Comparison |  1000 |    31,629.6 ns |    458.76 ns |   303.44 ns |  0.43 |  2.5635 |      - |  16,160 B |
| Sort_ArraySortHelper_TComparer |  1000 |    28,220.1 ns |    388.15 ns |   256.74 ns |  0.38 |  2.5330 |      - |  16,072 B |
|         SortHelper_SortToArray |  1000 |    21,761.2 ns |    479.44 ns |   285.31 ns |  0.29 |  1.8921 |      - |  12,048 B |
|                                |       |                |              |             |       |         |        |           |
|               OrderBy_Standard | 10000 | 1,104,199.0 ns | 11,887.10 ns | 7,862.58 ns |  1.00 | 37.1094 | 7.8125 | 240,274 B |
|            Sort_Map_Comparison | 10000 |   740,163.0 ns |  6,211.65 ns | 3,696.45 ns |  0.67 | 25.3906 | 2.9297 | 160,160 B |
| Sort_ArraySortHelper_TComparer | 10000 |   596,291.7 ns |  6,093.82 ns | 3,626.33 ns |  0.54 | 24.4141 | 0.9766 | 160,072 B |
|         SortHelper_SortToArray | 10000 |   596,021.6 ns | 11,859.12 ns | 7,844.08 ns |  0.54 | 18.5547 |      - | 120,049 B |