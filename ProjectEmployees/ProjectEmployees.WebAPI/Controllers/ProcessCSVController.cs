using Microsoft.AspNetCore.Mvc;
using ProjectEmployees.Core.Interfaces;
using ProjectEmployees.Core.Objects;

namespace ProjectEmployees.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessCSVController : ControllerBase
    {
        private readonly ILogger<ProcessCSVController> _logger;
        private readonly IManager _manager;

        public ProcessCSVController(ILogger<ProcessCSVController> logger, IManager manager)
        {
            _logger = logger;
            _manager = manager;
        }

        [Route("Upload")]
        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File missing.");
            }
            if (!file.FileName.ToLower().EndsWith(".csv"))
            {
                return BadRequest("Only csv files can be processed.");
            }

            VerifyDatabank();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Databank", file.FileName);
            using (var fileStream = System.IO.File.Create(filePath))
            {
                file.CopyTo(fileStream);
            }

            _manager.ClearCache(filePath);

            return Ok("File uploaded.");
        }

        [Route("GetData")]
        [HttpGet]
        public IEnumerable<EmployeePair> GetProcessedData(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new BadHttpRequestException("File name missing.");
            }
            if (!fileName.ToLower().EndsWith(".csv"))
            {
                throw new BadHttpRequestException("Only csv files can be processed.");
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Databank", fileName);
            if (!System.IO.File.Exists(filePath))
                throw new BadHttpRequestException($"File with name \"{fileName}\" missing from the bank");

            var processedData = _manager.CompileCsvData(filePath);

            return processedData;
        }

        [Route("GetFiles")]
        [HttpGet]
        public IEnumerable<string> GetAvailableFilesList()
        {
            string bankPath = Path.Combine(Directory.GetCurrentDirectory(), "Databank");
            var rawFilePaths = Directory.GetFiles(bankPath);

            var fileNames = new List<string>();

            foreach (var rawFile in rawFilePaths) 
            { 
                fileNames.Add(Path.GetFileName(rawFile)); 
            }

            return fileNames;
        }

        private void VerifyDatabank()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Databank");
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
