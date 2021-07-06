using System;
using System.Threading;

namespace ZapBot
{
    public static class Set
    {
        public static void Pause(float segundos) => Thread.Sleep((int)segundos * 1000);
        public static void Mensagem(string mensagem, ConsoleColor color = ConsoleColor.Cyan, bool pressEnter = true, bool breakline = true, bool clear = false)
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
        public static void ExecuteUntilWorks(Action action)
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
