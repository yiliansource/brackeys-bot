using System;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace BrackeysBot.Commands.Archive
{
    /// <summary>
    /// Represents an archive that a channel can be projected into.
    /// </summary>
    internal sealed class ChannelArchive : IDisposable
    {
        /// <summary>
        /// The title of the archive.
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// How many messages the archive contains.
        /// </summary>
        public int ArchivedMessages => _archivedMessages;
        /// <summary>
        /// How many images the archive contains.
        /// </summary>
        public int ArchivedImages => _archivedImages;

        private readonly MarkdownFile _index;
        private readonly WebClient _assetDownloader;

        private readonly string _localDirectoryPath;
        private readonly string _localImageSubdirectoryPath;
        private string _localZippedPath;

        private int _archivedMessages;
        private int _archivedImages;

        /// <summary>
        /// Initializes a new <see cref="ChannelArchive"/>, ready to store archive data in.
        /// </summary>
        public ChannelArchive(string title)
        {
            Title = title;

            // Create a directory with a temporary name, to store the data in.
            _localDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _localImageSubdirectoryPath = Path.Combine(_localDirectoryPath, "assets");

            Directory.CreateDirectory(_localDirectoryPath);
            Directory.CreateDirectory(_localImageSubdirectoryPath);

            // Initialize the markdown index file
            _index = new MarkdownFile(Path.Combine(_localDirectoryPath, "index.md"));
            _index.AddHeading(title);

            // Initialize the asset downloader
            _assetDownloader = new WebClient();
        }

        /// <summary>
        /// Adds a message to the archive.
        /// </summary>
        public void AddMessage(ArchiveMessage message)
        {
            string content = message.Format();

            _index.AddListItem(content);
            _index.AddSeperator();
            _archivedMessages++;
        }
        /// <summary>
        /// Adds an image to the archive.
        /// </summary>
        public void AddImage(ArchiveImage image)
        {
            string imageSource = image.URL;
            string imageTarget = Path.Combine(_localImageSubdirectoryPath, Path.GetFileName(imageSource));

            _assetDownloader.DownloadFile(imageSource, imageTarget);
            _archivedImages++;
        }
        /// <summary>
        /// Closes the archive, cleaning up everything.
        /// </summary>
        public void CloseArchive()
        {
            _index.Dispose();
            _assetDownloader.Dispose();

            // Delete the assets folder if its not needed
            if (Directory.GetFiles(_localImageSubdirectoryPath).Length == 0)
                Directory.Delete(_localImageSubdirectoryPath);
        }

        /// <summary>
        /// Zips the archive and returns the path of the zipped archive.
        /// </summary>
        public string ZipArchive()
        {
            _localZippedPath = Path.Combine(Path.GetTempPath(), $"{Title}.zip");

            // Make sure a zipped archive doesnt already exist
            if (File.Exists(_localZippedPath))
                File.Delete(_localZippedPath);

            ZipFile.CreateFromDirectory(_localDirectoryPath, _localZippedPath);
            return _localZippedPath;
        }

        /// <summary>
        /// Disposes the <see cref="ChannelArchive"/>, deleting directories that were created during the archive process.
        /// </summary>
        public void Dispose()
        {
            // Clean up the archive files

            if (Directory.Exists(_localDirectoryPath))
                Directory.Delete(_localDirectoryPath, true);

            if (File.Exists(_localZippedPath))
                File.Delete(_localZippedPath);
        }
    }
}