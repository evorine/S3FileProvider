# S3FileProvider

S3FileProvider is an implementation of `Microsoft.Extensions.FileProviders.Abstractions` library for AWS S3.

# Nuget
Library is published at Nuget.org as `Evorine.FileSystem.S3FileProvider`.

# Examples
Creating a `S3FileProvider` instance is very simple:
```csharp
var fileProvider = new S3FileProvider(amazonS3Service, bucketName);
```

## Using S3 as static files in Asp.Net Core website
In `Configure` method:
```csharp
public void Configure(IApplicationBuilder app)
{
    // ...
    
    var amazonS3 = new Amazon.S3.AmazonS3Client("awsAccessKeyId", "awsSecretAccessKey", Amazon.RegionEndpoint.USWest2);
    var fileProvider = new S3FileProvider(amazonS3, "bucket-name");

    var staticFilesOption = new StaticFileOptions()
    {
        FileProvider = fileProvider
    };
    app.UseStaticFiles(staticFilesOption);
    
    // ...
}
```
That's all!

# ToDos:
- Cancellation tokens
- File and directory watch feature
