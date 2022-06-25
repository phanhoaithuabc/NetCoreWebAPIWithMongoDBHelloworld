using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using NetCoreWebAPIWithMongoDBHelloworld.Models;
using System;
using System.IO;
using System.Linq;

namespace NetCoreWebAPIWithMongoDBHelloworld.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public EmployeeController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public JsonResult Get()
        {
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("EmployeeAppCon"));
            var dbList = dbClient.GetDatabase("NetCoreAPIHelloworld").GetCollection<Employee>("Employee").AsQueryable();
            return new JsonResult(dbList);
        }

        [HttpPost]
        public JsonResult Post(Employee emp)
        {
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("EmployeeAppCon"));
            int LastEmployeeId = dbClient.GetDatabase("NetCoreAPIHelloworld").GetCollection<Department>("Employee").AsQueryable().Count();
            emp.EmployeeId = LastEmployeeId + 1;
            dbClient.GetDatabase("NetCoreAPIHelloworld").GetCollection<Employee>("Employee").InsertOne(emp);
            return new JsonResult("Added Successfully");
        }

        [HttpPut]
        public JsonResult Put(Employee emp)
        {
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("EmployeeAppCon"));
            var filter = Builders<Employee>.Filter.Eq("EmployeeId", emp.EmployeeId);
            var update = Builders<Employee>.Update.Set("EmployeeName", emp.EmployeeName)
                                                    .Set("Department", emp.Department)
                                                    .Set("DateOfJoining", emp.DateOfJoining)
                                                    .Set("PhotoFileName", emp.PhotoFileName);
            dbClient.GetDatabase("NetCoreAPIHelloworld").GetCollection<Employee>("Employee").UpdateOne(filter, update);
            return new JsonResult("Updated Successfully");
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("EmployeeAppCon"));
            var filter = Builders<Employee>.Filter.Eq("EmployeeId", id);
            dbClient.GetDatabase("NetCoreAPIHelloworld").GetCollection<Employee>("Employee").DeleteOne(filter);
            return new JsonResult("Deleted Successfully");
        }

        [Route("SaveFile")]
        [HttpPost]
        public JsonResult SaveFile()
        {
            try{
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string filename = postedFile.FileName;
                var physicalPath = _env.ContentRootPath + "/Photos/" + filename;
                using (var stream = new FileStream(physicalPath, FileMode.Create)){ postedFile.CopyTo(stream); }
                return new JsonResult(filename);
            }
            catch (Exception){
                return new JsonResult("anonymous.png");
            }
        }
    }
}
