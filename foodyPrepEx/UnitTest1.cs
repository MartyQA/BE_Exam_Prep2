using Foody.Models;
using foodyPrepEx.models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;




namespace foodyPrepEx
{
    [TestFixture]
    public class Tests
    {
        private const string BaseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";
        private RestClient client;
        private static string lastFoodyId;
        private const string token =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJiNmFmMjhmOS0zOTBhLTQyODAtOTAyNi0xYjU5NDVmODg2ZjYiLCJpYXQiOiIwOC8xNS8yMDI1IDE1OjUzOjQwIiwiVXNlcklkIjoiOGJhY2MyYzYtNzZlZS00MzNlLTlhZWMtMDhkZGQ4ZTVkYWIyIiwiRW1haWwiOiJtdnZAZ21haWwuY29tIiwiVXNlck5hbWUiOiJtdnYiLCJleHAiOjE3NTUyOTQ4MjAsImlzcyI6IkZvb2R5X0FwcF9Tb2Z0VW5pIiwiYXVkIjoiRm9vZHlfV2ViQVBJX1NvZnRVbmkifQ.1TFnjgfnK_EGWTXk8zwndb_CtvwTOD-grh_IUUyFaM8";

        private const string LoginEmail = "mvv@gmail.com";
        private const string LoginPassword = "123456";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrEmpty(token))
            {
                jwtToken = token;

            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string loginEmail, string loginPassword)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/Users/Authentication", Method.Post);

            request.AddJsonBody(new { loginEmail, loginPassword });

            var response = tempClient.Execute(request);

            var content = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return content.GetProperty("accessToken").GetString() ?? string.Empty;

        }
        // All Tests here

        [Order(1)]
        [Test]
        public void CreateNewFoodWithRequiredFields_ShouldReturnSuccess()
        {
            var food = new FoodDTO
            {
                Name = "Test Food",
                Description = "This is a test food item.",
                Url = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
           request.AddJsonBody(food);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            lastFoodyId = json.GetProperty("foodId").GetString();

            Assert.That(lastFoodyId, Is.Not.Null.And.Not.Empty);


        }

        [Order(2)]
        [Test]
        public void EditfoodName_ShouldReturnSuccess()
        {
            var food = new[]
            {
                new {path = "/name", op = "replace", value = "Updated Food Name"}
            };
            var request = new RestRequest($"/api/Food/Edit/{lastFoodyId}", Method.Patch);
            request.AddJsonBody(food);
            

            var response = this.client.Execute(request); 
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.Msg, Is.EqualTo("Successfully edited"));


        }

        [Order(3)]
        [Test]

        public void GetAllFoods_ShouldReturnSuccess()
        {
            var reqest = new RestRequest("/api/Food/All", Method.Get);

            var response = this.client.Execute(reqest);

            var json = JsonSerializer.Deserialize<List<object>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json, Is.Not.Empty);

        }

        [Order(4)]
        [Test]

        public void DeleteFood_ShouldReturnSuccess()
        {
            var reqest = new RestRequest ($"/api/Food/Delete/{lastFoodyId}", Method.Delete);

            var response = this.client.Execute(reqest);

            

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(json.Msg, Is.EqualTo("Deleted successfully!"));

        }

        [Order(5)]
        [Test]
        public void CreateFoodWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var food = new FoodDTO
            {
                Name = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Order(6)]
        [Test]
        public void EditNonExistingFood_ShouldReturnNotFound()
        {
            var food = new[]
            {
                new {path = "/name", op = "replace", value = "Updated Food Name"}
            };
            var request = new RestRequest("/api/Food/Edit/dksfjsdkl", Method.Patch);
            request.AddJsonBody(food);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var json = JsonSerializer.Deserialize<ApiResponseDTO> (response.Content);
            Assert.That(json.Msg, Is.EqualTo("No food revues..."));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingFood_ShouldReturnBadReqest()
        {
            var reqest = new RestRequest($"/api/Food/Delete/fafadgg", Method.Delete);

            var response = this.client.Execute(reqest);



            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(json.Msg, Is.EqualTo("Unable to delete this food revue!"));
        }














        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}