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

# Replacing JPEG
ragephoto-cli set "$INPUT" --jpeg "photo.jpg"

# Replacing JSON
ragephoto-cli set "$INPUT" --json "$JSON"

# Patching Signature
ragephoto-cli set "$INPUT" --update-sign

# Updating Title
ragephoto-cli set "$INPUT" --title "$TITLE"
```
