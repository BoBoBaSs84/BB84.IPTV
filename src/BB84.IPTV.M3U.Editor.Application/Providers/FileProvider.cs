using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Providers;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Providers;

/// <summary>
/// Represents a provider for file-related operations. This class serves as
/// a wrapper around the <see cref="File"/> class, providing an abstraction
/// layer for file operations within the application.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This class is a simple wrapper around the System.IO.File class.")]
[GenerateAbstraction(typeof(File), typeof(IFileProvider), typeof(FileProvider))]
internal sealed partial class FileProvider : IFileProvider
{ }
