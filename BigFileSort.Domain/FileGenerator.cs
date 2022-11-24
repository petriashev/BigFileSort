namespace BigFileSort.Domain;

public sealed class FileGenerator : IFileGenerator
{
    private long _currentFileSize;
    
    /// <inheritdoc />
    public void GenerateFile(Stream output, GenerateFileCommand command)
    {
        using var streamWriter = new StreamWriter(output, bufferSize: 10.MegabytesInBytes());

        var targetFileSize = command.TargetFileSize - 100;
        while (_currentFileSize <= targetFileSize)
        {
            GenerateLine(out string number, out string text);
            streamWriter.Write(number);
            streamWriter.Write(". ");
            streamWriter.Write(text);
            streamWriter.Write(Environment.NewLine);
            
            _currentFileSize += number.Length + text.Length + 2 + Environment.NewLine.Length;
        }
    }

    private void GenerateLine(out string number, out string text)
    {
        // Example: '415. Apple'
        number = Random.Shared.Next(1, 999999).ToString();
        text = $"String_{Random.Shared.Next(1, 999999)}";
    }
}