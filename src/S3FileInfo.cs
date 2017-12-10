// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;
using Amazon.S3.Model;
using System.IO;
using Amazon.S3;
using System.Net;

namespace Evorine.FileSystem.S3FileProvider
{
    public class S3FileInfo : IFileInfo
    {
        private readonly IAmazonS3 amazonS3;
        private readonly string bucketName;
        private readonly string key;

        private GetObjectResponse fileObject;
        private bool? exists;

        public S3FileInfo(IAmazonS3 amazonS3, string bucketName, string key)
        {
            this.amazonS3 = amazonS3;
            this.bucketName = bucketName;
            this.key = key;
        }

        private GetObjectResponse getfileObject()
        {
            if (fileObject == null)
            {
                fileObject = amazonS3.GetObjectAsync(bucketName, key).Result;
            }
            return fileObject;
        }


        public string MD5 => getfileObject().Metadata["Content-MD5"];

        public bool Exists
        {
            get
            {
                if (!exists.HasValue)
                {
                    try
                    {
                        getfileObject();
                        exists = true;
                    }
                    catch (AmazonS3Exception e)
                    {
                        if (e.StatusCode == HttpStatusCode.NotFound) exists = false;
                        throw;
                    }
                }
                return exists.Value;
            }
        }



        public long Length => getfileObject().ContentLength;


        /// <summary>
        /// A http url to the file, including the file name.
        /// </summary>
        public string PhysicalPath => $"s3-{amazonS3.Config.RegionEndpoint.SystemName}.amazonaws.com/{getfileObject().BucketName}/{getfileObject().Key}";

        public string Name => Path.GetFileName(getfileObject().Key.TrimEnd('/'));

        public DateTimeOffset LastModified => getfileObject().LastModified;

        public bool IsDirectory => getfileObject().Key.EndsWith("/");

        public Stream CreateReadStream()
        {
            return getfileObject().ResponseStream;
        }
    }
}
