using System.Text;
using Amazon.S3;
using Amazon.S3.Model;


// ======== Uploading data =========

var s3Client = new AmazonS3Client();

using var inputStream = new FileStream("./face.jpg", FileMode.Open, FileAccess.Read);

var putObjectRequest =  new PutObjectRequest
{
    BucketName = "erbaws",
    Key = "images/face.jpg",
    ContentType = "image/jpeg",
    InputStream = inputStream,
};

await s3Client.PutObjectAsync(putObjectRequest);

// uploading csv file
using var inputStream2 = new FileStream("./movies.csv", FileMode.Open, FileAccess.Read);

var putObjectRequest2 =  new PutObjectRequest
{
    BucketName = "erbaws",
    Key = "files/movies.csv",
    ContentType = "text/csv",
    InputStream = inputStream2,
};

await s3Client.PutObjectAsync(putObjectRequest2);


// ========== Downloading data ==========

var getObjectRequest = new GetObjectRequest
{
    BucketName = "erbaws",
    Key = "files/movies.csv",
};
var response = await s3Client.GetObjectAsync(getObjectRequest);

var memoryStream = new MemoryStream();
response.ResponseStream.CopyTo(memoryStream);   // copy response to mem stream to read as byte

var textString = Encoding.Default.GetString(memoryStream.ToArray());

System.Console.WriteLine(textString);



// NB: AWS sdk is architectured around "request-response" pattern.

/*

"using" vs not "using"

var memoryStream = new MemoryStream();
try
{
    // Perform operations on memoryStream
}
finally
{
    if (memoryStream != null)
    {
        memoryStream.Dispose(); // Must manually ensure disposal
    }
}

----

using var memoryStream = new MemoryStream();
// Perform operations on memoryStream
// Automatically disposed at the end of the scope


In conclusion, using 'using var' for MemoryStream (and other IDisposable objects) is a best practice as 
it ensures that resources are properly and automatically disposed of, improving resource management, exception safety, and code clarity.







*/