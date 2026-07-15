[![codecov](https://codecov.io/gh/StevanFreeborn/bgr/graph/badge.svg?token=5qp2LpvOZA)](https://codecov.io/gh/StevanFreeborn/bgr)
[![Pull Request](https://github.com/StevanFreeborn/bgr/actions/workflows/pull_request.yml/badge.svg)](https://github.com/StevanFreeborn/bgr/actions/workflows/pull_request.yml)
[![Publish](https://github.com/StevanFreeborn/bgr/actions/workflows/publish.yml/badge.svg)](https://github.com/StevanFreeborn/bgr/actions/workflows/publish.yml)
![GitHub Release](https://img.shields.io/github/v/release/StevanFreeborn/bgr?display_name=tag)
![GitHub License](https://img.shields.io/github/license/StevanFreeborn/bgr)
[![semantic-release: angular](https://img.shields.io/badge/semantic--release-angular-e10079?logo=semantic-release)](https://github.com/semantic-release/semantic-release)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/StevanFreeborn/bgr/total)

# BGR

This is a background removal app that uses machine learning models to remove the background from an image. The app is available for download [here](https://github.com/StevanFreeborn/bgr/releases). You'll just need to unzip the files, place them in a directory, and add it to your PATH. You can rename the executable to your preferred name. The app can also be built from source using the [.NET SDK](https://dotnet.microsoft.com/download). If you'd like to read more about the idea and initial development behind the app you can read the blog post I wrote about it [here](https://blog.stevanfreeborn.com/building-a-background-removal-app-with-machine-learning-and-dotnet).

## Usage

The app is meant to be run from the command line. For example:

```pwsh
bgr /path/to/image.jpg --output /path/to/output.png
```

You can find the full list of commands, arguments, and options using the `--help` option:

```pwsh
bgr --help
```

### Examples

#### Input Image

![Input Image](examples/input.png)

#### Output Image

![Output Image](examples/output.png)

### Feathering Options

By default, BGR applies feathering to create smooth transitions between the foreground and background, reducing harsh edges and halos. You can control the feathering range using two options:

- `--feather-min` - Mask values below this threshold become fully transparent. Default: `70`
- `--feather-max` - Mask values above this threshold become fully opaque. Default: `117`

Values between `feather-min` and `feather-max` are linearly scaled to produce partial transparency, creating a smooth gradient at edges.

```pwsh
bgr /path/to/image.jpg --feather-min 60 --feather-max 130
```

A wider range (e.g., `50` to `150`) produces softer edges, while a narrower range (e.g., `90` to `100`) produces sharper edges.

## Issues

If you encounter any issues while using the app, please open an issue on the repository. If you have any suggestions or feature requests, feel free to open an issue as well.

## Contributing

If you'd like to contribute to the project, feel free to fork the repository and submit a pull request. If you have any questions or suggestions, feel free to open an issue.
