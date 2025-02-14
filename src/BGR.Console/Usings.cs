global using System.ComponentModel;
global using System.Reflection;

global using BGR.Console.Common;
global using BGR.Console.Logging;
global using BGR.Console.Removal;
global using BGR.Console.Removal.ImageSharp;
global using BGR.Console.Removal.Models;
global using BGR.Console.Removal.Onnx;
global using BGR.Console.Resources;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.ML.OnnxRuntime;
global using Microsoft.ML.OnnxRuntime.Tensors;

global using Serilog;
global using Serilog.Events;
global using Serilog.Formatting.Compact;

global using SixLabors.ImageSharp;
global using SixLabors.ImageSharp.Formats.Png;
global using SixLabors.ImageSharp.PixelFormats;
global using SixLabors.ImageSharp.Processing;

global using Spectre.Console;
global using Spectre.Console.Cli;