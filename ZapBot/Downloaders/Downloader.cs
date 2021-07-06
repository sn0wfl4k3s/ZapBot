using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ZapBot.Downloaders
{
    public abstract class Downloader : IDisposable
    {
        private readonly IWebDriver _driver;

        private DateTime _whenStartDownload;

        public abstract string TextInputId { get; }
        public abstract string FormatButtonId { get; }
        public abstract string ButtonDownloadId { get; }
        public abstract string UrlPage { get; }

        public DateTime WhenStartDownload => _whenStartDownload;

        protected Downloader(IWebDriver driver)
        {
            _driver = driver;
        }

        public void DownloadAll(string[] urls)
        {
            var pages = new List<string>();
            for (int i = 0; i < urls.Length; ++i)
            {
                (_driver as IJavaScriptExecutor).ExecuteScript("window.open()");
                Set.Pause(.2f);
                _driver.SwitchTo().Window(_driver.WindowHandles.Last());
                Set.Pause(.5f);
                _driver.Navigate().GoToUrl(UrlPage);
                Set.Pause(.5f);
                pages.Add(_driver.CurrentWindowHandle);
            }

            var pagesArrays = pages.ToImmutableArray();
            for (int i = 0; i < urls.Length; ++i)
            {
                Set.ExecuteUntilWorks(delegate ()
                {
                    _driver.SwitchTo().Window(pagesArrays[i]);
                    Set.Pause(.5f);
                    var input = _driver.FindElement(By.Id(TextInputId));
                    Set.Pause(1);
                    input.Click();
                    Set.Pause(.5f);
                    input.SendKeys(urls[i]);
                    Set.Pause(.5f);
                    input.SendKeys(Keys.Enter);
                });
            }

            Set.Pause(1);


            if (!string.IsNullOrEmpty(FormatButtonId))
            {
                for (int i = 0; i < urls.Length; ++i)
                {
                    _driver.SwitchTo().Window(pagesArrays[i]);
                    Set.ExecuteUntilWorks(delegate ()
                    {
                        Set.Pause(.7f);
                        var format = _driver.FindElement(By.Id(FormatButtonId));
                        Set.Pause(1);
                        format.Click();
                        Set.Pause(.7f);
                    });
                }
            }

            _whenStartDownload = DateTime.Now;
            int tryNumber = 0;

            for (int i = 0; i < urls.Length; ++i)
            {
                _driver.SwitchTo().Window(pagesArrays[i]);
                Set.ExecuteUntilWorks(delegate ()
                {
                    tryNumber++;
                    if (tryNumber > 100)
                        throw new ApplicationException(nameof(tryNumber));
                    Set.Pause(.7f);
                    var baixar = _driver.FindElement(By.Id(ButtonDownloadId));
                    Set.Pause(1);
                    baixar.Click();
                    Set.Pause(.7f);
                });
            }
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
