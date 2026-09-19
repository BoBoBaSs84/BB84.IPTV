using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;

namespace BB84.IPTV.M3U.Editor.Services;

/// <summary>
/// The user service implementation.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class UserService : IUserService
{
	/// <inheritdoc/>
	public string Name => Environment.UserName;

	/// <inheritdoc/>
	public string Domain => Environment.UserName;

	/// <inheritdoc/>
	public string Machine => Environment.MachineName;
}
