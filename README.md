# cstone.cli (WIP)

## Usage

```pwsh
# Setup (only once per terminal session)
cd src
dotnet build --configuration Release
$bin = Join-Path $pwd ./bin/Release/net8.0/
$Env:Path += [IO.Path]::PathSeparator + $bin
$tessdata = Join-Path $pwd ../tessdata
$Env:TESSDATA_PREFIX = $tessdata

# Example Usage (provided that you're analyzing Aaron Halo between ARC L1 and CRU L5)
cstone halo --server EU --origin 'ARC L1' --dest 'CRU L5'
```

## Commands

- [cstone halo --server {server} --origin {origin} --destination {destination}](src\commands\AaronHaloCommand.help.md)
- [cstone parse-logs [-d|--directory {path}]  [-o|--output (raw|console|csv)]](src\commands\LogParserCommand.help.md)

## Contributing (WIP)

First off, clone this repo (with submodules).



```pwsh

# check that it works
dotnet test

# run the app
cd src
dotnet watch halo --server EU --origin 'ARC L1' --dest 'CRU L5'
```

## Tesseract OCR Training Data (WIP)

> NOTE: To reach better accuracy (and speed), we might need to train the model with CS UI

You have basically 2 options:

1. Download [eng.traineddata](https://github.com/tesseract-ocr/tessdata_fast) into `/tessdata`
2. [Train your own tessdata](https://pretius.com/blog/ocr-tesseract-training-data/)

Whichever you choose, point you `TESSDATA_PREFIX` environment variable to the full path of the `tessdata` directory. Do ditch the trailing slash!
