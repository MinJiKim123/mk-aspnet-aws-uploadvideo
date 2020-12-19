using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _300958687_kim__Lab3.Models
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class CreateMovieModel
    {
        public string Name { get; set; }
        public IFormFile VideoFilePath { get; set; }
        public string UserId { get; set; }
    }

    public class UserAccountModel
    {
        public IEnumerable<Movie> UMovies { get; set; }
        public string UserId { get; set; }
    }

    public class UserReviewModel
    {
        public string UserId { get; set; }
        public string MovieId { get; set; }
        public int Rate { get; set; }
        public string Content { get; set; }
    }
    public class UserMovieModel
    {
        public string UserId { get; set; }
        public string MovieId { get; set; }
    }
    
}
