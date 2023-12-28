# CStone.Capture (WIP)

## Usage

```pwsh
dotnet watch {server} {origin} {destination}

# Example
dotnet watch 'EU' 'ARC L1' 'CRU L5'
```

## Tesseract OCR Training Data

You have basically 2 options:

1. Download [eng.traineddata](https://github.com/tesseract-ocr/tessdata_fast) into `/tessdata`
2. [Train your own tessdata](https://pretius.com/blog/ocr-tesseract-training-data/)

Whichever you choose, point you `TESSDATA_PREFIX` environment variable to the full path of the `tessdata` directory. Do ditch the trailing slash!

## Usage

First off, clone this repo (with submodules).

```pwsh
# check that it works
dotnet test

# run the app
cd src
dotnet watch
```

Lauch the game, warp to a location, and start scanning.

## Key Bindings

### F2

The app will turn into `Location` mode and wait for you to select a location (Stanton star)

### V

#### KeyDown

The app will enter `Scanning` mode.

#### KeyUp

The app will save the results of the scanning

### B

The app will enter Quantum mode.