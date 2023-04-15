using System;
using System.Collections.Generic;

namespace WpfBlazor.Data
{
    public class Movie
    {
        public string title { get; set; }
        public string num { get; set; }
        public string seller { get; set; }
        public List<string> tags { get; set; }
        public string file { get; set; }
        public string cover { get; set; }
        public string fanart { get; set; }
        public string website { get; set; }
        public string nfo { get; set; }
        public Movie(string title, string num, string seller, List<string> tags, string file, string cover, string fanart, string website, string nfo)
        {
            this.title = title ?? throw new ArgumentNullException(nameof(title));
            this.num = num ?? throw new ArgumentNullException(nameof(num));
            this.seller = seller ?? throw new ArgumentNullException(nameof(seller));
            this.tags = tags ?? throw new ArgumentNullException(nameof(tags));
            this.file = file ?? throw new ArgumentNullException(nameof(file));
            this.cover = cover ?? throw new ArgumentNullException(nameof(cover));
            this.fanart = fanart ?? throw new ArgumentNullException(nameof(fanart));
            this.website = website ?? throw new ArgumentNullException(nameof(website));
            this.nfo = nfo ?? throw new ArgumentNullException(nameof(nfo));
        }
    }
}
