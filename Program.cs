using RagePhoto;
using System.CommandLine;
using System.Text;

internal static class Program {

    private static void Main(String[] args) {

        Argument<FileInfo> photoArgument = new("photo") {
            Description = "Photo File"
        };
        Argument<String> dataTypeArgument = new("dataType") {
            Description = "Data Type",
            DefaultValueFactory = _ => "jpeg"
        };
        Option<String> outputOption = new("output", "o") {
            Description = "Output",
            DefaultValueFactory = _ => "-"
        };
        Command getCommand = new("get", "Get Photo Data") {
            photoArgument, dataTypeArgument, outputOption
        };
        getCommand.SetAction(result => Get(
            result.GetRequiredValue(photoArgument),
            result.GetRequiredValue(dataTypeArgument),
            result.GetRequiredValue(outputOption)));

        RootCommand rootCommand = new("ragephoto-cli Application") {
            getCommand
        };

        rootCommand.Parse(args).Invoke();
    }

    private static void Get(FileInfo photoFile, String dataType, String outputFile) {
        try {
            using Photo photo = new();
            photo.LoadFile(photoFile.FullName);

            Byte[] content = [];
            switch (dataType.ToLowerInvariant()) {
                case "format":
                    content = Encoding.UTF8.GetBytes(photo.Format switch {
                        PhotoFormat.GTA5 => "gta5",
                        PhotoFormat.RDR2 => "rdr2",
                        _ => "unknown"
                    });
                    break;
                case "jpeg":
                    content = photo.Jpeg;
                    break;
                case "json":
                    content = Encoding.UTF8.GetBytes(photo.Json);
                    break;
                case "sign":
                    content = Encoding.UTF8.GetBytes($"{photo.Sign}");
                    break;
                case "title":
                    content = Encoding.UTF8.GetBytes(photo.Title);
                    break;
                default:
                    Console.Error.WriteLine($"Unknown Content Type: {dataType}");
                    Environment.Exit(1);
                    break;
            }

            Stream output = outputFile == "-" ? Console.OpenStandardOutput() : File.Create(outputFile);
            output.Write(content);
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
}
