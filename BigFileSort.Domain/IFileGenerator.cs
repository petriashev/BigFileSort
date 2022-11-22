﻿namespace BigFileSort.Domain;

public interface IFileGenerator
{
    void GenerateFile(Stream output, GenerateFileCommand command);
}

public record GenerateFileCommand
{
    public long TargetFileSize { get; init; }
}