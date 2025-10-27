## ragephoto-cli
Open Source RAGE Photo CLI based on libragephoto

- Read/Write RAGE Photos with get/set commands
- Support for stdin/stdout in JPEG option and output

#### Build ragephoto-cli

```sh
git clone https://github.com/Syping/ragephoto-cli
dotnet publish -c Release ragephoto-cli
```

#### How to Use ragephoto-cli

```sh
# Exporting JPEG
ragephoto-cli get PGTA5123456789 --output photo.jpg

# Getting Format
ragephoto-cli get PGTA5123456789 format

# Getting JSON
ragephoto-cli get PGTA5123456789 json

# Getting Title
ragephoto-cli get PGTA5123456789 title

# Replacing JPEG
ragephoto-cli set PGTA5123456789 --jpeg photo.jpg

# Patching Signature
ragephoto-cli set PGTA5123456789 --json "$(ragephoto-cli get PGTA5123456789 json \
    | jq -c ".sign = $(ragephoto-cli get PGTA5123456789 sign)")"

# Updating Title
ragephoto-cli set PGTA5123456789 --title "New Photo Title"
```
