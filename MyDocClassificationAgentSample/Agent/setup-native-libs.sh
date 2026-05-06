#!/bin/bash

# Setup script for native Tesseract/Leptonica libraries
# This script copies the native libraries to the location where Daenet.DocumentParser expects them

set -e

OUTPUT_DIR="bin/Debug/net9.0"
X64_DIR="$OUTPUT_DIR/x64"

echo "=== Setting up native libraries for Daenet.DocumentParser ==="

# Check if running on macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "Platform: macOS"
    
    # Check if Homebrew libraries exist
    if [ ! -f "/opt/homebrew/lib/libleptonica.dylib" ]; then
        echo "ERROR: Leptonica not found. Install with: brew install leptonica"
        exit 1
    fi
    
    if [ ! -f "/opt/homebrew/lib/libtesseract.dylib" ]; then
        echo "ERROR: Tesseract not found. Install with: brew install tesseract"
        exit 1
    fi
    
    # Create x64 directory if it doesn't exist
    mkdir -p "$X64_DIR"
    
    # Copy native libraries with expected names
    echo "Copying libleptonica to $X64_DIR/libleptonica-1.82.0.dylib"
    cp /opt/homebrew/lib/libleptonica.dylib "$X64_DIR/libleptonica-1.82.0.dylib"
    
    echo "Copying libtesseract to $X64_DIR/libtesseract50.dylib"
    cp /opt/homebrew/lib/libtesseract.dylib "$X64_DIR/libtesseract50.dylib"
    
    echo "✅ Native libraries setup complete!"
    echo "Libraries installed in: $X64_DIR"
    ls -lh "$X64_DIR"
    
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    echo "Platform: Linux"
    echo "ERROR: Linux support not yet implemented"
    echo "Please manually copy libleptonica and libtesseract to $X64_DIR"
    exit 1
else
    echo "Platform: Unknown ($OSTYPE)"
    echo "ERROR: This platform is not supported by this script"
    exit 1
fi

echo ""
echo "You can now run: dotnet run"
