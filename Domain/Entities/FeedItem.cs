using System;

namespace Domain.Entities;

public class FeedItem
{
    public Guid Guid { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public DateTime PubDate { get; set; }
    public string Link { get; set; }
    
    public string Serialize()
    {
        var description = string.IsNullOrEmpty(Image)
            ? Description
            : $"""<p>{Description}</p> <img src="{Image}" alt="{Title}" title="{Title}" />""";

        return $"""
                <item>
                    <title><![CDATA[{Title}]]></title>
                    <description><![CDATA[{description}]]></description>
                    <link>{Link}</link>
                    <pubDate>{PubDate:R}</pubDate>
                    <guid>{Guid}</guid>
                </item>
                """;
    }
}
