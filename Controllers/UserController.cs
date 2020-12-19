using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _300958687_kim__Lab3.Data;
using _300958687_kim__Lab3.Models;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;

namespace _300958687_kim__Lab3.Controllers
{
    public class UserController : Controller
    {
        private IAmazonDynamoDB dynamoDBClient;

        public UserController(IAmazonDynamoDB dynamoDBClient)
        {
            this.dynamoDBClient = dynamoDBClient;
        }
        public IActionResult Index()
        {
            return View();
        }
        // GET : User/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Password,FirstName,LastName")] User user)
        {
            if (ModelState.IsValid)
            {
                DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
                User newUser = await service.InsertUser(user);
                return RedirectToAction("Index");
            }
            return View(user);
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel lm)
        {
            if(lm.Email != null)
            {
                //Console.WriteLine("EMAIL IS : " + lm.Email);
                DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
                User guser = await service.GetUserByEmailAsync(lm.Email);
                if(guser != null && guser.Password == lm.Password)
                {
                    //Console.WriteLine("USER FOUND!");
                    UserAccountModel uam = new UserAccountModel
                    {
                        UMovies = await service.GetMoviesAsync(),
                        UserId = guser.Id
                    };
                    //ViewBag.UserId = guser.Id;
                    return View("DisplayMovies",uam);
                }             
            }
            
            return RedirectToAction("Index");
        }
        

        public IActionResult Result()
        {
            return View();
        }
    }
}
