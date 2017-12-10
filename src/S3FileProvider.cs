// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.FileProviders;
using Amazon.S3;

namespace Evorine.FileSystem.S3FileProvider
{
    /// <summary>
    /// Looks up files on AWS S3 file system.
    /// </summary>
    /// <remarks>
    /// It is readonly.
    /// </remarks>
    public class S3FileProvider : IFileProvider, IDisposable
    {
        private static readonly char[] pathSeparators = new[] { '/' };
        private static readonly char[] invalidFileNameChars = new[] { '\\', '{', '}', '^', '%', '`', '[', ']', '\'', '"', '>', '<', '~', '#', '|' }
                                                              .Concat(Enumerable.Range(128, 255).Select(x => (char)x))
                                                              .ToArray();

        readonly IAmazonS3 amazonS3;
        readonly string bucketName;

        /// <summary>
        /// Initializes a new instance of a <see cref="S3FileProvider"/> at the given bucket.
        /// </summary>
        /// <param name="amazonS3"><see cref="IAmazonS3" /> Amazon S3 service object</param>
        /// <param name="bucketName">Name of the bucket that will be used</param>
        public S3FileProvider(IAmazonS3 amazonS3, string bucketName)
        {
            this.amazonS3 = amazonS3;
            this.bucketName = bucketName;
        }

        
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null) throw new ArgumentNullException(nameof(subpath));
            if (HasInvalidFileNameChars(subpath)) return NotFoundDirectoryContents.Singleton;

            // Relative paths starting with leading slashes are okay
            subpath = subpath.TrimStart(pathSeparators);

            return new S3DirectoryContents(amazonS3, bucketName, subpath);
        }


        /// <summary>
        /// Locates a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the bucket</param>
        /// <returns>The file information. Caller must check Exists property.</returns>
        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null) throw new ArgumentNullException(nameof(subpath));
            if (HasInvalidFileNameChars(subpath)) return new NotFoundFileInfo(subpath);

            // Relative paths starting with leading slashes are okay
            subpath = subpath.TrimStart(pathSeparators);
            
            if (string.IsNullOrEmpty(subpath))
                return new NotFoundFileInfo(subpath);
            
            return new S3FileInfo(amazonS3, bucketName, subpath);
        }


        /// <summary>
        /// Watch is not supported.
        /// </summary>
        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }


        /// <summary>
        /// Disposes the file provider.
        /// </summary>
        public void Dispose()
        {
            amazonS3.Dispose();
        }


        private bool HasInvalidFileNameChars(string path)
        {
            return path.IndexOfAny(invalidFileNameChars) != -1;
        }
    }
}
