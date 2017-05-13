using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;
using Amazon.S3.Model;
using System.IO;
using Amazon.S3;
using System.Net;

namespace Evorine.Engine.FileProviders.S3
{
    public class S3FileInfo : IFileInfo
    {
        readonly IAmazonS3 amazonS3;
        readonly string bucketName;
        readonly string key;

        private GetObjectResponse _fileObject;
        private bool? _exists;

        public S3FileInfo(IAmazonS3 amazonS3, string bucketName, string key)
        {
            this.amazonS3 = amazonS3;
            this.bucketName = bucketName;
            this.key = key;
        }

        private GetObjectResponse getfileObject()
        {
            if (_fileObject == null)
            {
                _fileObject = amazonS3.GetObjectAsync(bucketName, key).Result;
            }
            return _fileObject;
        }


        public string MD5 => getfileObject().Metadata["Content-MD5"];

        public bool Exists
        {
            get
            {
                if (_exists.HasValue)
                {
                    try
                    {
                        getfileObject();
                        _exists = true;
                    }
                    catch (AmazonS3Exception e)
                    {
                        if (e.StatusCode == HttpStatusCode.NotFound) _exists = false;
                        throw;
                    }
                }
                return _exists.Value;
            }
        }



        public long Length => getfileObject().ContentLength;


        /// <summary>
        /// A http url to the file, including the file name.
        /// </summary>
        public string PhysicalPath => $"{getfileObject().BucketName}.s3.amazonaws.com/{getfileObject().Key}";

        public string Name => Path.GetFileName(getfileObject().Key);

        public DateTimeOffset LastModified => getfileObject().LastModified;

        public bool IsDirectory => getfileObject().Key.EndsWith("/");

        public Stream CreateReadStream()
        {
            return getfileObject().ResponseStream;
        }
    }
}
