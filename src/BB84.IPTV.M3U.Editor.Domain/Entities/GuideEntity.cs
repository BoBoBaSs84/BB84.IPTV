using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

public sealed class GuideEntity : EntityBase
{
	public string? Channel { get; set; }
	public string? Feed { get; set; }
	public required string Site { get; set; }
	public required string SiteId { get; set; }
	public required string SiteName { get; set; }
	public required string Lang { get; set; }
}
