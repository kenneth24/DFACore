using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ImageModel
    {
        public string Name { get; set; }
        public IFormFile File { get; set; }
    }
}
