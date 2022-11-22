namespace BigFileSort.Domain;

public sealed class FileGenerator : IFileGenerator
{
    private long _targetFileSize = 1.GigabytesInBytes();
    private long _currentFileSize;
    
    /// <inheritdoc />
    public void GenerateFile(Stream output, GenerateFileCommand command)
    {
        using var streamWriter = new StreamWriter(output, bufferSize: 10.MegabytesInBytes());

        while (_currentFileSize <= command.TargetFileSize)
        {
            var textLine = GenerateLine();
            streamWriter.Write(textLine);
            _currentFileSize += textLine.Length;
        }
    }

    public string GenerateLine()
    {
        // Example: '415. Apple'
        var number = Random.Shared.Next(1, 999999);
        string text = $"String_{Random.Shared.Next(1, 999999)}";
        return $"{number}. {text}{Environment.NewLine}";
    }
}