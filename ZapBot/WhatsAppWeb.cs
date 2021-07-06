using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZapBot
{
    public class WhatsAppWeb : IDisposable
    {
        private readonly IWebDriver _driver;

        public WhatsAppWeb(IWebDriver driver)
        {
            _driver = driver;
        }

        public void Open()
        {
            _driver.Navigate().GoToUrl("https://web.whatsapp.com/");
            Set.Mensagem("Scanneie o código QR se necessário...", ConsoleColor.Green, pressEnter: false);
            Set.ExecuteUntilWorks(delegate ()
            {
                var pesquisa = _driver.FindElement(By.CssSelector("#side > div.SgIJV > div > div"));
                var escaneado = pesquisa.Text.Contains("Pesquisar ou começar");
                Set.Pause(.6f);
            });
        }

        public void OpenTalk(string talkname)
        {
            Set.ExecuteUntilWorks(delegate ()
            {
                var conversa = _driver
                    .FindElements(By.TagName("span"))
                    .First(t => t.Text.Contains(talkname, StringComparison.InvariantCultureIgnoreCase));
                Set.Pause(.7f);
                conversa.Click();
            });
        }

        public void ScrollTalk(int count)
        {
            var actions = new Actions(_driver);
            IWebElement oldMessage;
            Set.ExecuteUntilWorks(delegate ()
            {
                for (int i = 0; i < count; ++i)
                {
                    oldMessage = _driver.FindElements(By.ClassName("message-in")).First();
                    actions.MoveToElement(oldMessage);
                    actions.Perform();
                    Set.Pause(.5f);
                }
            });
        }

        public record YoutubeLink(DateTime Date, string Title, string Link);

        public ImmutableArray<YoutubeLink> GetYoutubeLinks(int hoursAgo) => _driver
            .FindElements(By.ClassName("message-in"))
            .TryWhere(m => m.FindElements(By.TagName("a")).Last().Text.Contains("youtu"))
            .Select(m =>
            {
                string dateMessage = m
                    .FindElement(By.ClassName("copyable-text"))
                    .GetAttribute("data-pre-plain-text");
                string dateString = Regex.Replace(dateMessage, @"\[|,|\].*", string.Empty);
                var date = Convert.ToDateTime(dateString);
                var tagsA = m.FindElements(By.TagName("a"));
                string title = Regex.Replace(tagsA.First().Text, @"\-|\(|\)|\r|\n.*", string.Empty);
                return new YoutubeLink(date, title, Link: tagsA.Last().Text);
            })
            .Where(m => DateTime.Now.AddHours(-hoursAgo) < m.Date)
            .ToImmutableArray();


        public void Dispose() => GC.SuppressFinalize(this);
    }
}
