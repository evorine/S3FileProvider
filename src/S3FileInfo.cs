using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;
using Amazon.S3.Model;
using System.IO;
using Amazon.S3;

namespace Evorine.Engine.FileProviders.S3
{
    public class S3FileInfo : IFileInfo
    {
        private GetObjectResponse fileObject;
        private IAmazonS3 amazonS3;
        
        public S3FileInfo(IAmazonS3 amazonS3, GetObjectResponse fileObject)
        {
            this.amazonS3 = amazonS3;
            this.fileObject = fileObject;
        }


        public bool Exists => true;

        public long Length => fileObject.ContentLength;


        /// <summary>
        /// A http url to the file, including the file name.
        /// </summary>
        public string PhysicalPath => $"{fileObject.BucketName}.s3.amazonaws.com/{fileObject.Key}";

        public string Name => Path.GetFileName(fileObject.Key);

        public DateTimeOffset LastModified => fileObject.LastModified;

        public bool IsDirectory => fileObject.Key.EndsWith("/");

        public Stream CreateReadStream()
        {
            return fileObject.ResponseStream;
        }
    }
}
