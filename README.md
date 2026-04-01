# Markdig Inclusion Test

This project is a PoC on how we might include markdown files from other files.

A good introduction to Markdig for developers is given at their
[Wiki AST](https://xoofx.github.io/markdig/docs/advanced/ast/).

The [Crash Course in Markdig](https://johnh.co/blog/a-crash-course-in-markdig)
is also very informative.

This implementation looks at the
[DocFX Inclusion](https://github.com/dotnet/docfx/blob/2a436495ed0716eebbc7eaad84d652f18600c2f6/src/Docfx.MarkdigEngine.Extensions/Inclusion/InclusionBlock/InclusionBlockParser.cs)
parser sources, that implements the specification at
[DocFX Include Markdown Files](https://dotnet.github.io/docfx/docs/markdown.html?tabs=linux%2Cdotnet#include-markdown-files)
which aligns well with the crash course (parsing and insert a
`BlockContainer` in the tree, then walk the AST).
