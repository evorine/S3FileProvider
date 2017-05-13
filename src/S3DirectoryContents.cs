using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using System.Linq;

namespace Evorine.Engine.FileProviders.S3
{
    /// <summary>
    /// Contents of a S3 directory. Files are the keys prefixed by given 'path'.
    /// </summary>
    public class S3DirectoryContents : IDirectoryContents
    {
        readonly IAmazonS3 amazonS3;
        readonly string bucketName;
        readonly string subpath;

        private IEnumerable<IFileInfo> contents;

        /// <summary>
        /// Initializes a <see cref="S3DirectoryContents"/> instance.
        /// </summary>
        public S3DirectoryContents(IAmazonS3 amazonS3, string bucketName, string subpath)
        {
            this.amazonS3 = amazonS3;
            this.bucketName = bucketName;
            this.subpath = subpath;
        }
        

        /// <summary>
        /// True if a directory is located at the given path. 
        /// </summary>
        public bool Exists
        {
            get
            {
                try
                {
                    amazonS3.GetObjectMetadataAsync(bucketName, subpath).Wait();
                    return true;
                }
                catch(AmazonS3Exception e)
                {
                    if (e.StatusCode == HttpStatusCode.NotFound) return false;
                    throw;
                }
                
            }
        }

        /// <inheritdoc />
        public IEnumerator<IFileInfo> GetEnumerator()
        {
            enumerateContents();
            return contents.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            enumerateContents();
            return contents.GetEnumerator();
        }

        private void enumerateContents()
        {
            contents = amazonS3.GetAllObjectKeysAsync(bucketName, subpath, null)
                               .Result
                               .Select(x => new S3FileInfo(amazonS3, bucketName, x));
        }
    }
}
