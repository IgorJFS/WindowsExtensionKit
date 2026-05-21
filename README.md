# WPF Utilities App

A C# WPF desktop application that extends Windows functionality by providing tools and utilities in a clean, straightforward interface. Built with modern .NET setup, this application groups different helpful resources in a single tabbed experience.

## Features

### 1. File Organizer
A fast and easy way to organize messy folders. Move all files of a specific format (e.g., `.png`, `.pdf`) from a chosen *source folder* into a *destination folder* with just one click.
* Note: Duplicate files natively are moved to the Recycle Bin instead of overwriting, preventing data loss.

### 2. Temperature Converter
A straightforward, real-time temperature converter between Celsius (°C) and Fahrenheit (°F). 
* Note: Automatically adjusts while typing.

### 3. Video to GIF Converter
Generate small `.gif` files from standard videos right on your desktop using FFmpeg. Instead of struggling with CLI commands, simply pick your video file and hit convert.
* Adjustable options for **FPS** and **Maximum Width** mapping.
* Operates locally — automatically downloads the official FFmpeg binaries on first startup.

## Preview

![Project Preview GIF](WEK.gif)

## How to execute (for development)

1. Clone this repository.
2. Open the solution in Visual Studio or your preferred IDE.
3. Build and Run the `WPF_Utils` project.

> NOTE: Make sure you have at least the **.NET Desktop Runtime** installed. If you're using just the tool itself, the necessary runtime will be included in the published version.

## Dependencies (Optional)

- **[Xabe.FFmpeg](https://ffmpeg.xabe.net/)**: A great .NET wrapper for FFmpeg used in the "Video to GIF" functionality.
- **[Xabe.FFmpeg.Downloader](https://ffmpeg.xabe.net/)**: Handles downloading the FFmpeg binaries if not locally found.
