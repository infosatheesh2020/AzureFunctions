
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using System.Net;
using System.Text;

namespace FunctionApp22
{
    public static class GetRatingById
    {
        [FunctionName("GetRatingById")]
        public static HttpResponseMessage Run1([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
                Route = "rating/{id}")]HttpRequest req, [CosmosDB(databaseName: "hacker1satheesh",collectionName: "Items",
                ConnectionStringSetting = "CosmosConn",Id = "{id}")]Ratings ratingObj,
           TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (ratingObj == null)
            {
                log.Info($"ToDo item not found");
            }
            else
            {
                log.Info($"Found ToDo item, Description={ratingObj.id}");
            }
            //return new OkResult();
            var jsonToReturn = JsonConvert.SerializeObject(ratingObj);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public class Ratings
        {
            public string id { get; set; }
            public string userId { get; set; }
            public string productId { get; set; }
            public DateTime timestamp { get; set; }
            public string locationName { get; set; }
            public string rating { get; set; }
            public string userNotes { get; set; }
        }
    }



}
