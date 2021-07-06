using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace ZapBot
{
    public class FileManager : IDisposable
    {
        private readonly DateTime _whenDownloadStart;
        private readonly string _downloadPath;

        public FileManager(DateTime whenDownloadStart)
        {
            _whenDownloadStart = whenDownloadStart;
            _downloadPath = Path.Combine("C:", "Users", Environment.UserName, "Downloads");
        }

        bool Mp3Valido(FileInfo f) => ".mp3".Equals(f.Extension)
            && _whenDownloadStart <= f.CreationTime
            //&& f.Name.StartsWith("yt1s.com")
            ;

        public void WaitDownloadsEnd(int expectedCountDownloads)
        {
            int quantidadeDeMp3Baixado = 0;
            while (quantidadeDeMp3Baixado < expectedCountDownloads)
            {
                Set.Pause(2);
                quantidadeDeMp3Baixado = Directory
                    .GetFiles(_downloadPath)
                    .Select(f => new FileInfo(f))
                    .Count(Mp3Valido);
            }
        }

        public void DeleteAllMp3From(string unidade)
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
                    Set.Mensagem($"Erro ao deletar o {Path.GetFileName(musicasParaApagar[i])}", ConsoleColor.Red, pressEnter: false);
                    Set.Mensagem($"Mensagem de erro: {e.Message}", ConsoleColor.Red, pressEnter: false);
                }
            }
            Set.Mensagem($"Músicas apagadas...", ConsoleColor.Green, pressEnter: false);
        }

        public string WaitDeviceBePluged()
        {
            string[] unidades = { "D:", "E:", "F:", "G:" };
            bool temPendrivePlugado = false;
            while (!temPendrivePlugado)
            {
                temPendrivePlugado = unidades.Any(u => Directory.Exists(u));
                Set.Pause(0.3f);
            }
            return unidades.First(u => Directory.Exists(u));
        }

        public void TransfererFilesTo(string unidade)
        {
            var musicas = Directory
                .GetFiles(_downloadPath)
                .Select(f => new FileInfo(f))
                .Where(Mp3Valido)
                .ToImmutableArray();
            for (int i = 0; i < musicas.Length; ++i)
            {
                try
                {
                    Set.Mensagem($"Transferindo: {musicas[i].Name}", ConsoleColor.Green, pressEnter: false);
                    File.Copy(musicas[i].FullName, Path.Combine(unidade, musicas[i].Name));
                }
                catch (Exception e)
                {
                    Set.Mensagem($"Erro ao transferir o {musicas[i].Name}", ConsoleColor.Red, pressEnter: false);
                    Set.Mensagem($"Mensagem de erro: {e.Message}", ConsoleColor.Red, pressEnter: false);
                }
            }
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
