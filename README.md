## ragephoto-cli
Open Source RAGE Photo CLI based on libragephoto

- Read/Write RAGE Photos with get/set commands
- Support for stdin/stdout in input/output and JPEG option

#### Build ragephoto-cli

```sh
git clone https://github.com/Syping/ragephoto-cli
dotnet publish -c Release ragephoto-cli
```

#### How to Use ragephoto-cli

```sh
# Exporting JPEG
ragephoto-cli get "$INPUT" --output "photo.jpg"

# Getting Format
ragephoto-cli get "$INPUT" format

# Getting JSON
ragephoto-cli get "$INPUT" json

# Getting Title
ragephoto-cli get "$INPUT" title

# Replacing Image
ragephoto-cli set "$INPUT" --image "photo.jpg"

# Replacing JSON
ragephoto-cli set "$INPUT" --json "$JSON"

# Replacing Title
ragephoto-cli set "$INPUT" --title "$TITLE"

# Updating JSON (Fixes some JSON errors)
ragephoto-cli set "$INPUT" --update-json
```
