# CStone.Capture

## Tesseract OCR Training Data

You have basically 2 options:

1. Pull the [git submodule](https://github.com/tesseract-ocr/tessdata.git) into `/tessdata`
2. [Train your own tessdata](https://pretius.com/blog/ocr-tesseract-training-data/)

Whichever you choose, point you `TESSDATA_PREFIX` environment variable to the full path of the `tessdata` directory. Do ditch the trailing slash!

## Usage

First off, clone this repo (with submodules).

```
cd src
dotnet watch
```

Lauch the game, warp to a location.

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