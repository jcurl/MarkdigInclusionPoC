namespace RJCP.MarkDigTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Markdig.Helpers;

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.

    public static class ExtensionsHelper
    {
        public static bool MatchStart(ref StringSlice slice, string startString, bool isCaseSensitive = true)
        {
            var c = slice.CurrentChar;
            var index = 0;

            while (c != '\0' && index < startString.Length && CharEqual(c, startString[index], isCaseSensitive))
            {
                c = slice.NextChar();
                index++;
            }

            return index == startString.Length;
        }

        public static bool MatchInclusionEnd(ref StringSlice slice)
        {
            if (slice.CurrentChar != ']')
            {
                return false;
            }

            slice.NextChar();

            return true;
        }

        public static bool MatchLink(ref StringSlice slice, ref string title, ref string path)
        {
            if (MatchTitle(ref slice, ref title) && MatchPath(ref slice, ref path))
            {
                return true;
            }

            return false;
        }

        private static bool MatchTitle(ref StringSlice slice, ref string title)
        {
            SkipSpace(ref slice);

            if (slice.CurrentChar != '[')
            {
                return false;
            }

            var c = slice.NextChar();
            var str = StringBuilderCache.Local();
            var hasEscape = false;

            while (c != '\0' && (c != ']' || hasEscape))
            {
                if (c == '\\' && !hasEscape)
                {
                    hasEscape = true;
                }
                else
                {
                    str.Append(c);
                    hasEscape = false;
                }
                c = slice.NextChar();
            }

            if (c == ']')
            {
                title = str.ToString().Trim();
                slice.NextChar();

                return true;
            }

            return false;
        }

        private static bool MatchPath(ref StringSlice slice, ref string path)
        {
            if (slice.CurrentChar != '(')
            {
                return false;
            }

            slice.NextChar();
            SkipWhitespace(ref slice);

            string includedFilePath;
            if (slice.CurrentChar == '<')
            {
                includedFilePath = TryGetStringBeforeChars([')', '>'], ref slice, breakOnWhitespace: true);
            }
            else
            {
                includedFilePath = TryGetStringBeforeChars([')'], ref slice, breakOnWhitespace: true);
            }

            if (includedFilePath is null)
            {
                return false;
            }

            if (includedFilePath.Length >= 1 && includedFilePath.First() == '<' && slice.CurrentChar == '>')
            {
                includedFilePath = includedFilePath[1..].Trim();
            }

            if (slice.CurrentChar == ')')
            {
                path = includedFilePath;
                slice.NextChar();
                return true;
            }
            else
            {
                var title = TryGetStringBeforeChars([')'], ref slice, breakOnWhitespace: false);
                if (title is null)
                {
                    return false;
                }
                else
                {
                    path = includedFilePath;
                    slice.NextChar();
                    return true;
                }
            }
        }

        private static void SkipSpace(ref StringSlice slice)
        {
            while (slice.CurrentChar == ' ')
            {
                slice.NextChar();
            }
        }

        private static void SkipWhitespace(ref StringSlice slice)
        {
            var c = slice.CurrentChar;
            while (c != '\0' && c.IsWhitespace())
            {
                c = slice.NextChar();
            }
        }
        private static bool CharEqual(char ch1, char ch2, bool isCaseSensitive)
        {
            return isCaseSensitive ? ch1 == ch2 : char.ToLower(ch1) == char.ToLower(ch2);
        }

        private static string TryGetStringBeforeChars(IReadOnlyList<char> chars, ref StringSlice slice, bool breakOnWhitespace = false)
        {
            StringSlice savedSlice = slice;
            var c = slice.CurrentChar;
            var hasEscape = false;
            var builder = StringBuilderCache.Local();

            while (c != '\0' && (!breakOnWhitespace || !c.IsWhitespace()) && (hasEscape || !chars.Contains(c)))
            {
                if (c == '\\' && !hasEscape)
                {
                    hasEscape = true;
                }
                else
                {
                    builder.Append(c);
                    hasEscape = false;
                }
                c = slice.NextChar();
            }

            if (c == '\0' && !chars.Contains('\0'))
            {
                slice = savedSlice;
                builder.Length = 0;
                return null;
            }
            else
            {
                var result = builder.ToString().Trim();
                builder.Length = 0;
                return result;
            }
        }
    }
}
