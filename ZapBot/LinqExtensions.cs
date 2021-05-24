using System;
using System.Collections.Generic;
using System.Linq;

namespace ZapBot
{
    static class LinqExtensions
    {
        public static IEnumerable<T> TryWhere<T>(this IEnumerable<T> elements, Func<T, bool> function)
        {
            return elements.Where(element => 
            {
                try
                {
                    return function.Invoke(element);
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}
