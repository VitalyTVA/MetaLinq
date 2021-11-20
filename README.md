# MetaLinq


## Release Notes

## Links

https://github.com/dotnet/runtime/tree/main/src/libraries/System.Linq/src/System/Linq

https://github.com/NetFabric/NetFabric.Hyperlinq

https://github.com/kevin-montrose/LinqAF

## Select benchmark

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

## OrderBy

|           Method |    N |        Mean |       Error |    StdDev | Ratio |  Gen 0 |  Gen 1 | Allocated |
|----------------- |----- |------------:|------------:|----------:|------:|-------:|-------:|----------:|
| OrderBy_Standard |   10 |    267.1 ns |     3.70 ns |   2.45 ns |  1.00 | 0.0815 |      - |     512 B |
|     OrderBy_Meta |   10 |    117.9 ns |     2.07 ns |   1.37 ns |  0.44 | 0.0267 |      - |     168 B |
|                  |      |             |             |           |       |        |        |           |
| OrderBy_Standard |  100 |  3,589.0 ns |    98.12 ns |  64.90 ns |  1.00 | 0.4234 |      - |   2,672 B |
|     OrderBy_Meta |  100 |  1,314.6 ns |    19.01 ns |  12.58 ns |  0.37 | 0.1984 |      - |   1,248 B |
|                  |      |             |             |           |       |        |        |           |
| OrderBy_Standard | 1000 | 74,772.8 ns | 1,212.70 ns | 802.13 ns |  1.00 | 3.7842 | 0.2441 |  24,272 B |
|     OrderBy_Meta | 1000 | 21,565.4 ns |   536.44 ns | 354.82 ns |  0.29 | 1.8921 | 0.0610 |  12,048 B |

## Select_OrderBy

|                 Method |    N |        Mean |       Error |      StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 | Allocated |
|----------------------- |----- |------------:|------------:|------------:|------:|--------:|-------:|-------:|----------:|
| SelectOrderBy_Standard |   10 |    210.2 ns |     4.95 ns |     3.27 ns |  1.00 |    0.00 | 0.0763 |      - |     480 B |
|     SelectOrderBy_Meta |   10 |    103.7 ns |     1.41 ns |     0.84 ns |  0.49 |    0.01 | 0.0305 |      - |     192 B |
|       SelectOrderBy_AF |   10 |    584.4 ns |    14.84 ns |     9.81 ns |  2.78 |    0.08 | 0.1307 |      - |     824 B |
|                        |      |             |             |             |       |         |        |        |           |
| SelectOrderBy_Standard |  100 |  3,572.3 ns |    45.38 ns |    27.01 ns |  1.00 |    0.00 | 0.3052 |      - |   1,920 B |
|     SelectOrderBy_Meta |  100 |  1,404.4 ns |    82.19 ns |    54.37 ns |  0.39 |    0.02 | 0.2022 |      - |   1,272 B |
|       SelectOrderBy_AF |  100 |  4,859.1 ns |   196.63 ns |   130.06 ns |  1.36 |    0.04 | 0.9460 |      - |   5,960 B |
|                        |      |             |             |             |       |         |        |        |           |
| SelectOrderBy_Standard | 1000 | 72,520.5 ns | 1,415.40 ns |   936.20 ns |  1.00 |    0.00 | 2.5635 |      - |  16,320 B |
|     SelectOrderBy_Meta | 1000 | 26,575.9 ns | 2,980.55 ns | 1,971.45 ns |  0.37 |    0.03 | 1.9226 | 0.0305 |  12,072 B |
|       SelectOrderBy_AF | 1000 | 71,090.0 ns | 1,391.03 ns |   920.08 ns |  0.98 |    0.02 | 7.2021 | 0.4883 |  45,544 B |

## Sort Benchmark

best ever:

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

current:

|                         Method |     N |           Mean |       Error |      StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Allocated |
|------------------------------- |------ |---------------:|------------:|------------:|------:|--------:|--------:|-------:|----------:|
|               OrderBy_Standard |    10 |       268.2 ns |     3.13 ns |     2.07 ns |  1.00 |    0.00 |  0.0815 |      - |     512 B |
|            Sort_Map_Comparison |    10 |       138.9 ns |     1.01 ns |     0.67 ns |  0.52 |    0.00 |  0.0508 |      - |     320 B |
| Sort_ArraySortHelper_TComparer |    10 |       104.2 ns |     1.83 ns |     1.21 ns |  0.39 |    0.01 |  0.0370 |      - |     232 B |
|         SortHelper_SortToArray |    10 |       129.5 ns |     6.26 ns |     4.14 ns |  0.48 |    0.02 |  0.0267 |      - |     168 B |
|                                |       |                |             |             |       |         |         |        |           |
|               OrderBy_Standard |   100 |     3,592.7 ns |    45.50 ns |    30.09 ns |  1.00 |    0.00 |  0.4234 |      - |   2,672 B |
|            Sort_Map_Comparison |   100 |     1,929.8 ns |    30.70 ns |    20.31 ns |  0.54 |    0.00 |  0.2785 |      - |   1,760 B |
| Sort_ArraySortHelper_TComparer |   100 |     1,325.6 ns |    19.44 ns |    11.57 ns |  0.37 |    0.00 |  0.2651 |      - |   1,672 B |
|         SortHelper_SortToArray |   100 |     1,423.3 ns |    41.83 ns |    21.88 ns |  0.40 |    0.01 |  0.1984 |      - |   1,248 B |
|                                |       |                |             |             |       |         |         |        |           |
|               OrderBy_Standard |  1000 |    75,468.6 ns |   898.19 ns |   534.50 ns |  1.00 |    0.00 |  3.7842 | 0.1221 |  24,272 B |
|            Sort_Map_Comparison |  1000 |    31,344.1 ns |   443.16 ns |   231.78 ns |  0.41 |    0.01 |  2.5635 |      - |  16,160 B |
| Sort_ArraySortHelper_TComparer |  1000 |    29,221.4 ns |   560.18 ns |   333.35 ns |  0.39 |    0.00 |  2.5330 |      - |  16,072 B |
|         SortHelper_SortToArray |  1000 |    22,630.7 ns |   599.60 ns |   396.60 ns |  0.30 |    0.01 |  1.8921 |      - |  12,048 B |
|                                |       |                |             |             |       |         |         |        |           |
|               OrderBy_Standard | 10000 | 1,107,315.8 ns | 9,423.15 ns | 5,607.56 ns |  1.00 |    0.00 | 37.1094 | 7.8125 | 240,273 B |
|            Sort_Map_Comparison | 10000 |   750,229.3 ns | 7,061.38 ns | 4,670.67 ns |  0.68 |    0.00 | 25.3906 | 2.9297 | 160,160 B |
| Sort_ArraySortHelper_TComparer | 10000 |   604,992.2 ns | 2,791.36 ns | 1,661.09 ns |  0.55 |    0.00 | 24.4141 | 0.9766 | 160,072 B |
|         SortHelper_SortToArray | 10000 |   600,363.3 ns | 7,074.10 ns | 4,679.08 ns |  0.54 |    0.00 | 18.5547 |      - | 120,049 B |