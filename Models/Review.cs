using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _300958687_kim__Lab3.Models
{
    [DynamoDBTable("Review")]
    public class Review
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        public User User { get; set; }
        public Movie Movie { get; set; }
        public string CreatedDate { get; set; }
        public int Rate { get; set; }
        public string Content { get; set; }
    }
}
