using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using System.Security.Authentication;

namespace OrderFunctions
{
    public static class DeliveryOrderProcessor
    {
        [FunctionName("SaveToBlob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            var data = JsonConvert.DeserializeObject<OrderToSave>(requestBody);                
            await Save(data);

            return new OkObjectResult(requestBody);
        }

        public static async Task Save(OrderToSave data)
        {
            string connectionString =
                @"mongodb://admin-sa:HRim4gEZTwF4d5qcYAaECDZjgXD9iEYOfQ83HRD6mnBMRiRGIJUs018wYQn6GlRtR7q630MIbqJvdqldHgD73Q==@admin-sa.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@admin-sa@";
            MongoClientSettings settings = MongoClientSettings.FromUrl(
                new MongoUrl(connectionString)
            );
            settings.SslSettings =
                new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);
            var db = mongoClient.GetDatabase("AzizDb");
            var orders = db.GetCollection<OrderToSave>("OrderToSave", new MongoCollectionSettings() { AssignIdOnInsert = false });
            await orders.InsertOneAsync(data);
        }

        public class OrderToSave
        {
            public Guid Id { get; set; }

            [JsonProperty("document")]
            public Order Document { get; set; }

        }
    }
}
