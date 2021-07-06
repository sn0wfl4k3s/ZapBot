using Microsoft.Edge.SeleniumTools;

namespace ZapBot.Downloaders
{
    public class Mp3downyDownloader : Downloader
    {
        public override string UrlPage => "https://mp3downy.com/";
        public override string TextInputId => "txtUrl";
        public override string FormatButtonId => null;
        public override string ButtonDownloadId => "percentageText";


        public Mp3downyDownloader(EdgeDriver driver) : base(driver)
        {
        }
    }
}
