using System.CommandLine;
using System.Runtime.InteropServices;
namespace RagePhoto.Cli;

internal static class Program {

    private static void Main(String[] args) {
        RootCommand rootCommand = new("ragephoto-cli Application") {
            Commands.CreateCommand, Commands.GetCommand, Commands.SetCommand
        };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            rootCommand.Add(Commands.PathCommand);
        rootCommand.Parse(args).Invoke();
    }
}
