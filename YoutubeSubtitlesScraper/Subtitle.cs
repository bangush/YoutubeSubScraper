using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeSubtitlesScraper
{
    class Subtitle
    {
        public string Id { get; set; }
        public string LangCode { get; set; }
        public string LangOriginal { get; set; }
        public string LangTranslated { get; set; }

        public override string ToString()
        {
            return $"{LangOriginal} ({LangTranslated})";
        }
    }
}
