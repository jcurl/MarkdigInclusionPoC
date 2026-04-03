namespace RJCP.MarkDigTest
{
    using System;
    using System.IO;
    using System.Reflection.Metadata;

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.

    using Markdig;
    using Markdig.Renderers;
    using Markdig.Renderers.Html;
    using Markdig.Syntax;

    public class HtmlInclusionBlockRenderer : HtmlObjectRenderer<InclusionBlock>
    {
        private readonly MarkdownPipeline _pipeline;

        public HtmlInclusionBlockRenderer(MarkdownPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        protected override void Write(HtmlRenderer renderer, InclusionBlock inclusion)
        {
            string includeFilePath;
            try
            {
                includeFilePath = InclusionFiles.PushDependency(inclusion.IncludedFilePath);
                try
                {
                    string content = ReadFile(includeFilePath);
                    renderer.Write(ToMaml(content, _pipeline));
                }
                finally
                {
                    InclusionFiles.Pop();
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                renderer.Write(inclusion.GetRawToken());
                return;
            }
        }

        private static string ToMaml(string content, MarkdownPipeline pipeline)
        {
            var document = Markdown.Parse(content, pipeline);
            var renderer = new MamlTopicRenderer(false, new StringWriter(), true);
            pipeline.Setup(renderer);
            renderer.Id = null;
            renderer.Render(document);
            renderer.Writer.Flush();

            return renderer.Writer.ToString()!;
        }

        private static string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}

