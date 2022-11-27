namespace BigFileSort.Sorting;

/// <summary>
/// Merge файлов.
/// </summary>
public interface IFileMerger
{
    /// <summary>
    /// Merge файлов построчно с сортировкой.
    /// </summary>
    void MergeFiles(FileName fileName1, FileName fileName2, FileName outputFileName);
    void MergeFiles(FileName[] files, FileName outputFile);
}