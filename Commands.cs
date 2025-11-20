using Microsoft.Win32;
using System.CommandLine;
using System.Runtime.InteropServices;
using System.Text;
namespace RagePhoto.Cli;

internal static class Commands {

    internal static Int32 CreateFunction(String format, String? jpegFile, String? description, String? json, String? title, String outputFile) {
        try {
            using Photo photo = new();

            photo.Format = format.ToLowerInvariant() switch {
                "gta5" => PhotoFormat.GTA5,
                "rdr2" => PhotoFormat.RDR2,
                _ => throw new ArgumentException("Invalid format", nameof(format))
            };

            if (photo.Format == PhotoFormat.GTA5) {
                photo.SetHeader("PHOTO - 10/26/25 02:28:08", 798615001, 0);
            }
            else {
                photo.SetHeader("PHOTO - 10/26/25 02:31:34", 3077307752, 2901366738);
            }

            if (String.IsNullOrEmpty(jpegFile)) {
                photo.Jpeg = Properties.Resources.EmptyJpeg;
            }
            else {
                using MemoryStream jpegStream = new();
                using Stream input = jpegFile == "-" ? Console.OpenStandardInput() : File.OpenRead(jpegFile);
                input.CopyTo(jpegStream);
                photo.Jpeg = jpegStream.ToArray();
            }

            if (photo.Format == PhotoFormat.GTA5) {
                photo.Json = json == null ?
                    Json.Initialize(PhotoFormat.GTA5, photo, Random.Shared.Next(),
                    DateTimeOffset.FromUnixTimeSeconds(Random.Shared.Next(1356998400, 1388534399))) :
                    Json.UpdateSign(photo, photo.Json);
            }
            else {
                photo.Json = json == null ?
                    Json.Initialize(PhotoFormat.RDR2, photo, Random.Shared.Next(),
                    DateTimeOffset.FromUnixTimeSeconds(Random.Shared.NextInt64(-2240524800, -2208988801))) :
                    Json.UpdateSign(photo, photo.Json);
            }

            photo.Description = description ?? String.Empty;
            photo.Title = title ?? "Custom Photo";

            if (outputFile == "-" || outputFile == String.Empty) {
                using MemoryStream photoStream = new(photo.Save());
                using Stream output = Console.OpenStandardOutput();
                photoStream.CopyTo(output);
            }
            else {
                String tempFile = Path.GetTempFileName();
                photo.SaveFile(tempFile);
                File.Move(tempFile, outputFile, true);
            }
            return 0;
        }
        catch (RagePhotoException exception) {
            Console.Error.WriteLine(exception.Message);
            return exception.Photo != null ? (Int32)exception.Error + 2 : -1;
        }
        catch (ArgumentException exception) {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
        catch (Exception exception) {
            Console.Error.WriteLine(exception.Message);
            return -1;
        }
    }

    internal static Int32 GetFunction(String inputFile, String dataType, String outputFile) {
        try {
            using Photo photo = new();

            if (inputFile == "-" || inputFile == String.Empty) {
                using MemoryStream photoStream = new();
                using Stream input = Console.OpenStandardInput();
                input.CopyTo(photoStream);
                photo.Load(photoStream.ToArray());
            }
            else {
                photo.LoadFile(inputFile);
            }

            Byte[] content = [];
            switch (dataType.ToLowerInvariant()) {
                case "d":
                case "description":
                    content = Encoding.UTF8.GetBytes($"{photo.Description}\n");
                    break;
                case "f":
                case "format":
                    content = Encoding.UTF8.GetBytes($"{photo.Format switch {
                        PhotoFormat.GTA5 => "gta5",
                        PhotoFormat.RDR2 => "rdr2",
                        _ => "unknown"
                    }}\n");
                    break;
                case "i":
                case "image":
                case "jpeg":
                    content = photo.Jpeg;
                    break;
                case "j":
                case "json":
                    content = Encoding.UTF8.GetBytes($"{photo.Json}\n");
                    break;
                case "s":
                case "sign":
                    content = Encoding.UTF8.GetBytes($"{photo.Sign}\n");
                    break;
                case "t":
                case "title":
                    content = Encoding.UTF8.GetBytes($"{photo.Title}\n");
                    break;
                default:
                    Console.Error.WriteLine($"Unknown Content Type: {dataType}");
                    return 1;
            }

            if (outputFile == "-" || outputFile == String.Empty) {
                using MemoryStream contentStream = new(content);
                using Stream output = Console.OpenStandardOutput();
                contentStream.CopyTo(output);
            }
            else {
                String tempFile = Path.GetTempFileName();
                using (MemoryStream contentStream = new(content)) {
                    using FileStream output = File.Create(tempFile);
                    contentStream.CopyTo(output);
                }
                File.Move(tempFile, outputFile, true);
            }
            return 0;
        }
        catch (RagePhotoException exception) {
            Console.Error.WriteLine(exception.Message);
            return exception.Photo != null ? (Int32)exception.Error + 2 : -1;
        }
        catch (Exception exception) {
            Console.Error.WriteLine(exception.Message);
            return -1;
        }
    }

