using System.CommandLine;
namespace RagePhoto.Cli;

internal static class Program {

    private static void Main(String[] args) {
        RootCommand rootCommand = new("ragephoto-cli Application") {
            Commands.CreateCommand, Commands.GetCommand, Commands.SetCommand
        };
        rootCommand.Parse(args).Invoke();
    }
}
