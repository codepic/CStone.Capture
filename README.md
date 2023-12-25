# CStone.Capture

## Tesseract OCR Training Data

You have basically 2 options:

1. Pull the [git submodule](https://github.com/tesseract-ocr/tessdata.git) into `/tessdata`
2. [Train your own tessdata](https://pretius.com/blog/ocr-tesseract-training-data/)

Whichever you choose, point you `TESSDATA_PREFIX` environment variable to the full path of the `tessdata` directory. Do ditch the trailing slash!