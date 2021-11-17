# MetaLinq


## Release Notes

## Links
https://github.com/dotnet/runtime/tree/main/src/libraries/System.Linq/src/System/Linq
https://github.com/NetFabric/NetFabric.Hyperlinq

##OrderBy simple benchmark
|              Method |     N |            Mean |         Error |       StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|-------------------- |------ |----------------:|--------------:|-------------:|------:|--------:|---------:|---------:|---------:|----------:|
|    OrderBy_Standard |    10 |       261.81 ns |      3.637 ns |     2.164 ns |  1.00 |    0.00 |   0.0815 |        - |        - |     512 B |
|        OrderBy_Meta |    10 |        86.35 ns |      0.757 ns |     0.500 ns |  0.33 |    0.00 |   0.0267 |        - |        - |     168 B |
| OrderBy_MetaWithMap |    10 |        80.68 ns |      1.947 ns |     1.019 ns |  0.31 |    0.00 |   0.0509 |        - |        - |     320 B |
|          OrderBy_AF |    10 |       646.91 ns |      7.469 ns |     4.445 ns |  2.47 |    0.03 |   0.1411 |        - |        - |     888 B |
|                     |       |                 |               |              |       |         |          |          |          |           |
|    OrderBy_Standard |   100 |     3,499.90 ns |     54.437 ns |    36.007 ns |  1.00 |    0.00 |   0.4234 |        - |        - |   2,672 B |
|        OrderBy_Meta |   100 |     1,516.04 ns |     63.760 ns |    42.173 ns |  0.43 |    0.01 |   0.1984 |        - |        - |   1,248 B |
| OrderBy_MetaWithMap |   100 |     1,140.14 ns |      7.119 ns |     4.709 ns |  0.33 |    0.00 |   0.2804 |        - |        - |   1,760 B |
|          OrderBy_AF |   100 |     7,043.06 ns |     88.583 ns |    52.715 ns |  2.01 |    0.02 |   1.1444 |   0.0076 |        - |   7,216 B |
|                     |       |                 |               |              |       |         |          |          |          |           |
|    OrderBy_Standard |  1000 |    71,865.46 ns |    418.254 ns |   248.896 ns |  1.00 |    0.00 |   3.7842 |   0.2441 |        - |  24,272 B |
|        OrderBy_Meta |  1000 |    28,523.10 ns |    473.511 ns |   281.779 ns |  0.40 |    0.00 |   1.8921 |   0.0305 |        - |  12,048 B |
| OrderBy_MetaWithMap |  1000 |    15,919.58 ns |    119.822 ns |    79.255 ns |  0.22 |    0.00 |   2.5635 |   0.0916 |        - |  16,160 B |
|          OrderBy_AF |  1000 |   115,148.82 ns |    683.070 ns |   406.484 ns |  1.60 |    0.01 |   9.1553 |   0.4883 |        - |  58,000 B |
|                     |       |                 |               |              |       |         |          |          |          |           |
|    OrderBy_Standard | 10000 | 1,072,501.00 ns |  3,514.060 ns | 2,091.161 ns |  1.00 |    0.00 |  37.1094 |   9.7656 |        - | 240,273 B |
|        OrderBy_Meta | 10000 |   652,991.27 ns |  5,929.334 ns | 3,921.887 ns |  0.61 |    0.00 |  18.5547 |   5.8594 |        - | 120,048 B |
| OrderBy_MetaWithMap | 10000 |   211,171.88 ns |  1,719.419 ns | 1,137.289 ns |  0.20 |    0.00 |  25.3906 |   6.3477 |        - | 160,160 B |
|          OrderBy_AF | 10000 | 1,651,090.10 ns | 14,015.850 ns | 9,270.617 ns |  1.54 |    0.01 | 132.8125 | 132.8125 | 132.8125 | 769,215 B |