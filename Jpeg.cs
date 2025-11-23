using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
namespace RagePhoto.Cli;

internal class Jpeg {

    internal static Byte[] GetEmptyJpeg(PhotoFormat format, out Size size) {
        size = format switch {
            PhotoFormat.GTA5 => new(960, 536),
            PhotoFormat.RDR2 => new(1920, 1080),
            _ => throw new ArgumentException("Invalid Format", nameof(format))
        };
        using Image<Rgb24> image = new(size.Width, size.Height);
        image.ProcessPixelRows(static pixelAccessor => {
            for (Int32 y = 0; y < pixelAccessor.Height; y++) {
                Span<Rgb24> pixelRow = pixelAccessor.GetRowSpan(y);
                for (Int32 x = 0; x < pixelRow.Length; x++) {
                    pixelRow[x] = Color.Black;
                }
            }
        });
        using MemoryStream jpegStream = new();
        image.SaveAsJpeg(jpegStream, new() {
            Quality = 100,
            ColorType = JpegEncodingColor.YCbCrRatio444
        });
        return jpegStream.ToArray();
    }

    internal static Byte[] GetJpeg(Stream input, bool imageAsIs, out Size size) {
        if (!imageAsIs) {
            using Image image = Image.Load(input);
            size = image.Size;
            image.Metadata.ExifProfile = null;
            using MemoryStream jpegStream = new();
            image.SaveAsJpeg(jpegStream, new() {
                Quality = 100,
                ColorType = JpegEncodingColor.YCbCrRatio444
            });
            return jpegStream.ToArray();
        }
        else {
            using MemoryStream jpegStream = new();
            input.CopyTo(jpegStream);
            Byte[] jpeg = jpegStream.ToArray();
            size = GetSize(jpeg);
            return jpeg;
        }
    }

    internal static Size GetSize(ReadOnlySpan<Byte> jpeg) {
        try {
            return Image.Identify(new DecoderOptions {
                Configuration = new(new JpegConfigurationModule())
            }, jpeg).Size;
        }
        catch (UnknownImageFormatException exception) {
            throw new Exception("Unsupported Image Format", exception);
        }
    }
}
