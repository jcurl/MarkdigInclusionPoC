namespace RJCP.MarkDigTest
{
    using System.IO;
    using Markdig;
    using Markdig.Parsers;
    using Markdig.Syntax;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;

    [TestFixture]
    public class MarkdownExtensionsTest
    {
        [Test]
        public void LoadRoot()
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UsePipeTables()
                .Use(new InclusionExtension())
                .Build();
            var renderer = new MamlTopicRenderer(false, new StringWriter());
            pipeline.Setup(renderer);

            string path = Path.Combine(Deploy.WorkDirectory, "TestResources", "Root.md");

            InclusionFiles.PushFile(path);
            string text = File.ReadAllText(path);
            MarkdownDocument document = MarkdownParser.Parse(text, pipeline);

            renderer.Id = "topic-identifier";
            renderer.Render(document);
            renderer.Writer.Flush();

            var result = renderer.Writer.ToString()!;

            renderer.ResetRenderer();
        }
    }
}
