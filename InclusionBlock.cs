namespace RJCP.MarkDigTest
{
    using Markdig.Parsers;
    using Markdig.Syntax;

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.

    public class InclusionBlock : ContainerBlock
    {
        public string Title { get; set; }

        public string IncludedFilePath { get; set; }

        public object ResolvedFilePath { get; set; }

        public string GetRawToken() => $"[!include[{Title}]({IncludedFilePath})]";

        public InclusionBlock(BlockParser parser) : base(parser)
        {

        }
    }
}
