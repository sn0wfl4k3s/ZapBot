using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

// https://docs.microsoft.com/pt-br/microsoft-edge/webdriver-chromium/?tabs=c-sharp
namespace ZapBot
{
    class Program
    {
        static readonly string Conversa = "Pai";

        static void Main(string[] args)
        {
            int horas = 3;
            ExecuteUntilWorks(delegate ()
            {
                Mensagem($"Pegar as mensagens de quantas horas atrás? ({horas} horas padrão)", pressEnter: false, clear: true);
                Mensagem("Digite a quantidade de horas atrás: ", pressEnter: false, breakline: false);
                string answer = Console.ReadLine();
                if (!string.IsNullOrEmpty(answer) && !string.IsNullOrWhiteSpace(answer))
                    horas = int.Parse(answer);
            });
            string alert = $"Pegando as mensagens de {horas} hora{(horas > 1 ? "s" : string.Empty)} atrás...";
            Mensagem(alert, ConsoleColor.Green, pressEnter: false);
            var options = new EdgeOptions
            {
                UseChromium = true,
                AcceptInsecureCertificates = true,
                PageLoadStrategy = PageLoadStrategy.Eager,
                UnhandledPromptBehavior = UnhandledPromptBehavior.Ignore,
            };
            string userDataDir2 = $@"C:\Users\{Environment.UserName}\AppData\Local\Microsoft\Edge\ZapBot";
            options.AddArgument($@"--user-data-dir={userDataDir2}");
            using var driver = new EdgeDriver(options);
            driver.Navigate().GoToUrl("https://web.whatsapp.com/");
            try
            {
                Mensagem("Scanneie o código QR se necessário...", ConsoleColor.Green, pressEnter: false);

                ExecuteUntilWorks(delegate ()
                {
                    var pesquisa = driver.FindElement(By.CssSelector("#side > div.SgIJV > div > div"));
                    var escaneado = pesquisa.Text.Contains("Pesquisar ou começar");
                    Pause(.6f);
                });

                Mensagem("Verificando as mensagens...", ConsoleColor.Green, pressEnter: false);

                ExecuteUntilWorks(delegate ()
                {
                    var conversa = driver.FindElementsByTagName("span").First(t => t.Text.Contains(Conversa, StringComparison.InvariantCultureIgnoreCase));
                    conversa.Click();
                });

                var actions = new Actions(driver);

                ExecuteUntilWorks(delegate ()
                {
                    var mensagemAntiga = driver.FindElements(By.ClassName("message-in")).First();
                    actions.MoveToElement(mensagemAntiga);
                    actions.Perform();
                    Pause(.5f);
                    mensagemAntiga = driver.FindElements(By.ClassName("message-in")).First();
                    actions.MoveToElement(mensagemAntiga);
                    actions.Perform();
                    Pause(.5f);
                });

                var messages = driver
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
                        return (date, title, link: tagsA.Last().Text);
                    })
                    .Where(m => DateTime.Now.AddHours(-horas) < m.date)
                    .ToImmutableArray();

                if (messages.Length == 0)
                {
                    Mensagem("Nenhum link do youtube foi enviado ainda.", ConsoleColor.Red, false);
                    return;
                }

                Mensagem($"Foram encontrados {messages.Length} links...", ConsoleColor.Green, pressEnter: false);
                for (int i = 0; i < messages.Length; ++i)
                {
                    int tamanhoMaxTitulo = 40;
                    string tituloFormatado = messages[i].title.Length > tamanhoMaxTitulo ?
                        $"{messages[i].title.Substring(0, tamanhoMaxTitulo)}..." :
                        messages[i].title;
                    string tituloVideo = $"- {tituloFormatado}";
                    Mensagem(tituloVideo, ConsoleColor.DarkCyan, pressEnter: false);
                }

                var pages = new List<string>();
                for (int i = 0; i < messages.Length; ++i)
                {
                    driver.ExecuteScript("window.open()");
                    Pause(.2f);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Pause(.5f);
                    driver.Navigate().GoToUrl("https://yt1s.com/youtube-to-mp3/pt");
                    Pause(.5f);
                    pages.Add(driver.CurrentWindowHandle);
                }

                var pagesArrays = pages.ToImmutableArray();
                for (int i = 0; i < messages.Length; ++i)
                {
                    ExecuteUntilWorks(delegate ()
                    {
                        driver.SwitchTo().Window(pagesArrays[i]);
                        Pause(.5f);
                        var input = driver.FindElement(By.Id("s_input"));
                        Pause(1);
                        input.Click();
                        Pause(.5f);
                        input.SendKeys(messages[i].link);
                        Pause(.5f);
                        input.SendKeys(Keys.Enter);
                    });
                }

                Pause(1);

                var inicioDownloads = DateTime.Now;

