using System;
using Julefrokost16.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;

namespace Julefrokost16.Controllers
{
    [Route("api/[controller]")]
    public class DocumentController : Controller
    {
        private readonly DocumentClient client;
        public DocumentController(IOptions<AppSettings> options)
        {
            client = new DocumentClient(new Uri(options.Value.DbEndpoint), options.Value.DbToken);
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            
            return "value";
        }


    }
}