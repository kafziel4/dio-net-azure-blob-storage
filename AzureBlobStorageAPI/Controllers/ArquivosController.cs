using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobStorageAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlobStorageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArquivosController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public ArquivosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("BlobConnectionString")!;
            _containerName = configuration.GetValue<string>("BlobContainerName")!;
        }

        [HttpPost]
        public IActionResult UploadArquivo(IFormFile arquivo)
        {
            BlobContainerClient container = new(_connectionString, _containerName);
            BlobClient blob = container.GetBlobClient(arquivo.FileName);

            using var data = arquivo.OpenReadStream();
            blob.Upload(data, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = arquivo.ContentType }
            });

            return Ok(blob.Uri.ToString());
        }

        [HttpGet("{nome}")]
        public IActionResult DownloadArquivo(string nome)
        {
            BlobContainerClient container = new(_connectionString, _containerName);
            BlobClient blob = container.GetBlobClient(nome);

            if (!blob.Exists())
                return BadRequest();

            var retorno = blob.DownloadContent();
            return File(retorno.Value.Content.ToArray(), retorno.Value.Details.ContentType, blob.Name);
        }

        [HttpDelete("{nome}")]
        public IActionResult DeletarArquivo(string nome)
        {
            BlobContainerClient container = new(_connectionString, _containerName);
            BlobClient blob = container.GetBlobClient(nome);

            blob.DeleteIfExists();

            return NoContent();
        }

        [HttpGet]
        public IActionResult ListarAquivos()
        {
            BlobContainerClient container = new(_connectionString, _containerName);
            List<BlobDto> blobDtos = [];

            foreach (var blob in container.GetBlobs())
            {
                blobDtos.Add(new BlobDto
                {
                    Nome = blob.Name,
                    Tipo = blob.Properties.ContentType,
                    Uri = $"{container.Uri.AbsoluteUri}/{blob.Name}"
                });
            }

            return Ok(blobDtos);

        }
    }
}