                for (int i = 0; i < messages.Length; ++i)
                {
                    ExecuteUntilWorks(delegate ()
                    {
                        driver.SwitchTo().Window(pagesArrays[i]);
                        Pause(.7f);
                        var baixar = driver.FindElement(By.XPath("//*[@id=\"asuccess\"]"));
                        Pause(1);
                        baixar.Click();
                    });
                }

                Mensagem("Esperando os downloads terminarem...", ConsoleColor.Green, pressEnter: false);

                string[] titulos = messages.Select(m => m.title.Trim()).ToArray();
                bool mp3Valido(FileInfo f) => ".mp3".Equals(f.Extension)
                    && inicioDownloads <= f.CreationTime
                    && f.Name.StartsWith("yt1s.com");
                string downloadPath = Path.Combine("C:", "Users", Environment.UserName, "Downloads");
                int quantidadeDeMp3Baixado = 0;
                do
                {
                    Pause(2);
                    quantidadeDeMp3Baixado = Directory
                        .GetFiles(downloadPath)
                        .Select(f => new FileInfo(f))
                        .Count(mp3Valido);
                } while (quantidadeDeMp3Baixado < messages.Length);

                Mensagem("Downloads concluídos com sucesso.", ConsoleColor.Green, pressEnter: false);
                Pause(1);

                driver.Quit();

                Mensagem("Esperando plugar o mp3 ou pendrive...", pressEnter: false);
                string[] unidades = { "D:", "E:", "F:", "G:" };
                bool temPendrivePlugado = false;
                while (!temPendrivePlugado)
                {
                    temPendrivePlugado = unidades.Any(u => Directory.Exists(u));
                    Pause(0.3f);
                }
                string unidade = unidades.First(u => Directory.Exists(u));
                Mensagem($"Pendrive Plugado em {unidade}", ConsoleColor.Green, pressEnter: false);
                string pergunta = $"Deseja apagar todos os audios da unidade {unidade}? [ s ou n (padrão) ] : ";
                Mensagem(pergunta, pressEnter: false, breakline: false);
                string resposta = Console.ReadLine();
                if (resposta.Contains("s"))
                {
                    var musicasParaApagar = Directory
                        .GetFiles(unidade)
                        .Where(f => ".mp3".Equals(Path.GetExtension(f)))
                        .ToImmutableArray();
                    for (int i = 0; i < musicasParaApagar.Length; ++i)
                    {
                        try
                        {
                            File.Delete(musicasParaApagar[i]);
                        }
                        catch (Exception e)
                        {
                            Mensagem($"Erro ao deletar o {Path.GetFileName(musicasParaApagar[i])}", ConsoleColor.Red, pressEnter: false);
                            Mensagem($"Mensagem de erro: {e.Message}", ConsoleColor.Red, pressEnter: false);
                        }
                    }
                    Mensagem($"Músicas apagadas...", ConsoleColor.Green, pressEnter: false);
                }
                Mensagem($"Transferindo para a unidade {unidade} os arquivos...", ConsoleColor.Green, pressEnter: false);

                var musicas = Directory
                    .GetFiles(downloadPath)
                    .Select(f => new FileInfo(f))
                    .Where(mp3Valido)
                    .ToImmutableArray();

                Mensagem($"Transferindo {musicas.Length} arquivos...", ConsoleColor.Green, pressEnter: false);
                for (int i = 0; i < musicas.Length; ++i)
                {
                    try
                    {
                        File.Copy(musicas[i].FullName, Path.Combine(unidade, musicas[i].Name));
                    }
                    catch (Exception e)
                    {
                        Mensagem($"Erro ao transferir o {musicas[i].Name}", ConsoleColor.Red, pressEnter: false);
                        Mensagem($"Mensagem de erro: {e.Message}", ConsoleColor.Red, pressEnter: false);
                    }
                }

                Mensagem($"Arquivos transferidos com sucesso. =)", ConsoleColor.Green, pressEnter: false);
            }
            catch (Exception e)
            {
                Mensagem($"{e.Source}: {e.Message}", ConsoleColor.Red);
            }
            finally
            {
                driver.Quit();
            }
        }

        static void Pause(float segundos) => Thread.Sleep((int)segundos * 1000);
        static void Mensagem(string mensagem, ConsoleColor color = ConsoleColor.Cyan, bool pressEnter = true, bool breakline = true, bool clear = false)
        {
            Console.Beep();
            if (clear)
                Console.Clear();
            Console.ResetColor();
            Console.ForegroundColor = color;
            if (breakline)
                Console.WriteLine(mensagem);
            else
                Console.Write(mensagem);
            Console.ResetColor();
            if (pressEnter)
                Console.ReadKey();
        }
        static void ExecuteUntilWorks(Action action)
        {
            bool isOk;
            do
            {
                try
                {
                    action.Invoke();
                    isOk = true;
                }
                catch
                {
                    isOk = false;
                }
            } while (!isOk);
        }
    }
}
