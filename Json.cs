using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
namespace RagePhoto.Cli;

internal class Json {

    internal static String Initialize(PhotoFormat format, Photo photo, Int32 photoUid, DateTimeOffset photoTime) {
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
                ["sign"] = photo.Sign,
                ["plyrs"] = new JsonArray()
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
                ["sign"] = photo.Sign,
                ["meta"] = new JsonObject()
            },
            _ => throw new ArgumentException("Invalid format", nameof(format)),
        };
        return json.ToJsonString(new() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    internal static String UpdateSign(Photo photo, String json) {
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
}
