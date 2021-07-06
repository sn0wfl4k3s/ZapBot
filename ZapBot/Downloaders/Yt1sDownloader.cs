using Microsoft.Edge.SeleniumTools;

namespace ZapBot.Downloaders
{
    public class Yt1sDownloader : Downloader
    {
        public override string UrlPage => "https://yt1s.com/youtube-to-mp3/pt";
        public override string TextInputId => "s_input";
        public override string FormatButtonId => "btn-action";
        public override string ButtonDownloadId => "asuccess";

        public Yt1sDownloader(EdgeDriver driver) : base(driver)
        {
        }
    }
}
