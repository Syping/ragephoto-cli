using System.CommandLine;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
namespace RagePhoto.Cli;

internal static class Program {

    private static void Main(String[] args) {
        RootCommand rootCommand = new("ragephoto-cli Application") {
            CreateCommand, GetCommand, SetCommand
        };
        rootCommand.Parse(args).Invoke();
    }

    private static void Create(String format, String jpegFile, String outputFile, String? description, String? json, String? title) {
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

            if (jpegFile == String.Empty) {
                photo.Jpeg = Properties.Resources.EmptyJpeg;
            }
            else if (jpegFile != null) {
                using MemoryStream jpegStream = new();
                using Stream input = jpegFile == "-" ? Console.OpenStandardInput() : File.OpenRead(jpegFile);
                input.CopyTo(jpegStream);
                photo.Jpeg = jpegStream.ToArray();
            }

            if (photo.Format == PhotoFormat.GTA5) {
                photo.Json = json == null ?
                    InitializeJson(PhotoFormat.GTA5, photo, Random.Shared.Next(),
                    DateTimeOffset.FromUnixTimeSeconds(Random.Shared.Next(1356998400, 1388534399))) :
                    UpdateSign(photo, photo.Json);
            }
            else {
                photo.Json = json == null ?
                    InitializeJson(PhotoFormat.RDR2, photo, Random.Shared.Next(),
                    DateTimeOffset.FromUnixTimeSeconds(Random.Shared.NextInt64(-2240524800, -2208988801))) :
                    UpdateSign(photo, photo.Json);
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
                photo.Json = UpdateSign(photo, photo.Json);
            }
            else if (jpegFile != null) {
                using MemoryStream jpegStream = new();
                using Stream input = jpegFile == "-" ? Console.OpenStandardInput() : File.OpenRead(jpegFile);
                input.CopyTo(jpegStream);
                photo.Jpeg = jpegStream.ToArray();
                photo.Json = UpdateSign(photo, photo.Json);
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

    private static String InitializeJson(PhotoFormat format, Photo photo, Int32 photoUid, DateTimeOffset photoTime) {
        JsonObject jsonLocation = new() {
            ["x"] = 0,
            ["y"] = 0,
            ["z"] = 0
        };
        JsonObject jsonTime = new() {
            ["hour"] = photoTime.Hour,
            ["minute"] = photoTime.Minute,
            ["second"] = photoTime.Second,
            ["day"] = photoTime.Day,
            ["month"] = photoTime.Month,
            ["year"] = photoTime.Year
        };
        JsonObject json = format switch {
            PhotoFormat.GTA5 => new() {
                ["loc"] = jsonLocation,
                ["area"] = "SANAND",
                ["street"] = 0,
                ["nm"] = String.Empty,
                ["rds"] = String.Empty,
                ["scr"] = 1,
                ["sid"] = "0x0",
                ["crewid"] = 0,
                ["mid"] = String.Empty,
                ["mode"] = "FREEMODE",
                ["meme"] = false,
                ["uid"] = photoUid,
                ["time"] = jsonTime,
                ["creat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["slf"] = true,
                ["drctr"] = false,
                ["rsedtr"] = false,
                ["cv"] = true,
                ["sign"] = photo.Sign
            },
            PhotoFormat.RDR2 => new() {
                ["loc"] = jsonLocation,
                ["regionname"] = 0,
                ["districtname"] = 0,
                ["statename"] = 0,
                ["nm"] = String.Empty,
                ["sid"] = "0x0",
                ["crewid"] = 0,
                ["mid"] = String.Empty,
                ["mode"] = "SP",
                ["meme"] = false,
                ["mug"] = false,
                ["uid"] = photoUid,
                ["time"] = jsonTime,
                ["creat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["slf"] = false,
                ["drctr"] = false,
                ["rsedtr"] = false,
                ["inphotomode"] = true,
                ["advanced"] = false,
                ["width"] = 1920,
                ["height"] = 1080,
                ["size"] = photo.JpegSize,
                ["sign"] = photo.Sign
            },
            _ => throw new ArgumentException("Invalid format", nameof(format)),
        };
        return json.ToJsonString(new() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    private static String UpdateSign(Photo photo, String json) {
        try {
            if (JsonNode.Parse(json) is not JsonObject jsonObject)
                throw new ArgumentException("Invalid json", nameof(json));
            jsonObject["sign"] = photo.Sign;
            return jsonObject.ToJsonString(new() {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
        catch (Exception exception) {
            Console.Error.WriteLine($"Failed to update sign: {exception.Message}");
            return json;
        }
    }

    private static Command CreateCommand {
        get {
            Argument<String> formatArgument = new("format") {
                Description = "Photo Format"
            };
            formatArgument.CompletionSources.Add(_ => [
                new("gta5"),
                new("rdr2")]);
            Argument<String> jpegArgument = new("jpeg") {
                Description = "JPEG File",
                DefaultValueFactory = _ => "-"
            };
            Argument<String> outputArgument = new("output") {
                Description = "Output File",
                DefaultValueFactory = _ => "-"
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
            Command createCommand = new("create", "Create Photo") {
                formatArgument, jpegArgument, outputArgument, descriptionOption, titleOption
            };
            createCommand.SetAction(result => Create(
                result.GetRequiredValue(formatArgument),
                result.GetRequiredValue(jpegArgument),
                result.GetRequiredValue(outputArgument),
                result.GetValue(descriptionOption),
                result.GetValue(jsonOption),
                result.GetValue(titleOption)));
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
