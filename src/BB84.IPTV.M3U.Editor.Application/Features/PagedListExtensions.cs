namespace BB84.IPTV.M3U.Editor.Application.Features;

/// <summary>
/// Provides the helpers that turn a sequence into a <see cref="PagedList{T}"/>.
/// </summary>
public static class PagedListExtensions
{
	/// <summary>
	/// Creates the requested page of <paramref name="source"/>, which is enumerated once.
	/// </summary>
	/// <typeparam name="T">The type of the items.</typeparam>
	/// <param name="source">All items to page through.</param>
	/// <param name="pageNumber">The page number, the first page is page one.</param>
	/// <param name="pageSize">The page size.</param>
	/// <returns>The requested page with the metadata of the whole sequence.</returns>
	public static PagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
	{
		ArgumentNullException.ThrowIfNull(source);

		List<T> items = [.. source];

		return new PagedList<T>(items.Skip((pageNumber - 1) * pageSize).Take(pageSize), items.Count, pageNumber, pageSize);
	}

	/// <summary>
	/// Creates the requested page from the <paramref name="parameters"/> of a request.
	/// </summary>
	/// <typeparam name="T">The type of the items.</typeparam>
	/// <param name="source">All items to page through.</param>
	/// <param name="parameters">The request the page number and the page size are taken from.</param>
	/// <returns>The requested page with the metadata of the whole sequence.</returns>
	public static PagedList<T> ToPagedList<T>(this IEnumerable<T> source, Parameters parameters)
	{
		ArgumentNullException.ThrowIfNull(parameters);

		return source.ToPagedList(parameters.PageNumber, parameters.PageSize);
	}
}