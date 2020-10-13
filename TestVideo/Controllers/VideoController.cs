using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TestVideo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public VideoController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("addlocal")]
        public async Task<string> Addlocal()
        {
            var cs = configuration.GetConnectionString("AccessKey");

            BlobServiceClient blobServiceClient = new BlobServiceClient(cs);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("videos");

            string localPath = "Data/";
            string fileName = "video-" + Guid.NewGuid().ToString() + ".mp4";
            string readFileName = "video.mp4";
            string localFilePath = Path.Combine(localPath, readFileName);
            using FileStream uploadFileStream = System.IO.File.OpenRead(localFilePath);
            await containerClient.UploadBlobAsync(fileName, uploadFileStream);
            var uri = containerClient.Uri.AbsoluteUri;
            uploadFileStream.Close();

            return string.Join("/", uri, fileName);
        }

        [HttpPost]
        [Route("addurl")]
        public async Task<string> Addurl()
        {
            var cs = configuration.GetConnectionString("AccessKey");

            BlobServiceClient blobServiceClient = new BlobServiceClient(cs);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("videos");

            using (var client = new HttpClient())
            {
                // var response = await client.GetAsync("http://techslides.com/demos/sample-videos/small.webm"); // small video
                var response = await client.GetAsync("http://clips.vorwaerts-gmbh.de/big_buck_bunny.webm"); // bigger video
                var stream = await response.Content.ReadAsStreamAsync();

                string fileName = "video-" + Guid.NewGuid().ToString() + ".mp4";
                await containerClient.UploadBlobAsync(fileName, stream);
                var uri = containerClient.Uri.AbsoluteUri;

                return string.Join("/", uri, fileName);
            }
        }
    }
}
