# Задача

На входе есть большой текстовый файл, где каждая строка имеет вид Number. String

Например:
```text
415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
```

   Обе части могут в пределах файла повторяться. Необходимо получить на выходе другой файл, где
   все строки отсортированы. Критерий сортировки: сначала сравнивается часть String, если она
   совпадает, тогда Number.
   Т.е. в примере выше должно получиться:
```text
1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
```

Требуется написать две программы:
1. Утилита для создания тестового файла заданного размера. Результатом работы должен быть
   текстовый файл описанного выше вида. Должно быть какое-то количество строк с одинаковой
   частью String.
2. Собственно сортировщик. Важный момент, файл может быть очень большой. Для тестирования
   будет использоваться размер ~100Gb.
   При оценке выполненного задания мы будем в первую очередь смотреть на результат
   (корректность генерации/сортировки и время работы), во вторую на то, как кандидат пишет код.
   Язык программирования: C#.

# Нефункциональные требования

Во время проверки мы обращаем внимание на быстродействие решения, качество написания кода и, в целом, время выполнения.

Как ориентир по времени - 10Гб файл сортируется около 9 минут (кстати, это не самое быстрое решение), а 1Гб файл сортируется в рамках минуты (самый быстрый результат - 26 секунд). Дополнительный ориентир - при сортировке 1Гб используется 2-2,5 Гб памяти.


# Детали реализации
- Максимальный размер памяти задается 
- Текст может быть разделен различными разделителями. Вынесено в настройки запуска.
- Реализация IFileParser для попытки найти самый быстрый парсер
- Есть проблема чтения последней строки (может не быть последнего разделителя). Не во всех алгоритмах реализована 

## Алгоритм
- Основной алгоритм итерационно зачитывает кусок больного файла в буфер заданного размера
- Указанный FileParser парсит буфер построчно и заносит в индекс {text,list<number>}
- Индекс сортируется по ключу (text), далее по number
- Сортированные данные пишутся в файл sorted_{iteration}.txt
- После записи все кусков алгоритм попарно берет файлы и мержит в следующий файл
- Мерж идет итеративно, пока не останется один файл

## Производительность
- Размеры файлов: 1Gb и 10Gb
- Железо: Intel Core i7, SSD

### 1Gb file

Result: 13s, MaxMemory: 1.5 Gb

```json
// StreamReaderParser, BufferFileParser, SpanFileParser,
"FileParser" : "BufferFileParser",

// Размер выделямого буффера в памяти. Max: 0X7FEFFFFF (чуть меньше 2Gb).
"BufferSize" : 1025,

// BYTES, MEGABYTES
"BufferSizeUnit" : "MEGABYTES",

// Использовать многопоточный парсинг и индексирование.
"UseMultithreading" : true,

// Количество потоков при использовании UseMultithreading.
"Threads" : 4,

// MergeIterative=true: Мержит сортированные промежуточные файлы попарно, пока не останется только один.
// MergeIterative=false: Мержит сортированные промежуточные файлы одновременно
"MergeIterative" : false,

// Удалять промежуточные файлы после мержа.
"DeleteTempFiles" : true
```

```text
C:\Users\petri\Documents\Projects\BigFileSort\BigFileSort\bin\Release\net6.0>BigFileSort.exe
BigFileSort
WorkingDirectory: C:\Users\petri\Documents\Projects\BigFileSort\BigFileSort\bin\output
Command: Sort
InputFileName: generated1.txt
FileParser: BufferFileParser
MemoryLimitInBytes: 1074790400
SortFile  Elapsed: 00:00:13.0401663
──SplitIteration 1  Elapsed: 00:00:13.0372610
────SplitAsync 1  Elapsed: 00:00:07.7983956
──────ReadAndParse 11 TotalLines = 11785014 Elapsed: 00:00:07.7380769
──────ReadAndParse 12 TotalLines = 11785065 Elapsed: 00:00:07.6728044
──────ReadAndParse 13 TotalLines = 11785010 Elapsed: 00:00:07.6655067
──────ReadAndParse 14 TotalLines = 11785033 Elapsed: 00:00:07.6144560
────Sort  Elapsed: 00:00:02.2222038
────WriteSorted sorted_1.txt  Elapsed: 00:00:02.5885641
Sorted in 00:00:13.0461397
```

### 10Gb file

Result: 4.32s, MaxMemory: 3.4 Gb

```json
// StreamReaderParser, BufferFileParser, SpanFileParser,
"FileParser" : "BufferFileParser",

// Размер выделямого буффера в памяти. Max: 0X7FEFFFFF (чуть меньше 2Gb).
"BufferSize" : 0X7FEFFFFF,

// BYTES, MEGABYTES
"BufferSizeUnit" : "BYTES",

// Использовать многопоточный парсинг и индексирование.
"UseMultithreading" : true,

// Количество потоков при использовании UseMultithreading.
"Threads" : 6,

// MergeIterative=true: Мержит сортированные промежуточные файлы попарно, пока не останется только один.
// MergeIterative=false: Мержит сортированные промежуточные файлы одновременно
"MergeIterative" : false,

// Удалять промежуточные файлы после мержа.
"DeleteTempFiles" : true
```

