using SixLabors.ImageSharp;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace RagePhoto.Cli;

internal class Json {

    internal static String Initialize(Photo photo, Size size, out Int32 uid) {
        JsonObject jsonLocation = new() {
            ["x"] = 0,
            ["y"] = 0,
            ["z"] = 0
        };
        DateTimeOffset time = DateTimeOffset.FromUnixTimeSeconds(photo.Format switch {
            PhotoFormat.GTA5 => Random.Shared.Next(1356998400, 1388534399),
            PhotoFormat.RDR2 => Random.Shared.NextInt64(-2240524800, -2208988801),
            _ => throw new ArgumentException("Invalid Format")
        });
        JsonObject jsonTime = new() {
            ["hour"] = time.Hour,
            ["minute"] = time.Minute,
            ["second"] = time.Second,
            ["day"] = time.Day,
            ["month"] = time.Month,
            ["year"] = time.Year
        };
        uid = Random.Shared.Next();
        JsonObject json = photo.Format switch {
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
                ["mug"] = false,
                ["uid"] = uid,
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
                ["uid"] = uid,
                ["time"] = jsonTime,
                ["creat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["slf"] = false,
                ["drctr"] = false,
                ["rsedtr"] = false,
                ["inphotomode"] = true,
                ["advanced"] = false,
                ["width"] = size.Width,
                ["height"] = size.Height,
                ["size"] = photo.JpegSize,
                ["sign"] = photo.Sign,
                ["meta"] = new JsonObject()
            },
            _ => throw new ArgumentException("Invalid Format")
        };
        return json.ToJsonString(SerializerOptions);
    }

    internal static String Update(Photo photo, Size size, String json, out Int32 uid) {
        try {
            if (JsonNode.Parse(json) is not JsonObject jsonObject)
                throw new ArgumentException("Invalid JSON", nameof(json));
            if (jsonObject["uid"] is not JsonValue uidValue ||
                uidValue.GetValueKind() != JsonValueKind.Number) {
                uid = Random.Shared.Next();
                jsonObject["uid"] = uid;
            }
            else {
                uid = uidValue.GetValue<Int32>();
            }
            if (photo.Format == PhotoFormat.RDR2) {
                jsonObject["width"] = size.Width;
                jsonObject["height"] = size.Height;
            }
            jsonObject["sign"] = photo.Sign;
            jsonObject["size"] = photo.JpegSize;
            return jsonObject.ToJsonString(SerializerOptions);
        }
        catch (Exception exception) {
            Console.Error.WriteLine($"Failed to update JSON: {exception.Message}");
            uid = 0;
            return json;
        }
    }

    internal static readonly JsonSerializerOptions SerializerOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
