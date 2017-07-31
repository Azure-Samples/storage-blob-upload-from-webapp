using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ImageResizeWebApp.Models;
using Microsoft.Extensions.Options;

namespace ImageResizeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private readonly AzureStorageConfig _storageConfig;

        public ImagesController(IOptions<AzureStorageConfig> config)
        {
            _storageConfig = config.Value;
        }

        [HttpGet("[action]")]
        public string Storage()
        {
            var val = _storageConfig;
            return "https://url";
        }

        [HttpGet]
        public string List()
        {
            var val = _storageConfig;
            return "https://url";
        }



    }
}