    internal static Int32 SetFunction(String inputFile, String? format, String? jpegFile, String? description, String? json, String? title, bool updateSign, String? outputFile) {
        if (format == null && jpegFile == null && description == null
            && json == null && title == null && !updateSign) {
            Console.Error.WriteLine("No value has being set");
            return 1;
        }
        else if (inputFile == "-" && jpegFile == "-") {
            Console.Error.WriteLine("Multiple pipes are not supported");
            return 1;
        }

        try {
            using Photo photo = new();

            if (inputFile == "-" || inputFile == String.Empty) {
                using MemoryStream photoStream = new();
                using Stream input = Console.OpenStandardInput();
                input.CopyTo(photoStream);
                photo.Load(photoStream.ToArray());
            }
            else {
                photo.LoadFile(inputFile);
            }

            if (format != null) {
                photo.Format = format.ToLowerInvariant() switch {
                    "gta5" => PhotoFormat.GTA5,
                    "rdr2" => PhotoFormat.RDR2,
                    _ => throw new ArgumentException("Invalid format", nameof(format))
                };
            }

            if (description != null)
                photo.Description = description;

            if (json != null)
                photo.Json = json;

            if (title != null)
                photo.Title = title;

            if (jpegFile == String.Empty) {
                photo.Jpeg = Properties.Resources.EmptyJpeg;
                photo.Json = Json.UpdateSign(photo, photo.Json);
            }
            else if (jpegFile != null) {
                using MemoryStream jpegStream = new();
                using Stream input = jpegFile == "-" ? Console.OpenStandardInput() : File.OpenRead(jpegFile);
                input.CopyTo(jpegStream);
                photo.Jpeg = jpegStream.ToArray();
                photo.Json = Json.UpdateSign(photo, photo.Json);
            }
            else if (updateSign) {
                photo.Json = Json.UpdateSign(photo, photo.Json);
            }

            if (outputFile == "-") {
                using MemoryStream photoStream = new(photo.Save());
                using Stream output = Console.OpenStandardOutput();
                photoStream.CopyTo(output);
            }
            else {
                String tempFile = Path.GetTempFileName();
                photo.SaveFile(tempFile);
                File.Move(tempFile, !String.IsNullOrEmpty(outputFile) ? outputFile : inputFile, true);
            }
            return 0;
        }
        catch (RagePhotoException exception) {
            Console.Error.WriteLine(exception.Message);
            return exception.Photo != null ? (Int32)exception.Error + 2 : -1;
        }
        catch (ArgumentException exception) {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
        catch (Exception exception) {
            Console.Error.WriteLine(exception.Message);
            return -1;
        }
    }

    internal static Int32 PathFunction(String command) {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return 0;
        try {
            if (command == "register" || command == "unregister") {
                String appPath = Path.GetDirectoryName(Environment.ProcessPath) ??
                    throw new Exception("Application path can not be found");
                String fullAppPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(appPath));
                using RegistryKey environmentKey = Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true) ??
                    throw new Exception("Environment registry key can not be opened");
                String? path = environmentKey.GetValue(
                    "Path", null, RegistryValueOptions.DoNotExpandEnvironmentNames) as String ??
                    throw new Exception("Path registry value is invalid");
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
            else {
                Console.Error.WriteLine("Invalid path command supplied");
                return 0;
            }
        }
        catch (Exception exception) {
            Console.Error.WriteLine(exception.Message);
            return -1;
        }
    }

