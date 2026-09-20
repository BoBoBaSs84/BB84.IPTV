namespace BB84.IPTV.M3U.Editor.Application.Features;

/// <summary>
/// The base request parameter class.
/// </summary>
public abstract class Parameters
{
	/// <summary>
	/// The smallest allowed page size.
	/// </summary>
	public const int MinPageSize = 100;

	/// <summary>
	/// The largest allowed page size.
	/// </summary>
	public const int MaxPageSize = 1000;

	private int _pageNumber = 1;
	private int _pageSize = MaxPageSize;

	/// <summary>
	/// The page number property.
	/// </summary>
	/// <remarks>
	/// The first page is page one, a smaller value is raised to it.
	/// </remarks>
	public int PageNumber
	{
		get => _pageNumber;
		set => _pageNumber = Math.Max(1, value);
	}

	/// <summary>
	/// The desired page size.
	/// </summary>
	/// <remarks>
	/// Kept between <see cref="MinPageSize"/> and <see cref="MaxPageSize"/>, so a page is never
	/// empty by accident. The default value is <see cref="MaxPageSize"/>.
	/// </remarks>
	public int PageSize
	{
		get => _pageSize;
		set => _pageSize = Math.Clamp(value, MinPageSize, MaxPageSize);
	}

	/// <summary>
	/// The number of items to skip to reach the current page.
	/// </summary>
	public int Skip
		=> (PageNumber - 1) * PageSize;
}