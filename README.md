# BGR

This is a background removal app that uses machine learning models to remove the background from an image. The app is available for download as a single file executable [here](). The app can also be built from source using the [.NET SDK](https://dotnet.microsoft.com/download). If you'd like to read more about the idea and initial development behind the app you can read the blog post I wrote about it [here]().

## Usage

The app is meant to be run from the command line. For example:

```pwsh
bgr /path/to/image.jpg --output /path/to/output.jpg
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

## Issues

If you encounter any issues while using the app, please open an issue on the repository. If you have any suggestions or feature requests, feel free to open an issue as well.

## Contributing

If you'd like to contribute to the project, feel free to fork the repository and submit a pull request. If you have any questions or suggestions, feel free to open an issue.
