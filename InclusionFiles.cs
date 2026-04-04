namespace RJCP.MarkDigTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    public struct InclusionFileContext : IDisposable
    {
        public InclusionFileContext(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; }

        public void Dispose() => InclusionFiles.Pop();
    }

    public static class InclusionFiles
    {
        private static readonly HashSet<string> files = new(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Stack<string> stack = new();

        public static string PushFile(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);
            string fullPath;
            if (!Path.IsPathFullyQualified(fileName) && Path.IsPathRooted(fileName))
            {
                // We have either `X:filename` or `\filename`, but not "X:\filename".
                if (fileName[0] != Path.DirectorySeparatorChar && fileName[0] != Path.AltDirectorySeparatorChar ||
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw new ArgumentException("File name is not a valid path", nameof(fileName));
                }
                fullPath = Path.GetFullPath(fileName);
            }
            else
            {
                fullPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, fileName));
            }

            files.Clear();
            stack.Clear();
            files.Add(fullPath);
            stack.Push(fullPath);
            return fullPath;
        }

        public static InclusionFileContext PushDependency(string fileName)
        {
            if (Path.IsPathFullyQualified(fileName) || Path.IsPathRooted(fileName))
            {
                throw new ArgumentException("File name is not a valid relative path", nameof(fileName));
            }

            string prevPath = stack.Peek();
            string newPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(prevPath), fileName));
            if (files.Contains(newPath))
            {
                throw new ArgumentException("Circular reference detected", nameof(fileName));
            }

            if (!File.Exists(newPath))
            {
                throw new ArgumentException($"File not found: '{newPath}'", nameof(newPath));
            }

            stack.Push(newPath);
            files.Add(newPath);
            return new(newPath);
        }

        public static void Pop()
        {
            if (stack.Count <= 1)
            {
                throw new InvalidOperationException("No file to pop");
            }

            string popped = stack.Pop();
            files.Remove(popped);
        }
    }
}
