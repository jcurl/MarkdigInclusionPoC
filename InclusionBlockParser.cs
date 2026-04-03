namespace RJCP.MarkDigTest
{
    using Markdig.Helpers;
    using Markdig.Parsers;

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.

    public class InclusionBlockParser : BlockParser
    {
        private const string StartString = "[!include";

        public InclusionBlockParser()
        {
            // important: if you don't set this, the parser won't be called!
            OpeningCharacters = ['['];
        }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            // stop processing if we're in a code block
            if (processor.IsCodeIndent)
            {
                return BlockState.None;
            }

            // [!include[<title>](<filepath>)]
            int column = processor.Column;
            StringSlice line = processor.Line;
            //string command = line.ToString();

            if (!ExtensionsHelper.MatchStart(ref line, StartString, false))
            {
                return BlockState.None;
            }
            else
            {
                if (line.CurrentChar == '+')
                {
                    line.NextChar();
                }
            }

            string title = null, path = null;

            if (!ExtensionsHelper.MatchLink(ref line, ref title, ref path) || !ExtensionsHelper.MatchInclusionEnd(ref line))
            {
                return BlockState.None;
            }

            while (line.CurrentChar.IsSpaceOrTab()) line.NextChar();
            if (line.CurrentChar != '\0')
            {
                return BlockState.None;
            }

            processor.NewBlocks.Push(new InclusionBlock(this)
            {
                Title = title,
                IncludedFilePath = path,
                Line = processor.LineIndex,
                Column = column,
            });

            return BlockState.BreakDiscard;
        }
    }
}