    internal static Command CreateCommand {
        get {
            Argument<String> formatArgument = new("format") {
                Description = "Photo Format"
            };
            formatArgument.CompletionSources.Add(_ => [
                new("gta5"),
                new("rdr2")]);
            Option<String?> jpegOption = new("--jpeg", "--image", "-i") {
                Description = "JPEG File"
            };
            Option<String?> descriptionOption = new("--description", "-d") {
                Description = "Photo Description"
            };
            Option<String?> jsonOption = new("--json", "-j") {
                Description = "Photo JSON"
            };
            Option<String?> titleOption = new("--title", "-t") {
                Description = "Photo Title"
            };
            Option<String> outputOption = new("--output", "-o") {
                Description = "Output File",
                DefaultValueFactory = _ => "-"
            };
            Command createCommand = new("create", "Create Photo") {
                formatArgument, jpegOption, descriptionOption, jsonOption, titleOption, outputOption
            };
            createCommand.SetAction(result => Environment.Exit(CreateFunction(
                result.GetRequiredValue(formatArgument),
                result.GetValue(jpegOption),
                result.GetValue(descriptionOption),
                result.GetValue(jsonOption),
                result.GetValue(titleOption),
                result.GetRequiredValue(outputOption))));
            return createCommand;
        }
    }

    internal static Command GetCommand {
        get {
            Argument<String> inputArgument = new("input") {
                Description = "Input File"
            };
            Argument<String> dataTypeArgument = new("dataType") {
                Description = "Data Type",
                DefaultValueFactory = _ => "jpeg"
            };
            dataTypeArgument.CompletionSources.Add(_ => [
                new("description"),
                new("format"),
                new("jpeg"),
                new("json"),
                new("sign"),
                new("title")]);
            Option<String> outputOption = new("--output", "-o") {
                Description = "Output File",
                DefaultValueFactory = _ => "-"
            };
            Command getCommand = new("get", "Get Photo Data") {
                inputArgument, dataTypeArgument, outputOption
            };
            getCommand.SetAction(result => Environment.Exit(GetFunction(
                result.GetRequiredValue(inputArgument),
                result.GetRequiredValue(dataTypeArgument),
                result.GetRequiredValue(outputOption))));
            return getCommand;
        }
    }

    internal static Command SetCommand {
        get {
            Argument<String> inputArgument = new("input") {
                Description = "Input File"
            };
            Option<String?> formatOption = new("--format", "-f") {
                Description = "Photo Format"
            };
            Option<String?> jpegOption = new("--jpeg", "--image", "-i") {
                Description = "JPEG File"
            };
            Option<String?> descriptionOption = new("--description", "-d") {
                Description = "Photo Description"
            };
            Option<String?> jsonOption = new("--json", "-j") {
                Description = "Photo JSON"
            };
            Option<String?> titleOption = new("--title", "-t") {
                Description = "Photo Title"
            };
            Option<bool> updateSignOption = new("--update-sign", "-u") {
                Description = "Update Photo Signature"
            };
            Option<String?> outputOption = new("--output", "-o") {
                Description = "Output File"
            };
            Command setCommand = new("set", "Set Photo Data") {
                inputArgument, formatOption, jpegOption, descriptionOption, jsonOption, titleOption, updateSignOption, outputOption
            };
            setCommand.SetAction(result => SetFunction(
                result.GetRequiredValue(inputArgument),
                result.GetValue(formatOption),
                result.GetValue(jpegOption),
                result.GetValue(descriptionOption),
                result.GetValue(jsonOption),
                result.GetValue(titleOption),
                result.GetValue(updateSignOption),
                result.GetValue(outputOption)));
            return setCommand;
        }
    }

    internal static Command PathCommand {
        get {
            Argument<String> commandArgument = new("command") {
                Description = "Path Command"
            };
            commandArgument.CompletionSources.Add(_ => [
                new ("register"),
                new ("unregister")]);
            Command pathCommand = new("path", "Register/Unregister Path") {
                commandArgument
            };
            pathCommand.Hidden = true;
            pathCommand.SetAction(result => Environment.Exit(PathFunction(
                result.GetRequiredValue(commandArgument))));
            return pathCommand;
        }
    }
}
