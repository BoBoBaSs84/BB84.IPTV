using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

public sealed class GuideEntity : EntityBase
{
	public string? Channel { get; set; }
	public string? Feed { get; set; }
	public string Site { get; set; }
	public string SiteId { get; set; }
	public string SiteName { get; set; }
	public string Lang { get; set; }
}
