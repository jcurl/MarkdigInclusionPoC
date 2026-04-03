namespace RJCP.MarkDigTest
{
    using System.Threading;
    using Markdig;
    using Markdig.Renderers;

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.

    public class InclusionExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline
                .BlockParsers
                .AddIfNotAlready(new InclusionBlockParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is HtmlRenderer htmlRenderer)
            {
                if (!htmlRenderer.ObjectRenderers.Contains<HtmlInclusionBlockRenderer>())
                {
                    htmlRenderer.ObjectRenderers.Insert(0, new HtmlInclusionBlockRenderer(pipeline));
                }
            }
        }
    }
}
