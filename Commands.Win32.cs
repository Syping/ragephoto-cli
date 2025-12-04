using Microsoft.Win32;
using System.CommandLine;
using System.Runtime.Versioning;
namespace RagePhoto.Cli;

internal static partial class Commands {

    [SupportedOSPlatform("Windows")]
    internal static Int32 PathFunction(String command) {
        try {
            if (command == "register" || command == "unregister") {
                String appPath = Path.GetDirectoryName(Environment.ProcessPath) ??
                    throw new Exception("Application Path can not be found");
                String fullAppPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(appPath));
                using RegistryKey environmentKey = Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true) ??
                    throw new Exception("Environment Registry Key can not be opened");
                String? path = environmentKey.GetValue(
                    "Path", null, RegistryValueOptions.DoNotExpandEnvironmentNames) as String ??
                    throw new Exception("Path Registry Value is invalid");
                List<String> paths = [.. path.Split(';', StringSplitOptions.RemoveEmptyEntries)];
                for (Int32 i = 0; i < paths.Count; i++) {
                    if (!String.Equals(
                        fullAppPath,
                        Path.TrimEndingDirectorySeparator(Path.GetFullPath(paths[i])),
                        StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (command == "register")
                        return 0;
                    paths.RemoveAt(i);
                    environmentKey.SetValue("Path", String.Join(";", paths), RegistryValueKind.ExpandString);
                    return 0;
                }
                if (command == "unregister")
                    return 0;
                paths.Add(fullAppPath);
                environmentKey.SetValue("Path", String.Join(";", paths), RegistryValueKind.ExpandString);
                return 0;
            }
            throw new ArgumentException("Invalid Path Command");
        }
        catch (ArgumentException exception) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(exception.Message);
            Console.ResetColor();
            return 1;
        }
        catch (Exception exception) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(exception.Message);
            Console.ResetColor();
            return -1;
        }
    }

    [SupportedOSPlatform("Windows")]
    internal static Command PathCommand {
        get {
            Argument<String> commandArgument = new("command") {
                Description = "Path Command"
            };
            commandArgument.CompletionSources.Add(_ => [
                new("register"),
                new("unregister")]);
            commandArgument.Validators.Add(result => {
                String[] commands = ["register", "unregister"];
                String command = result.GetValueOrDefault<String>();
                if (!commands.Contains(command, StringComparer.InvariantCultureIgnoreCase))
                    result.AddError("Invalid Path Command.");
            });
            Command pathCommand = new("path", "Register/Unregister Path") {
                commandArgument
            };
            pathCommand.Hidden = true;
            pathCommand.SetAction(result => Environment.ExitCode = PathFunction(
                result.GetRequiredValue(commandArgument)));
            return pathCommand;
        }
    }
}