```text
C:\Users\petri\Documents\Projects\BigFileSort\BigFileSort\bin\Release\net6.0>BigFileSort.exe
BigFileSort
WorkingDirectory: C:\Users\petri\Documents\Projects\BigFileSort\BigFileSort\bin\output
Command: Sort
InputFileName: generated10.txt
FileParser: BufferFileParser
MemoryLimitInBytes: 2146435071
SortFile  Elapsed: 00:04:32.1622529
──SplitIteration 1  Elapsed: 00:00:27.4565665
────SplitAsync 1  Elapsed: 00:00:14.1504733
──────ReadAndParse 12 TotalLines = 15705591 Elapsed: 00:00:14.0524415
──────ReadAndParse 13 TotalLines = 15705650 Elapsed: 00:00:13.9192887
──────ReadAndParse 15 TotalLines = 15705513 Elapsed: 00:00:14.0166208
──────ReadAndParse 16 TotalLines = 15705578 Elapsed: 00:00:13.8562960
──────ReadAndParse 14 TotalLines = 15705697 Elapsed: 00:00:13.8968504
──────ReadAndParse 11 TotalLines = 15705623 Elapsed: 00:00:13.8964761
────Sort  Elapsed: 00:00:03.2674783
────WriteSorted sorted_1.txt  Elapsed: 00:00:07.4544682
──SplitIteration 2  Elapsed: 00:00:26.2026138
────SplitAsync 2  Elapsed: 00:00:12.3713862
──────ReadAndParse 21 TotalLines = 15705579 Elapsed: 00:00:12.3018748
──────ReadAndParse 22 TotalLines = 15705624 Elapsed: 00:00:12.2423734
──────ReadAndParse 23 TotalLines = 15705558 Elapsed: 00:00:12.1426639
──────ReadAndParse 24 TotalLines = 15705578 Elapsed: 00:00:12.2035847
──────ReadAndParse 25 TotalLines = 15705597 Elapsed: 00:00:12.1770804
──────ReadAndParse 26 TotalLines = 15705705 Elapsed: 00:00:12.1864998
────Sort  Elapsed: 00:00:03.2235308
────WriteSorted sorted_2.txt  Elapsed: 00:00:08.5284146
──SplitIteration 3  Elapsed: 00:00:25.0422285
────SplitAsync 3  Elapsed: 00:00:12.1876635
──────ReadAndParse 31 TotalLines = 15705597 Elapsed: 00:00:12.1759966
──────ReadAndParse 32 TotalLines = 15705757 Elapsed: 00:00:12.1046383
──────ReadAndParse 34 TotalLines = 15705437 Elapsed: 00:00:12.0711796
──────ReadAndParse 33 TotalLines = 15705596 Elapsed: 00:00:12.1221871
──────ReadAndParse 35 TotalLines = 15705554 Elapsed: 00:00:12.1144099
──────ReadAndParse 36 TotalLines = 15705580 Elapsed: 00:00:11.9911103
────Sort  Elapsed: 00:00:03.1929375
────WriteSorted sorted_3.txt  Elapsed: 00:00:07.5458845
──SplitIteration 4  Elapsed: 00:00:25.7827350
────SplitAsync 4  Elapsed: 00:00:12.5124601
──────ReadAndParse 41 TotalLines = 15705645 Elapsed: 00:00:12.4100501
──────ReadAndParse 42 TotalLines = 15705610 Elapsed: 00:00:12.2312399
──────ReadAndParse 43 TotalLines = 15705577 Elapsed: 00:00:12.3448404
──────ReadAndParse 44 TotalLines = 15705590 Elapsed: 00:00:12.2848729
──────ReadAndParse 45 TotalLines = 15705482 Elapsed: 00:00:12.2821292
──────ReadAndParse 46 TotalLines = 15705676 Elapsed: 00:00:12.2871404
────Sort  Elapsed: 00:00:03.1844334
────WriteSorted sorted_4.txt  Elapsed: 00:00:08.0584597
──SplitIteration 5  Elapsed: 00:00:25.3966393
────SplitAsync 5  Elapsed: 00:00:12.4073750
──────ReadAndParse 51 TotalLines = 15705631 Elapsed: 00:00:12.2769488
──────ReadAndParse 52 TotalLines = 15705675 Elapsed: 00:00:12.1893824
──────ReadAndParse 53 TotalLines = 15705636 Elapsed: 00:00:12.1725801
──────ReadAndParse 54 TotalLines = 15705523 Elapsed: 00:00:12.1954920
──────ReadAndParse 55 TotalLines = 15705649 Elapsed: 00:00:12.3182278
──────ReadAndParse 56 TotalLines = 15705506 Elapsed: 00:00:12.1267918
────Sort  Elapsed: 00:00:03.1898621
────WriteSorted sorted_5.txt  Elapsed: 00:00:07.7967698
──SplitIteration 6  Elapsed: 00:00:00.8300872
────ReadAndParse 6 TotalLines = 230162 Elapsed: 00:00:00.5279018
────Sort  Elapsed: 00:00:00.1310416
────WriteSorted sorted_6.txt  Elapsed: 00:00:00.0568526
──MergeFiles  Elapsed: 00:02:21.4460070
────MergeOutput: sorted_1+2+3+4+5+6.txt  Elapsed: 00:02:21.4448727
Sorted in 00:04:32.1701301
```

## What Next
- завернуть в web service
- конфигурирование (готово)
- Чтение данных из различных источников (файл, S3, etc)
- Возобновление процесса после падения, ошибки
- Распараллеливание работы на несколько workers
- Progress (частично есть)