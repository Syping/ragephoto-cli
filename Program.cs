using System.CommandLine;
using System.Text;
namespace RagePhoto.Cli;

internal static class Program {

    private static void Main(String[] args) {
        RootCommand rootCommand = new("ragephoto-cli Application") {
            GetCommand, SetCommand
        };
        rootCommand.Parse(args).Invoke();
    }

    private static void Get(FileInfo photoFile, String dataType, String outputFile) {
        try {
            using Photo photo = new();
            photo.LoadFile(photoFile.FullName);

            Byte[] content = [];
            switch (dataType.ToLowerInvariant()) {
                case "d":
                case "description":
                    content = Encoding.UTF8.GetBytes(photo.Description);
                    break;
                case "f":
                case "format":
                    content = Encoding.UTF8.GetBytes(photo.Format switch {
                        PhotoFormat.GTA5 => "gta5",
                        PhotoFormat.RDR2 => "rdr2",
                        _ => "unknown"
                    });
                    break;
                case "i":
                case "image":
                case "jpeg":
                    content = photo.Jpeg;
                    break;
                case "j":
                case "json":
                    content = Encoding.UTF8.GetBytes(photo.Json);
                    break;
                case "s":
                case "sign":
                    content = Encoding.UTF8.GetBytes($"{photo.Sign}");
                    break;
                case "t":
                case "title":
                    content = Encoding.UTF8.GetBytes(photo.Title);
                    break;
                default:
                    Console.Error.WriteLine($"Unknown Content Type: {dataType}");
                    Environment.Exit(1);
                    break;
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
        }
        catch (RagePhotoException exception) {
            Console.Error.WriteLine(exception.Message);
            Environment.Exit(exception.Photo != null ? (Int32)exception.Error + 2 : -1);
        }
        catch (Exception exception) {
            Console.Error.WriteLine(exception.Message);
            Environment.Exit(-1);
        }
    }

    private static void Set(FileInfo photoFile, String? format, String? jpegFile, String? description, String? json, String? title, String? outputFile) {
        if (format == null && jpegFile == null &&
            description == null && json == null && title == null) {
            Console.Error.WriteLine("No value has being set");
            Environment.Exit(1);
        }

        try {
            using Photo photo = new();
            photo.LoadFile(photoFile.FullName);

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
            }
            else if (jpegFile != null) {
                using MemoryStream jpegStream = new();
                using Stream input = jpegFile == "-" ? Console.OpenStandardInput() : File.OpenRead(jpegFile);
                input.CopyTo(jpegStream);
                photo.Jpeg = jpegStream.ToArray();
            }

            if (outputFile == "-") {
                using MemoryStream photoStream = new(photo.Save());
                using Stream output = Console.OpenStandardOutput();
                photoStream.CopyTo(output);
            }
            else {
                String tempFile = Path.GetTempFileName();
                photo.SaveFile(tempFile);
                File.Move(tempFile, !String.IsNullOrEmpty(outputFile) ? outputFile : photoFile.FullName, true);
            }
        }
        catch (RagePhotoException exception) {
            Console.Error.WriteLine(exception.Message);
            Environment.Exit(exception.Photo != null ? (Int32)exception.Error + 2 : -1);
        }
        catch (ArgumentException exception) {
            Console.Error.WriteLine(exception.Message);
            Environment.Exit(1);
        }
        catch (Exception exception) {
            Console.Error.WriteLine(exception.Message);
            Environment.Exit(-1);
        }
    }

    private static Command GetCommand {
        get {
            Argument<FileInfo> photoArgument = new("photo") {
                Description = "Photo File"
            };
            Argument<String> dataTypeArgument = new("dataType") {
                Description = "Data Type",
                DefaultValueFactory = _ => "jpeg"
            };
            Option<String> outputOption = new("--output", "-o") {
                Description = "Output File",
                DefaultValueFactory = _ => "-"
            };
            Command getCommand = new("get", "Get Photo Data") {
                photoArgument, dataTypeArgument, outputOption
            };
            getCommand.SetAction(result => Get(
                result.GetRequiredValue(photoArgument),
                result.GetRequiredValue(dataTypeArgument),
                result.GetRequiredValue(outputOption)));
            return getCommand;
        }
    }

    private static Command SetCommand {
        get {
            Argument<FileInfo> photoArgument = new("photo") {
                Description = "Photo File"
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
            Option<String?> outputOption = new("--output", "-o") {
                Description = "Output File"
            };
            Command setCommand = new("set", "Set Photo Data") {
                photoArgument, formatOption, jpegOption, descriptionOption, jsonOption, titleOption, outputOption
            };
            setCommand.SetAction(result => Set(
                result.GetRequiredValue(photoArgument),
                result.GetValue(formatOption),
                result.GetValue(jpegOption),
                result.GetValue(descriptionOption),
                result.GetValue(jsonOption),
                result.GetValue(titleOption),
                result.GetValue(outputOption)));
            return setCommand;
        }
    }
}
