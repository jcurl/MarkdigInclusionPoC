namespace RJCP.MarkDigTest
{
    using Markdig;
    using Markdig.Renderers;
    using Markdig.Renderers.Html;
    using Markdig.Syntax;

    public class InclusionBlockRenderer : HtmlObjectRenderer<InclusionBlock>
    {
        private readonly MarkdownPipeline _pipeline;

        public InclusionBlockRenderer(MarkdownPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        protected override void Write(HtmlRenderer renderer, InclusionBlock inclusion)
        {
            if (!inclusion.Loaded)
            {
                renderer.Write(inclusion.GetRawToken());
            }
            else
            {
                foreach (Block block in inclusion)
                {
                    renderer.Write(block);
                }
            }
        }
    }
}
