using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
namespace RagePhoto.Cli;

internal class Jpeg {

    internal static Byte[] GetEmptyJpeg(PhotoFormat format, out Size size) {
        size = format == PhotoFormat.GTA5 ? new(960, 536) : new(1920, 1080);
        using Image<Rgb24> image = new(size.Width, size.Height);
        image.ProcessPixelRows(static pixelAccessor => {
            for (Int32 y = 0; y < pixelAccessor.Height; y++) {
                Span<Rgb24> pixelRow = pixelAccessor.GetRowSpan(y);
                for (Int32 x = 0; x < pixelRow.Length; x++) {
                    pixelRow[x] = Color.Black;
                }
            }
        });
        using MemoryStream output = new();
        image.SaveAsJpeg(output, new() {
            Quality = 100,
            ColorType = JpegEncodingColor.YCbCrRatio444
        });
        return output.ToArray();
    }

    internal static Byte[] GetJpeg(Stream stream, out Size size) {
        using Image image = Image.Load(stream);
        size = image.Size;
        image.Metadata.ExifProfile = null;
        using MemoryStream output = new();
        image.SaveAsJpeg(output, new() {
            Quality = 100,
            ColorType = JpegEncodingColor.YCbCrRatio444
        });
        return output.ToArray();
    }

    internal static Size GetSize(ReadOnlySpan<Byte> jpeg) {
        return Image.Identify(jpeg).Size;
    }
}
