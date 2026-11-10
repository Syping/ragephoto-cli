using System.Drawing;
using System.Drawing.Imaging;
namespace RagePhoto.Cli;

internal class Jpeg {

    internal static Byte[] GetEmptyJpeg(PhotoFormat format, out Size size) {
        size = format switch {
            PhotoFormat.GTA5 => new(960, 536),
            PhotoFormat.RDR2 => new(1920, 1080),
            _ => throw new ArgumentException("Invalid Format", nameof(format))
        };
        using Bitmap bitmap = new(size.Width, size.Height);
        using (Graphics graphics = Graphics.FromImage(bitmap)) {
            graphics.Clear(Color.Black);
        }
        using MemoryStream jpegStream = new();
        bitmap.Save(jpegStream, ImageFormat.Jpeg);
        return jpegStream.ToArray();
    }

    internal static Byte[] GetJpeg(Stream input, Boolean imageAsIs, out Size size) {
        if (imageAsIs) {
            using MemoryStream jpegStream = new();
            input.CopyTo(jpegStream);
            Byte[] jpeg = jpegStream.ToArray();
            size = GetSize(jpeg);
            return jpeg;
        }
        else {
            using Image image = Image.FromStream(input);
            size = image.Size;
            using Bitmap bitmap = new(size.Width, size.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap)) {
                graphics.DrawImageUnscaled(image, 0, 0);
            }
            using MemoryStream jpegStream = new();
            bitmap.Save(jpegStream, ImageFormat.Jpeg);
            return jpegStream.ToArray();
        }
    }

    internal static Size GetSize(Byte[] jpeg) {
        using MemoryStream jpegStream = new();
        using Image image = Image.FromStream(jpegStream, false, false);
        return image.Size;
    }
}
