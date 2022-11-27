namespace BigFileSort.Generation;

public interface IFileGenerator
{
    void GenerateFile(Stream output, GenerateFileCommand command);
}

public record GenerateFileCommand
{
    public long TargetFileSize { get; init; }
}