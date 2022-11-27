using BigFileSort;
using Newtonsoft.Json;

Console.WriteLine("BigFileSort");

var configuration = LoadConfiguration();

var app = new BigFileSortApp(configuration);

switch (configuration.Command)
{
    case FileCommand.Generate:
        app.GenerateFile();
        return;
    case FileCommand.Sort:
        app.Sort();
        return;
}

static BigFileSortConfiguration LoadConfiguration(string configFileName = "appsettings.json")
{
    var configurationText = File.ReadAllText(configFileName);
    var configuration = JsonConvert.DeserializeObject<BigFileSortConfiguration>(configurationText)
                                   ?? throw new ArgumentException($"Config {configFileName} should be deserialized to {nameof(BigFileSortConfiguration)} type.");

    if (configuration.WorkingDirectory != null)
    {
        configuration = configuration with
        {
            WorkingDirectory = Path.GetFullPath(configuration.WorkingDirectory)
        };
        Directory.CreateDirectory(configuration.WorkingDirectory);
        Directory.SetCurrentDirectory(configuration.WorkingDirectory);
        Console.WriteLine($"WorkingDirectory: {configuration.WorkingDirectory}");
    }
    
    Console.WriteLine($"Command: {configuration.Command}");

    return configuration;
}
