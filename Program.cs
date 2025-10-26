using System.CommandLine;
using System.Text;
namespace RagePhoto.Cli;

internal static class Program {

    private static void Main(String[] args) {
        RootCommand rootCommand = new("ragephoto-cli Application") {
            CreateCommand, GetCommand, SetCommand
        };
        rootCommand.Parse(args).Invoke();
    }

    private static void Create(FileInfo photoFile, String format, String jpegFile, String? title) {
        try {
            using Photo photo = new();

            photo.Format = format.ToLowerInvariant() switch {
                "gta5" => PhotoFormat.GTA5,
                "rdr2" => PhotoFormat.RDR2,
                _ => throw new ArgumentException("Invalid format", nameof(format))
            };

            if (jpegFile == String.Empty) {
                photo.Jpeg = Properties.Resources.EmptyJpeg;
            }
            else if (jpegFile != null) {
                using MemoryStream jpegStream = new();
                using Stream input = jpegFile == "-" ? Console.OpenStandardInput() : File.OpenRead(jpegFile);
                input.CopyTo(jpegStream);
                photo.Jpeg = jpegStream.ToArray();
            }

            Byte[] buffer = new Byte[4];
            Random.Shared.NextBytes(buffer);
            UInt32 photoUid = BitConverter.ToUInt32(buffer, 0);

            if (photo.Format == PhotoFormat.GTA5) {
                DateTimeOffset photoTime = DateTimeOffset.FromUnixTimeSeconds(Random.Shared.Next(1356998400, 1388534399));
                photo.SetHeader("PHOTO - 10/26/25 02:28:08", 0x2F99E5D9, 0x00000000);
                photo.Json = "{\"loc\":{\"x\":0,\"y\":0,\"z\":0}," +
                    "\"area\":\"SANAND\",\"street\":0,\"nm\":\"\",\"rds\":\"\"," +
                    "\"scr\":1,\"sid\":\"0x0\",\"crewid\":0,\"mid\":\"\"," +
                    "\"mode\":\"FREEMODE\",\"meme\":false,\"mug\":false," +
                    $"\"uid\":{photoUid}," + "\"time\":{" +
                    $"\"hour\":{photoTime.Hour}," +
                    $"\"minute\":{photoTime.Minute}," +
                    $"\"second\":{photoTime.Second}," +
                    $"\"day\":{photoTime.Day}," +
                    $"\"month\":{photoTime.Month}," +
                    $"\"year\":{photoTime.Year}" + "}," +
                    $"\"creat\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}," +
                    "\"slf\":true,\"drctr\":false,\"rsedtr\":false,\"cv\":true," +
                    $"\"sign\":{photo.Sign}" +
                    "}";
            }
            else {
                DateTimeOffset photoTime = DateTimeOffset.FromUnixTimeSeconds(Random.Shared.NextInt64(-2208988801, -2240524800));
                photo.SetHeader("PHOTO - 10/26/25 02:31:34", 0xB76BFD68, 0xACEF57D2);
                photo.Json = "{\"loc\":{\"x\":0,\"y\":0,\"z\":0}," +
                    "\"regionname\":0,\"districtname\":0,\"statename\":0,\"nm\":\"\"," +
                    "\"sid\":\"0x0\",\"crewid\":0,\"mid\":\"\",\"mode\":\"SP\"," +
                    "\"meme\":false,\"mug\":false," +
                    $"\"uid\":{photoUid}," + "\"time\":{" +
                    $"\"hour\":{photoTime.Hour}," +
                    $"\"minute\":{photoTime.Minute}," +
                    $"\"second\":{photoTime.Second}," +
                    $"\"day\":{photoTime.Day}," +
                    $"\"month\":{photoTime.Month}," +
                    $"\"year\":{photoTime.Year}" + "}," +
                    $"\"creat\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}," +
                    "\"slf\":false,\"drctr\":false,\"rsedtr\":false,\"inphotomode\":true," +
                    "\"advanced\":false," +
                    "\"width\":1920," +
                    "\"height\":1080," +
                    $"\"size\":{photo.JpegSize}," +
                    $"\"sign\":{photo.Sign}" +
                    "}";
            }

            photo.Title = title ?? "Custom Photo";

            String tempFile = Path.GetTempFileName();
            photo.SaveFile(tempFile);
            File.Move(tempFile, photoFile.FullName, true);
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

    private static Command CreateCommand {
        get {
            Argument<FileInfo> photoArgument = new("photo") {
                Description = "Photo File"
            };
            Argument<String> formatArgument = new("format") {
                Description = "Photo Format",
            };
            formatArgument.CompletionSources.Add(_ => [
                new("gta5"),
                new("rdr2")]);
            Argument<String> jpegArgument = new("jpeg") {
                Description = "JPEG File",
            };
            Argument<String?> titleArgument = new("title") {
                Description = "Photo Title",
                DefaultValueFactory = _ => null
            };
            Command createCommand = new("create", "Create Photo") {
                photoArgument, formatArgument, jpegArgument, titleArgument
            };
            createCommand.SetAction(result => Create(
                result.GetRequiredValue(photoArgument),
                result.GetRequiredValue(formatArgument),
                result.GetRequiredValue(jpegArgument),
                result.GetValue(titleArgument)));
            return createCommand;
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
