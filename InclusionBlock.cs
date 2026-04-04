// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RJCP.MarkDigTest
{
    using System;
    using System.IO;
    using Markdig;
    using Markdig.Parsers;
    using Markdig.Syntax;

    public class InclusionBlock : ContainerBlock
    {
        public string Title { get; set; }

        public string IncludedFilePath { get; set; }

        public string ResolvedFilePath { get; set; }

        public bool Loaded { get; set; }

        public string GetRawToken() => $"[!INCLUDE[{Title}]({IncludedFilePath})]";

        public InclusionBlock(BlockParser parser) : base(parser)
        {

        }

        public void Load(MarkdownPipeline pipeline)
        {
            if (Loaded)
            {
                return;
            }

            try
            {
                using (var path = InclusionFiles.PushDependency(IncludedFilePath))
                {
                    ResolvedFilePath = path.FilePath;
                    string content = ReadFile(ResolvedFilePath);
                    MarkdownDocument document = Markdown.Parse(content, pipeline);

                    // A foreach() won't work as expected, skipping some blocks.
                    while (document.Count > 0)
                    {
                        Block block = document[0];
                        document.RemoveAt(0);
                        this.Add(block);
                    }
                    Loaded = true;
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        private static string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
