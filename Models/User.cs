using Amazon.DynamoDBv2.DataModel;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _300958687_kim__Lab3.Models
{
    [DynamoDBTable("User")]
    public class User
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [DynamoDBProperty("First Name")]
        public string FirstName { get; set; }
        [DynamoDBProperty("Last Name")]
        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                return LastName + ", " + FirstName;
            }
        }

      
    }
}
