using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Sidhe.Utilities
{
    public static class Helpers
    {
        public static bool IsHexadecimal(this string text)
            => text?.All(c => ('0' <= c && c <= '9') || ('A' <= c && c <= 'F') || ('a' <= c && c <= 'f')) == true;

        [UsedImplicitly]
        public static TEnum Parse<TEnum>(this string token, TEnum fallback, bool ignoreCase = true) where TEnum : struct
            => (token != null && Enum.TryParse<TEnum>(token, ignoreCase, out var parsed)) ? parsed : fallback;


        [NotNull, UsedImplicitly]
        public static string ToWords(this Enum token)
        {
            var buffer = new StringBuilder();
            
            foreach (var character in token.ToString())
            {
                if (char.IsUpper(character) && buffer.Length > 0 && buffer[^1] != ' ' && !char.IsUpper(buffer[^1]))
                {
                    buffer.Append(' ');
                }

                buffer.Append(character);
            }

            return buffer.ToString();
        }
    }
}
