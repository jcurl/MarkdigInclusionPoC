// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RJCP.MarkDigTest
{
    using System.Linq;
    using Markdig;
    using Markdig.Renderers;
    using Markdig.Syntax;

    public class InclusionExtension : IMarkdownExtension
    {
        private MarkdownPipeline _pipeline;

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline
                .BlockParsers
                .AddIfNotAlready(new InclusionBlockParser());

            pipeline.DocumentProcessed += document =>
            {
                // Pipeline set up when renderer is configured.
                ProcessInclusions(document, _pipeline);
            };
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            _pipeline = pipeline;

            if (renderer is HtmlRenderer htmlRenderer)
            {
                if (!htmlRenderer.ObjectRenderers.Contains<InclusionBlockRenderer>())
                {
                    htmlRenderer.ObjectRenderers.Insert(0, new InclusionBlockRenderer(pipeline));
                }
            }
        }

        private static void ProcessInclusions(MarkdownDocument document, MarkdownPipeline pipeline)
        {
            var inclusions = document.Descendants<InclusionBlock>().ToList();

            foreach (var inclusion in inclusions)
            {
                inclusion.Load(pipeline);
            }
        }
    }
}
