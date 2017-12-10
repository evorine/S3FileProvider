// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using Amazon;
using Amazon.S3;
using Amazon.Util;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Evorine.FileSystem.S3FileProvider.Test
{
    public class UnitTests
    {
        private static RegionEndpoint regionEndpoint = RegionEndpoint.USWest2;

        private static string bucketName = "evorine-engine-fileproviders-s3-test";

        private IAmazonS3 getS3Service()
        {
            var config = new AmazonS3Config()
            {
                DisableLogging = true,
                RegionEndpoint = regionEndpoint
            };
            
            
            var s3Client = new AmazonS3Client(credentials: null, clientConfig: config);
            return s3Client;
        }


        [Fact]
        public void AmazonS3_GetFileAndCheckInformation()
        {
            var fileProvider = new S3FileProvider(getS3Service(), bucketName);
            var fileInfo = fileProvider.GetFileInfo("/dummy.pdf");

            Assert.Equal(true, fileInfo.Exists);
            Assert.Equal(false, fileInfo.IsDirectory);
            Assert.Equal(new DateTime(2017, 5, 13, 15, 25, 10, 0, DateTimeKind.Utc), fileInfo.LastModified);
            Assert.Equal("dummy.pdf", fileInfo.Name);
            Assert.Equal("s3-us-west-2.amazonaws.com/evorine-engine-fileproviders-s3-test/dummy.pdf", fileInfo.PhysicalPath);
            Assert.Equal(88929, fileInfo.Length);
        }



        [Fact]
        public void AmazonS3_GetRootContents()
        {
            var fileProvider = new S3FileProvider(getS3Service(), bucketName);
            var folderInfo = fileProvider.GetDirectoryContents("/");

            Assert.Equal(true, folderInfo.Exists);
            Assert.Equal(2, folderInfo.Count());
            Assert.True(folderInfo.Any(x => x.Name == "dummy.pdf"));
            Assert.True(folderInfo.Any(x => x.Name == "folder-1"));
        }



        [Fact]
        public void AmazonS3_GetContentsOfSubFolder()
        {
            var fileProvider = new S3FileProvider(getS3Service(), bucketName);
            var folderInfo1 = fileProvider.GetDirectoryContents("/folder-1");
            var folderInfo2 = fileProvider.GetDirectoryContents("/folder-1/");

            Assert.Equal(true, folderInfo1.Exists);
            Assert.Equal(true, folderInfo2.Exists);

            Assert.Equal(4, folderInfo1.Count());
            Assert.Equal(4, folderInfo2.Count());

            Assert.True(folderInfo1.Any(x => x.Name == "folder-1-a"));
            Assert.True(folderInfo2.Any(x => x.Name == "folder-1-a"));
            Assert.True(folderInfo1.Any(x => x.Name == "folder-1-b"));
            Assert.True(folderInfo2.Any(x => x.Name == "folder-1-b"));
            Assert.True(folderInfo1.Any(x => x.Name == "aws.png"));
            Assert.True(folderInfo2.Any(x => x.Name == "aws.png"));
            Assert.True(folderInfo1.Any(x => x.Name == "dummy.pdf"));
            Assert.True(folderInfo2.Any(x => x.Name == "dummy.pdf"));
        }

        [Fact]
        public void AmazonS3_GetContentsOfEmptySubFolder()
        {
            var fileProvider = new S3FileProvider(getS3Service(), bucketName);
            var folderInfo1 = fileProvider.GetDirectoryContents("/folder-1/folder-1-b");
            var folderInfo2 = fileProvider.GetDirectoryContents("/folder-1/folder-1-b/");

            Assert.Equal(true, folderInfo1.Exists);
            Assert.Equal(true, folderInfo2.Exists);

            Assert.Equal(0, folderInfo1.Count());
            Assert.Equal(0, folderInfo2.Count());
        }


        [Fact]
        public void AmazonS3_GetContentsOfNotExistingSubFolder()
        {
            var fileProvider = new S3FileProvider(getS3Service(), bucketName);
            var folderInfo1 = fileProvider.GetDirectoryContents("/folder-1-not-exists");
            var folderInfo2 = fileProvider.GetDirectoryContents("/folder-1/folder-1-a/not-exists");
            var folderInfo3 = fileProvider.GetDirectoryContents("/not-exists");

            Assert.Equal(false, folderInfo1.Exists);
            Assert.Equal(false, folderInfo2.Exists);
            Assert.Equal(false, folderInfo3.Exists);

            Assert.Equal(0, folderInfo1.Count());
            Assert.Equal(0, folderInfo2.Count());
            Assert.Equal(0, folderInfo3.Count());
        }



        [Fact]
        public void AmazonS3_GetFileAndCheckContent()
        {
            var fileProvider = new S3FileProvider(getS3Service(), bucketName);
            var fileInfo = fileProvider.GetFileInfo("/folder-1/folder-1-a/ContentTest.txt");

            Assert.Equal(true, fileInfo.Exists);

            using (var textReader = new StreamReader(fileInfo.CreateReadStream()))
                Assert.Equal("Foo Bar", textReader.ReadToEnd());
        }

    }
}
