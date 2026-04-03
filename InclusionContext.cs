namespace RJCP.MarkDigTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.

    /// <summary>
    /// Represents a thread static context of the current document.
    /// </summary>
    /// <remarks>
    /// Due to markdig API design, it is not obvious to pass per document information and reusing the
    /// same markdown pipeline instance at the same time.
    /// Thus a thread static <see cref="InclusionContext"/> class is created to store per document information.
    /// </remarks>
    public static class InclusionContext
    {
        private readonly struct ContextInfo
        {
            public ContextInfo(string file, Stack<string> inclusionStack)
            {
                File = file;
                InclusionStack = inclusionStack;
            }

            public string File { get; init; }

            public HashSet<string> Dependencies { get; } = [];

            public Stack<string> InclusionStack { get; init; }
        }

        private static readonly ThreadLocal<Stack<ContextInfo>> t_markupStacks
                          = new(() => new Stack<ContextInfo>());

        /// <summary>
        /// Gets the current file. This is the included file if the engine is currently parsing or rendering an include file.
        /// </summary>
        public static string File
        {
            get
            {
                var markupStack = t_markupStacks.Value;
                return markupStack.Count > 0 ? markupStack.Peek().InclusionStack.Peek() : null;
            }
        }

        /// <summary>
        /// Gets the root file, this is always the first file pushed to the context regardless of file inclusion.
        /// </summary>
        public static string RootFile
        {
            get
            {
                var markupStack = t_markupStacks.Value;
                return markupStack.Count > 0 ? markupStack.Peek().File : null;
            }
        }

        /// <summary>
        /// Whether the content is included by other markdown files.
        /// </summary>
        public static bool IsInclude
        {
            get
            {
                var markupStack = t_markupStacks.Value;
                return markupStack.Count > 0 && markupStack.Peek().InclusionStack.Count > 1;
            }
        }

        /// <summary>
        /// Gets all the dependencies referenced by the root markdown context.
        /// </summary>
        public static IEnumerable<string> Dependencies
        {
            get
            {
                var markupStack = t_markupStacks.Value;
                return markupStack.Count > 0 ? markupStack.Peek().Dependencies : Array.Empty<string>();
            }
        }

        /// <summary>
        /// Creates a scope for calling <see cref="Markdig.Markdown.ToHtml(string, Markdig.MarkdownPipeline?, Markdig.MarkdownParserContext?)"/>.
        /// </summary>
        public static IDisposable PushFile(string file)
        {
            var markupStack = t_markupStacks.Value;
            var inclusionStack = new Stack<string>();
            inclusionStack.Push(file);
            markupStack.Push(new(file, inclusionStack));

            return new DelegatingDisposable(() => markupStack.Pop());
        }

        /// <summary>
        /// Creates a scope for calling <see cref="Markdig.Markdown.ToHtml(string, Markdig.MarkdownPipeline?, Markdig.MarkdownParserContext?)"/>
        /// when processing a markdown inclusion inside <see cref="HtmlInclusionBlockRenderer"/> and <see cref="HtmlInclusionInlineRenderer"/>.
        /// </summary>
        public static IDisposable PushInclusion(string file)
        {
            var inclusionStack = t_markupStacks.Value.Peek().InclusionStack;
            inclusionStack.Push(file);

            return new DelegatingDisposable(() => inclusionStack.Pop());
        }

        /// <summary>
        /// Push dependency
        /// </summary>
        public static void PushDependency(string file)
        {
            t_markupStacks.Value.Peek().Dependencies.Add(file);
        }

        /// <summary>
        /// Checks if the input file results in a circular reference.
        /// </summary>
        public static bool IsCircularReference(string file, out IEnumerable<object> dependencyChain)
        {
            dependencyChain = null;

            var markupStack = t_markupStacks.Value;
            var inclusionStack = markupStack.Count > 0 ? markupStack.Peek().InclusionStack : null;
            if (inclusionStack is not null && inclusionStack.Contains(file))
            {
                dependencyChain = inclusionStack.Reverse();
                return true;
            }

            return false;
        }

        class DelegatingDisposable : IDisposable
        {
            private readonly Action _dispose;

            public DelegatingDisposable(Action dispose) => _dispose = dispose;

            public void Dispose() => _dispose();
        }
    }
}
