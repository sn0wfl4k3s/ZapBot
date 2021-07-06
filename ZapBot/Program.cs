using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium;
using System;
using System.Linq;
using ZapBot.Downloaders;

namespace ZapBot
{
    class Program
    {
        static readonly string Conversa = "Pai";

        static void Main(string[] args)
        {
            int hoursAgo = 3;
            Set.ExecuteUntilWorks(delegate ()
            {
                Set.Mensagem($"Pegar as mensagens de quantas horas atrás? ({hoursAgo} horas padrão)", pressEnter: false, clear: true);
                Set.Mensagem("Digite a quantidade de horas atrás: ", pressEnter: false, breakline: false);
                string answer = Console.ReadLine();
                if (!string.IsNullOrEmpty(answer) && !string.IsNullOrWhiteSpace(answer))
                    hoursAgo = int.Parse(answer);
            });
            Set.Mensagem($"Pegando as mensagens de {hoursAgo} hora{(hoursAgo > 1 ? "s" : string.Empty)} atrás...", ConsoleColor.Green, pressEnter: false);
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
            try
            {
                using WhatsAppWeb whatsapp = new(driver);
                whatsapp.Open();
                whatsapp.OpenTalk(Conversa);
                whatsapp.ScrollTalk(2);
                var messages = whatsapp.GetYoutubeLinks(hoursAgo);
                whatsapp.Dispose();

                if (messages.Length is 0)
                {
                    Set.Mensagem("Nenhum link do youtube foi enviado ainda.", ConsoleColor.Red, false);
                    return;
                }

                Set.Mensagem($"Foram encontrados {messages.Length} links...", ConsoleColor.Green, pressEnter: false);
                for (int i = 0; i < messages.Length; ++i)
                {
                    string tituloFormatado = messages[i].Title
                        .Substring(0, messages[i].Title.Length > 37 ? 37 : messages[i].Title.Length)
                        .PadRight(40, '.');
                    Set.Mensagem($"- {tituloFormatado}", ConsoleColor.DarkCyan, pressEnter: false);
                }

                using Downloader downloader = new Yt1sDownloader(driver);
                downloader.DownloadAll(messages.Select(m => m.Link).ToArray());

                using FileManager filemanager = new(downloader.WhenStartDownload);

                downloader.Dispose();

                Set.Mensagem("Esperando os downloads terminarem...", ConsoleColor.Green, pressEnter: false);
                filemanager.WaitDownloadsEnd(messages.Length);
                Set.Mensagem("Downloads concluídos com sucesso.", ConsoleColor.Green, pressEnter: false);

                driver.Quit();

                Set.Mensagem("Esperando plugar o mp3 ou pendrive...", pressEnter: false);
                string device = filemanager.WaitDeviceBePluged();

                Set.Mensagem($"Pendrive Plugado em {device}", ConsoleColor.Green, pressEnter: false);
                Set.Mensagem($"Deseja apagar todos os audios da unidade {device}? [ 's' ou 'n' (padrão) ] : ", pressEnter: false, breakline: false);
                string resposta = Console.ReadLine();
                if (resposta.Contains('s'))
                    filemanager.DeleteAllMp3From(device);

                Set.Mensagem($"Transferindo para a unidade {device} os arquivos...", ConsoleColor.Green, pressEnter: false);
                filemanager.TransfererFilesTo(device);

                Set.Mensagem($"Áudios transferidos com sucesso. =)", ConsoleColor.Green, pressEnter: false);

            }
            catch (Exception e)
            {
                Set.Mensagem($"{e.Source}: {e.Message}", ConsoleColor.Red);
            }
            finally
            {
                driver.Quit();
            }
        }

    }
}
