
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;
using System.Text;
using System;
using System.Threading.Tasks;

namespace Challenge2new
{
    public static class Function1
    {
        [FunctionName("CreateRating")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log,
             [CosmosDB(databaseName: "hacker1satheesh", collectionName: "Items", ConnectionStringSetting = "CosmosConn")]IAsyncCollector<object> outputDocument)

        {
            log.Info("CreateRating function triggerred...");

            string userId, productId, locationName, rating, userNotes = string.Empty;

            string jsonContent = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);

            log.Info("Processed payload...");

            if (data.userId != null)
                log.Info("Payload is empty...");
            else
                return null;

            userId = data.userId;
            productId = data.productId;
            locationName = data.locationName;
            rating = data.rating;
            userNotes = data.userNotes;

            var client = new HttpClient();

            var response = await client.GetAsync("http://serverlessohuser.trafficmanager.net/api/GetUser?userId=" + userId);
            //{"userId":"cc20a6fb-a91f-4192-874d-132493685376","userName":"doreen.riddle","fullName":"Doreen Riddle"}
            var userIdresponsecontent = await response.Content.ReadAsStringAsync();

            var response2 = await client.GetAsync("http://serverlessohuser.trafficmanager.net/api/GetProduct?productId=" + productId);
            // { "productId":"4c25613a-a3c2-4ef3-8e02-9c335eb23204","productName":"Truly Orange-inal","productDescription":"Made from concentrate."}
            var getproductresponsecontent = await response2.Content.ReadAsStringAsync();

            if (userIdresponsecontent.Contains(userId) && getproductresponsecontent.Contains(productId))
                log.Info("UserID and productId validated...");
            else
                log.Info("Invalid userId or productid");

            string newid = System.Guid.NewGuid().ToString();
            DateTime timestamp = DateTime.Now.ToUniversalTime();

            Ratings ratingObj = new Ratings()
            {
                id = newid,
                userId = userId,
                productId = productId,
                timestamp = timestamp,
                locationName = locationName,
                rating = rating,
                userNotes = userNotes
            };

            //Cosmos DB Insertion
            await outputDocument.AddAsync(ratingObj);

            var jsonToReturn = JsonConvert.SerializeObject(ratingObj);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }


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


        [FunctionName("GetRatingbyUserId")]
        public static HttpResponseMessage Run2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "userrating/{userId}")]
                HttpRequest req,
             [CosmosDB(databaseName: "createratingoutDatabase",
                collectionName: "MyCollection",
                ConnectionStringSetting = "CosmosConn",SqlQuery  = "SELECT * FROM c where c.userId={userId}")]
                System.Collections.Generic.IEnumerable<Ratings> documents,
           TraceWriter log)
        {
            bool atleastOne = false;
            foreach (var e in documents)
            {
                atleastOne = true;
                break;
            }

            if (atleastOne == false)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            var jsonToReturn = JsonConvert.SerializeObject(documents);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

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

    public class OrderHeadDetails
    {
        public string ponumber { get; set; }
        public string datetime { get; set; }
        public string locationid { get; set; }
        public string locationname { get; set; }
        public string locationaddress { get; set; }
        public string locationpostcode { get; set; }
        public string totalcost { get; set; }
        public string totaltax { get; set; }
    }

}
