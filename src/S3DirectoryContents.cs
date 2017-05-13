using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Amazon.S3;

namespace Evorine.Engine.FileProviders.S3
{
    public class S3DirectoryContents : IDirectoryContents
    {
        private IAmazonS3 amazonS3;
        private string fullPath;

        public S3DirectoryContents(IAmazonS3 amazonS3, string fullPath)
        {
            this.amazonS3 = amazonS3;
            this.fullPath = fullPath;
        }

        public bool Exists => throw new NotImplementedException();

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
