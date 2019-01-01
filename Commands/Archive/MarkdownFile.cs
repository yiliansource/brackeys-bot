using System;
using System.IO;
using System.Text;

namespace BrackeysBot.Commands.Archive
{
    /// <summary>
    /// Represents a markdown file.
    /// </summary>
    internal class MarkdownFile : IDisposable
    {
        /// <summary>
        /// The path where the file is located at.
        /// </summary>
        public string Path => _path;
        /// <summary>
        /// Is the file empty?
        /// </summary>
        public bool IsEmpty => _isStartOfFile;
        /// <summary>
        /// Is the file still open to be written to?
        /// </summary>
        public bool IsOpen => _isOpen;

        private readonly string _path;
        private readonly StreamWriter _writer;

        private bool _isStartOfFile = true;
        private bool _isOpen = true;

        private const string INDENT = "    ";

        /// <summary>
        /// Creates a new <see cref="MarkdownFile"/>, at the specified path.
        /// </summary>
        /// <param name="path"></param>
        public MarkdownFile(string path)
        {
            _path = path;

            FileStream stream = File.Create(path);
            _writer = new StreamWriter(stream);
        }

        /// <summary>
        /// Adds a paragraph with the specified content to the file.
        /// </summary>
        public void AddParagraph(string content)
        {
            Write(content);
        }
        /// <summary>
        /// Adds a top-level heading the file.
        /// </summary>
        public void AddHeading(string content)
        {
            AddHeading(content, 1);
        }
        /// <summary>
        /// Adds a heading with the specified level to the file.
        /// </summary>
        public void AddHeading(string content, int level)
        {
            if (level < 1) level = 1;
            Write($"{new string('#', level)} {content}");
        }
        /// <summary>
        /// Adds a list item to the file.
        /// </summary>
        public void AddListItem(string content)
        {
            Write(Indent(content, '-'));
        }
        /// <summary>
        /// Adds a quote to the file.
        /// </summary>
        public void AddQuote(string content)
        {
            Write(Indent(content, '>'));
        }
        /// <summary>
        /// Adds a codeblock to the file.
        /// </summary>
        public void AddCodeblock(string content)
        {
            AddCodeblock(content, string.Empty);
        }
        /// <summary>
        /// Adds a codeblock with the specified highlighting to the file.
        /// </summary>
        public void AddCodeblock(string content, string syntaxHighlighting)
        {
            StringBuilder codeblock = new StringBuilder()
                .AppendLine("```" + syntaxHighlighting)
                .AppendLine(content)
                .AppendLine("```");

            Write(codeblock.ToString());
        }
        /// <summary>
        /// Adds a seperator the file.
        /// </summary>
        public void AddSeperator()
        {
            _writer.Write("---");
        }

        /// <summary>
        /// Indents the specified text.
        /// </summary>
        private static string Indent(string text)
        {
            string[] lines = text.Split(Environment.NewLine);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i])) sb.AppendLine(INDENT + lines[i]);
                else sb.AppendLine();
            }

            return sb.ToString();
        }
        /// <summary>
        /// Indents the specified text, and prefixes the first line by the specified character.
        /// </summary>
        private static string Indent(string text, char firstLineChar)
        {
            string[] lines = text.Split(Environment.NewLine);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i])) sb.AppendLine((i == 0 ? firstLineChar + " " : INDENT) + lines[i]);
                else sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes the content to the file, seperating it from the rest of the contents, if the file has contents.
        /// </summary>
        private void Write(string content)
        {
            if (!_isStartOfFile) { _writer.WriteLine(Environment.NewLine); }
            _isStartOfFile = false;

            _writer.Write(content);
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        public void Dispose()
        {
            _writer.Close();
            _isOpen = false;
        }
    }
}