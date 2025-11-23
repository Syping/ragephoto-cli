using SixLabors.ImageSharp;
using System.CommandLine;
using System.Text;
namespace RagePhoto.Cli;

internal static partial class Commands {

    internal static Int32 CreateFunction(String format, String? imageFile, String? description,
        String? json, String? title, String? outputFile, Boolean imageAsIs) {
        try {
            using Photo photo = new();

            switch (format.ToLowerInvariant()) {
                case "gta5":
                    photo.Format = PhotoFormat.GTA5;
                    photo.SetHeader("PHOTO - 10/26/25 02:28:08", 798615001, 0);
                    break;
                case "rdr2":
                    photo.Format = PhotoFormat.RDR2;
                    photo.SetHeader("PHOTO - 10/26/25 02:31:34", 3077307752, 2901366738);
                    break;
                default:
                    throw new ArgumentException("Invalid Format", nameof(format));
            }

            Size size;
            if (String.IsNullOrEmpty(imageFile)) {
                photo.Jpeg = Jpeg.GetEmptyJpeg(photo.Format, out size);
            }
            else {
                using Stream input = imageFile == "-" ? Console.OpenStandardInput() : File.OpenRead(imageFile);
                photo.Jpeg = Jpeg.GetJpeg(input, imageAsIs, out size);
            }

            photo.Json = json == null ?
                Json.Initialize(photo, size, out Int32 uid) :
                Json.Update(photo, size, photo.Json, out uid);

            photo.Description = description ?? String.Empty;
            photo.Title = title ?? "Custom Photo";

            if (outputFile == "-") {
                using Stream output = Console.OpenStandardOutput();
                output.Write(photo.Save());
            }
            else {
                String tempFile = Path.GetTempFileName();
                photo.SaveFile(tempFile);
                if (!String.IsNullOrEmpty(outputFile)) {
                    File.Move(tempFile, outputFile, true);
                }
                else {
                    outputFile = $"{photo.Format switch {
                        PhotoFormat.GTA5 => "PGTA5",
                        PhotoFormat.RDR2 => "PRDR3",
                        _ => String.Empty
                    }}{uid}";
                    File.Move(tempFile, outputFile, true);
                    Console.WriteLine(outputFile);
                }
            }
            return 0;
        }
        catch (RagePhotoException exception) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(exception.Message);
            Console.ResetColor();
            return exception.Photo != null ? (Int32)exception.Error + 2 : -1;
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
                        _ => throw new ArgumentException("Invalid Format")
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
                    Console.Error.WriteLine($"Unknown Data Type: {dataType}");
                    return 1;
            }

            if (outputFile == "-" || outputFile == String.Empty) {
                using Stream output = Console.OpenStandardOutput();
                output.Write(content);
            }
            else {
                String tempFile = Path.GetTempFileName();
                using (FileStream output = File.Create(tempFile)) {
                    output.Write(content);
                }
                File.Move(tempFile, outputFile, true);
            }
            return 0;
        }
        catch (RagePhotoException exception) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(exception.Message);
            Console.ResetColor();
            return exception.Photo != null ? (Int32)exception.Error + 2 : -1;
        }
        catch (Exception exception) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(exception.Message);
            Console.ResetColor();
            return -1;
        }
    }

    internal static Int32 SetFunction(String inputFile, String? format, String? imageFile, String? description,
        String? json, String? title, Boolean updateJson, String? outputFile, Boolean imageAsIs) {
        try {
            if (format == null && imageFile == null && description == null
                && json == null && title == null && !updateJson) {
                throw new ArgumentException("No Value has being set");
            }
            else if ((inputFile == "-" || inputFile == String.Empty) && imageFile == "-") {
                throw new ArgumentException("Multiple Pipes are not supported");
            }

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
                    _ => throw new ArgumentException("Invalid Format", nameof(format))
                };
            }

            if (description != null)
                photo.Description = description;

            if (json != null)
                photo.Json = json;

            if (title != null)
                photo.Title = title;

            Size size;
            if (imageFile == String.Empty) {
                photo.Jpeg = Jpeg.GetEmptyJpeg(photo.Format, out size);
                photo.Json = Json.Update(photo, size, photo.Json, out Int32 uid);
            }
            else if (imageFile != null) {
                using Stream input = imageFile == "-" ? Console.OpenStandardInput() : File.OpenRead(imageFile);
                photo.Jpeg = Jpeg.GetJpeg(input, imageAsIs, out size);
                photo.Json = Json.Update(photo, size, photo.Json, out Int32 uid);
            }
            else if (updateJson) {
                size = Jpeg.GetSize(photo.Jpeg);
                photo.Json = Json.Update(photo, size, photo.Json, out Int32 uid);
            }

            if (outputFile == "-") {
                using Stream output = Console.OpenStandardOutput();
                output.Write(photo.Save());
            }
            else {
                String tempFile = Path.GetTempFileName();
                photo.SaveFile(tempFile);
                File.Move(tempFile, !String.IsNullOrEmpty(outputFile) ? outputFile : inputFile, true);
            }
            return 0;
        }
        catch (RagePhotoException exception) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(exception.Message);
            Console.ResetColor();
            return exception.Photo != null ? (Int32)exception.Error + 2 : -1;
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

    internal static Command CreateCommand {
        get {
            Argument<String> formatArgument = new("format") {
                Description = "Format"
            };
            formatArgument.CompletionSources.Add(_ => [
                new("gta5"),
                new("rdr2")]);
            formatArgument.Validators.Add(result => {
                String[] formats = ["gta5", "rdr2"];
                String format = result.GetValueOrDefault<String>();
                if (!formats.Contains(format, StringComparer.InvariantCultureIgnoreCase))
                    result.AddError("Invalid Format.");
            });
            Option<String?> imageOption = new("--image", "-i", "--jpeg") {
                Description = "Image File"
            };
            Option<String?> descriptionOption = new("--description", "-d") {
                Description = "Description"
            };
            Option<String?> jsonOption = new("--json", "-j") {
                Description = "JSON"
            };
            Option<String?> titleOption = new("--title", "-t") {
                Description = "Title"
            };
            Option<String?> outputOption = new("--output", "-o") {
                Description = "Output File"
            };
            Option<Boolean> imageAsIsOption = new("--image-as-is") {
                Description = "Force image being set as-is"
            };
            Command createCommand = new("create", "Create a new Photo") {
                formatArgument,
                imageOption,
                descriptionOption,
                jsonOption,
                titleOption,
                outputOption,
                imageAsIsOption
            };
            createCommand.SetAction(result => Environment.ExitCode = CreateFunction(
                result.GetRequiredValue(formatArgument),
                result.GetValue(imageOption),
                result.GetValue(descriptionOption),
                result.GetValue(jsonOption),
                result.GetValue(titleOption),
                result.GetValue(outputOption),
                result.GetValue(imageAsIsOption)));
            return createCommand;
        }
    }

    internal static Command GetCommand {
        get {
            Argument<String> inputArgument = new("input") {
                Description = "Input File"
            };
            Argument<String> typeArgument = new("type") {
                Description = "Data Type",
                DefaultValueFactory = _ => "jpeg"
            };
            typeArgument.CompletionSources.Add(_ => [
                new("description"),
                new("format"),
                new("jpeg"),
                new("json"),
                new("sign"),
                new("title")]);
            typeArgument.Validators.Add(result => {
                String[] types = [
                    "d", "description",
                    "f", "format",
                    "i", "image", "jpeg",
                    "j", "json",
                    "s", "sign",
                    "t", "title"];
                String type = result.GetValueOrDefault<String>();
                if (!types.Contains(type, StringComparer.InvariantCultureIgnoreCase))
                    result.AddError($"Unknown Data Type: {type}.");
            });
            Option<String> outputOption = new("--output", "-o") {
                Description = "Output File",
                DefaultValueFactory = _ => "-"
            };
            Command getCommand = new("get", "Get Data from a Photo") {
                inputArgument,
                typeArgument,
                outputOption
            };
            getCommand.SetAction(result => Environment.ExitCode = GetFunction(
                result.GetRequiredValue(inputArgument),
                result.GetRequiredValue(typeArgument),
                result.GetRequiredValue(outputOption)));
            return getCommand;
        }
    }

    internal static Command SetCommand {
        get {
            Argument<String> inputArgument = new("input") {
                Description = "Input File"
            };
            Option<String?> formatOption = new("--format", "-f") {
                Description = "Format"
            };
            Option<String?> imageOption = new("--image", "-i", "--jpeg") {
                Description = "Image File"
            };
            Option<String?> descriptionOption = new("--description", "-d") {
                Description = "Description"
            };
            Option<String?> jsonOption = new("--json", "-j") {
                Description = "JSON"
            };
            Option<String?> titleOption = new("--title", "-t") {
                Description = "Title"
            };
            Option<Boolean> updateJsonOption = new("--update-json", "-u") {
                Description = "Update JSON"
            };
            Option<String?> outputOption = new("--output", "-o") {
                Description = "Output File"
            };
            Option<Boolean> imageAsIsOption = new("--image-as-is") {
                Description = "Force image being set as-is"
            };
            Command setCommand = new("set", "Set Data from a Photo") {
                inputArgument,
                formatOption,
                imageOption,
                descriptionOption,
                jsonOption,
                titleOption,
                updateJsonOption,
                outputOption,
                imageAsIsOption
            };
            setCommand.SetAction(result => Environment.ExitCode = SetFunction(
                result.GetRequiredValue(inputArgument),
                result.GetValue(formatOption),
                result.GetValue(imageOption),
                result.GetValue(descriptionOption),
                result.GetValue(jsonOption),
                result.GetValue(titleOption),
                result.GetValue(updateJsonOption),
                result.GetValue(outputOption),
                result.GetValue(imageAsIsOption)));
            return setCommand;
        }
    }
}
