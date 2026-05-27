using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities;

public class Feed
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Language { get; set; }
    public List<FeedItem> Items { get; set; }

    public string Serialize()
    {
        var serializedItems = Items.Select(feedItem => feedItem.Serialize()).ToList();
        
        return $"""
                <rss version="2.0">
                    <channel>
                        <title>{Title}</title>
                        <description>{Description}</description>
                        <language>{Language}</language>
                        {(serializedItems.Count > 0 ? serializedItems.Aggregate((x, y) => x + string.Empty + y) : string.Empty)}
                    </channel>
                </rss>
                """;
    }
}
