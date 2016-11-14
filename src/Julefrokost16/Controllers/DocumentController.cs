using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Julefrokost16.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;

namespace Julefrokost16.Controllers
{
    [Route("api/[controller]")]
    public class DocumentController : Controller
    {
        private const string DbName = "Julefrokost16";
        private const string GameStateCollectionName = "GameState";
        private readonly DocumentClient _client;
        public DocumentController(IOptions<AppSettings> options)
        {
            _client = new DocumentClient(new Uri(options.Value.DbEndpoint), options.Value.DbToken);
            CreateDatabaseIfNotExists().Wait();
            CreateDocumentCollectionIfNotExists(GameStateCollectionName).Wait();
            

        }

        [HttpGet("{id}")]
        public GameState Get(string id)
        {
            try
            {
                var queryOptions = new FeedOptions {MaxItemCount = 1};
                var q = _client.CreateDocumentQuery<GameState>(GameStateUri(), queryOptions)
                    .Where(g => g.Id == id);
                return q.AsEnumerable().FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        [HttpPost]
        public void Post([FromBody]GameState gameState)
        {
            try
            {
                CreateGameSateDocumentIfNotExists(gameState).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private Uri GameStateUri()
        {
            return UriFactory.CreateDocumentCollectionUri(DbName, GameStateCollectionName);
        }
        private async Task CreateDatabaseIfNotExists()
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DbName));
                
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDatabaseAsync(new Database { Id = DbName });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateDocumentCollectionIfNotExists(string collectionName)
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DbName, collectionName));
                
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collectionName;

                    // Configure collections for maximum query flexibility including string range queries.
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // Here we create a collection with 400 RU/s.
                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DbName),
                        collectionInfo,
                        new RequestOptions { OfferThroughput = 400 });
                    
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateGameSateDocumentIfNotExists(GameState gameState)
        {
            try
            {
                await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DbName, GameStateCollectionName, gameState.Id));
                
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DbName, GameStateCollectionName), gameState);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}