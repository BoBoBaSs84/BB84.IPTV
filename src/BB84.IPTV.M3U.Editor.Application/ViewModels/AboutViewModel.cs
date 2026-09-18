using System.Reflection;
using System.Runtime.Versioning;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// The view model for the about page.
/// </summary>
public sealed class AboutViewModel : ViewModelBase, INavigateable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AboutViewModel"/> class.
	/// </summary>
	public AboutViewModel()
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		FrameworkName = GetFrameworkName(assembly) ?? string.Empty;
		Repository = GetRepositoryLocation(assembly) ?? string.Empty;
	}

	/// <summary>
	/// Gets the title of the about page, which is the product name from the about model.
	/// </summary>
	public string Title { get; } = AssemblyInformation.Product;

	/// <summary>
	/// Gets the description of the about page, which is the description from the about model.
	/// </summary>
	public string Description { get; } = AssemblyInformation.Description;

	/// <summary>
	/// Gets the version of the about page, which is the version from the about model.
	/// </summary>
	public string Version { get; } = AssemblyInformation.Version;

	/// <summary>
	/// Gets the company name of the about page, which is the company from the about model.
	/// </summary>
	public string Company { get; } = AssemblyInformation.Company;

	/// <summary>
	/// Gets the copyright information of the about page, which is the copyright from the about model.
	/// </summary>
	public string Copyright { get; } = AssemblyInformation.Copyright;

	/// <summary>
	/// Gets the framework name of the about page, which is the framework name from the about model.
	/// </summary>
	public string FrameworkName { get; }

	/// <summary>
	/// Gets the repository URL of the about page, which is the repository from the about model.
	/// </summary>
	public string Repository { get; }

	private static string? GetFrameworkName(Assembly assembly)
	{
		TargetFrameworkAttribute? targetFramework = assembly
			.GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
			.SingleOrDefault() as TargetFrameworkAttribute;

		return targetFramework is not null ? targetFramework.FrameworkName : default;
	}

	private static string? GetRepositoryLocation(Assembly assembly)
	{
		IEnumerable<AssemblyMetadataAttribute>? assemblyMetadata = assembly
			.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false) as IEnumerable<AssemblyMetadataAttribute>;

		return assemblyMetadata?.Where(x => x.Key == "RepositoryUrl").Select(x => x.Value).SingleOrDefault();
	}
}
