using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents a stored playlist in the list of playlists.
/// </summary>
public sealed class PlaylistItemViewModel : ViewModelBase
{
	private string _name;
	private int _entryCount;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistItemViewModel"/> class.
	/// </summary>
	/// <param name="id">The identifier of the stored playlist.</param>
	/// <param name="name">The name of the playlist.</param>
	/// <param name="entryCount">The number of entries in the playlist.</param>
	public PlaylistItemViewModel(int id, string name, int entryCount)
	{
		Id = id;
		_name = name;
		_entryCount = entryCount;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistItemViewModel"/> class.
	/// </summary>
	/// <param name="summary">The summary of the stored playlist.</param>
	public PlaylistItemViewModel(PlaylistSummaryResponse summary)
		: this(summary.Id, summary.Name, summary.EntryCount)
	{ }

	/// <summary>
	/// Gets the identifier of the stored playlist.
	/// </summary>
	public int Id { get; }

	/// <summary>
	/// Gets the name of the playlist.
	/// </summary>
	public string Name
	{
		get => _name;
		private set => SetProperty(ref _name, value);
	}

	/// <summary>
	/// Gets the number of entries in the playlist.
	/// </summary>
	public int EntryCount
	{
		get => _entryCount;
		private set => SetProperty(ref _entryCount, value);
	}

	/// <summary>
	/// Updates the item after the playlist has been saved.
	/// </summary>
	/// <param name="name">The saved name.</param>
	/// <param name="entryCount">The saved number of entries.</param>
	public void Update(string name, int entryCount)
	{
		Name = name;
		EntryCount = entryCount;
	}
}