using _300958687_kim__Lab3.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _300958687_kim__Lab3.Data
{
    public class DynamoDBServices
    {
        private IAmazonDynamoDB dynamoDBClient { get; set; }

        public DynamoDBServices(IAmazonDynamoDB dynamoDBClient)
        {
            this.dynamoDBClient = dynamoDBClient;
        }

        //user//
        public async Task<User> InsertUser(User user)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            // Add a unique id for the primary key.
            user.Id = System.Guid.NewGuid().ToString();
            await context.SaveAsync(user, default(System.Threading.CancellationToken));
            User newUser = await context.LoadAsync<User>(user.Id, default(System.Threading.CancellationToken));
            return user;
        }

        public async Task<User> GetUserAsync(string Id)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            User newUser = await context.LoadAsync<User>(Id, default(System.Threading.CancellationToken));
            return newUser;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            ScanFilter filter = new ScanFilter();
            filter.AddCondition("Email", ScanOperator.Equal, email);
            ScanOperationConfig soc = new ScanOperationConfig()
            {
                Filter = filter
            };
            List<User> resUser = new List<User>();
            AsyncSearch<User> search = context.FromScanAsync<User>(soc,null);
            do
            {
                resUser = await search.GetNextSetAsync(default(System.Threading.CancellationToken));
            } while (!search.IsDone);
            User user = resUser.First();
            
            //User user = await context.LoadAsync<User>(email);
            return user;
        }

        //Movie//
        public async Task<Movie> InsertMovie(Movie movie)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            movie.Id = System.Guid.NewGuid().ToString();
            //movie.UploadedDate = DateTime.Now.ToString();
            await context.SaveAsync(movie, default(System.Threading.CancellationToken));
            //User newUser = await context.LoadAsync<User>(movie.Id, default(System.Threading.CancellationToken));
            return movie;
        }
        public async Task DeleteMovieAsync(string Id)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            Movie tempmov = await context.LoadAsync<Movie>(Id, default(System.Threading.CancellationToken));
            foreach(Review rev in tempmov.Reviews)
            {
                await context.DeleteAsync<Review>(rev.Id, default(System.Threading.CancellationToken));
            }
            await context.DeleteAsync<Movie>(Id, default(System.Threading.CancellationToken));
            //also delete reviews here
            
        }
        public async Task<Movie> UpdateMovie(Movie movie)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            await context.SaveAsync(movie, default(System.Threading.CancellationToken));
            return movie;
        }
        public async Task<List<Movie>> GetMoviesAsync()
        {
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("Id", ScanOperator.NotEqual, 0);

            ScanOperationConfig soc = new ScanOperationConfig()
            {
                // AttributesToGet = new List { "Id", "Title", "ISBN", "Price" },
                Filter = scanFilter
            };
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            AsyncSearch<Movie> search = context.FromScanAsync<Movie>(soc, null);
            List<Movie> documentList = new List<Movie>();
            do
            {
                documentList = await search.GetNextSetAsync(default(System.Threading.CancellationToken));
            } while (!search.IsDone);

            return documentList;
        }
        public async Task<Movie> GetMovieAsync(string Id)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            Movie newMovie = await context.LoadAsync<Movie>(Id, default(System.Threading.CancellationToken));
            return newMovie;
        }
        public async Task<Review> InsertReview(Review review)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            review.Id = System.Guid.NewGuid().ToString();
            await context.SaveAsync(review, default(System.Threading.CancellationToken));
            Review newReview = await context.LoadAsync<Review>(review.Id, default(System.Threading.CancellationToken));
            return newReview;

        }
        public async Task DeleteReviewAsync(string Id)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            await context.DeleteAsync<Review>(Id, default(System.Threading.CancellationToken));

        }

        public async Task<List<Review>> GetReviewsAsync()
        {
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("Id", ScanOperator.NotEqual, 0);

            ScanOperationConfig soc = new ScanOperationConfig()
            {
                Filter = scanFilter
            };
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            AsyncSearch<Review> search = context.FromScanAsync<Review>(soc, null);
            List<Review> documentList = new List<Review>();
            do
            {
                documentList = await search.GetNextSetAsync(default(System.Threading.CancellationToken));
            } while (!search.IsDone);

            return documentList;
        }

    }
}
