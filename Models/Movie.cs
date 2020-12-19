using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _300958687_kim__Lab3.Models
{
    [DynamoDBTable("Movie")]
    public class Movie
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        public string Name { get; set; }
        [DynamoDBProperty("Uploaded Date")]
        public string UploadedDate { get; set; }
        public S3Link Video { get; set; }
        public User UploadedBy { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
