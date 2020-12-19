using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using _300958687_kim__Lab3.Data;
using _300958687_kim__Lab3.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Net.Http.Headers;
using Syroot.Windows.IO;

namespace _300958687_kim__Lab3.Controllers
{
    public class MovieController : Controller
    {
        private IAmazonDynamoDB dynamoDBClient;
        private readonly string bucketName = "mk-videorepository";
        private IAmazonS3 S3Client { get; set; }
        private string userId;

        public MovieController(IAmazonDynamoDB dynamoDBClient,IAmazonS3 S3Client)
        {
            this.dynamoDBClient = dynamoDBClient;
            this.S3Client = S3Client;
        }
        
        public async Task<IActionResult> NavWatch(string movieId,string userId)
        {
            if (movieId == null)
            {
                return NotFound();
            }
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            Movie movie = await service.GetMovieAsync(movieId);
            User user = await service.GetUserAsync(userId);
             ViewData["Message"] = "";
            ViewData["Movie"] = movie;
            ViewData["User"] = user;
            return View();
        }
        public async Task<int> CreateBucketAsync()
        {
            try
            {
                if (!(await AmazonS3Util.DoesS3BucketExistV2Async(S3Client, bucketName)))
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true
                    };

                    PutBucketResponse putBucketResponse = await S3Client.PutBucketAsync(putBucketRequest);
                }
                else
                {
                    return 1;
                }
            }
            catch (AmazonS3Exception e)
            {
                //MessageBoxResult ms = MessageBox.Show(String.Format("Error while creating bucket. Message : \n{0}", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 2;
            }
            catch (Exception e)
            {
                //MessageBoxResult ms = MessageBox.Show(String.Format("Unknown Error while creating bucket. Message : \n{0}", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 2;
            }
            return 0;
        }
        public async Task<IActionResult> DisplayMovies(string userid)
        {
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            UserAccountModel ua = new UserAccountModel()
            {
                UserId = userid,
                UMovies = await service.GetMoviesAsync()
            };
            return View("DisplayMovies", ua);
        }
        public IActionResult Upload(string id)
        {
            //userId = id;
            ViewBag.newUI = id;
            ViewBag.ErrMsg = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([Bind("Name,VideoFilePath,UserId")] CreateMovieModel cmm)
        {
            //first check bucket status
            int res = await CreateBucketAsync();
            if(res == 0 || res == 1)
            {
                //upload file to s3 bucket
                if (ModelState.IsValid)
                {
                    //cmm.UserId = userId;
                    DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
                    DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
                    //string fileTitle = Path.GetFileName(cmm.VideoFilePath);
                    string fileTitle = ContentDispositionHeaderValue
                                         .Parse(cmm.VideoFilePath.ContentDisposition)
                                         .FileName
                                         .TrimStart().ToString();
                    User foundUser = await service.GetUserAsync(cmm.UserId);
                    Movie newMovie = new Movie
                    {
                        Name = cmm.Name,
                        UploadedDate = DateTime.Now.ToString("MM/dd/yyyy"),
                        Video = S3Link.Create(context, bucketName, fileTitle, Amazon.RegionEndpoint.USWest2),
                        UploadedBy = foundUser,
                        Reviews = new List<Review>()

                    };

                    //insert the data into dynamoDB                           
                    Movie InsertedMovie = await service.InsertMovie(newMovie);

                    //upload the file to S3 bucket 
                    bool status;
                    using (var filestream = cmm.VideoFilePath.OpenReadStream())
                    using (var ms = new MemoryStream())
                    {
                        await filestream.CopyToAsync(ms);
                        status = await UploadFileAsync(ms, newMovie.Video);
                    }
                    UserAccountModel uam = new UserAccountModel { UMovies = await service.GetMoviesAsync(), UserId = cmm.UserId };
                    return View("DisplayMovies", uam);
                }
                return View(cmm);
            } else
            {
                if (ModelState.IsValid)
                {
                    ViewBag.newUI = cmm.UserId;
                    ViewBag.ErrMsg = "Error while creating bucket name " + bucketName;
                }
            }
            return View("Upload");
            
        }

        public async Task<bool> UploadFileAsync(Stream fileStream, S3Link lnkobj)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(S3Client);

                var fileUploadRequest = new TransferUtilityUploadRequest()
                {
                    CannedACL = S3CannedACL.PublicRead,
                    BucketName = lnkobj.BucketName,
                    Key = lnkobj.Key,
                    InputStream = fileStream
                };

                await fileTransferUtility.UploadAsync(fileUploadRequest);

                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    // add error logging
                }
                else
                {
                    //add error handling
                }
                return false;
            }
        }
        [HttpPost]
        public async Task<IActionResult> Download(string MovieId,string UserId)
        {
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            Movie m = await service.GetMovieAsync(MovieId);
            bool res = await DownloadAsync(m.Video);
            User u = await service.GetUserAsync(UserId);
            //alert message after the download is done.
            //open file dialog when done
            ViewData["Message"] = "Downloaded!";
            ViewData["Movie"] = m;
            ViewData["User"] = u;
           

            return View("NavWatch");
        }
        public async Task<bool> DownloadAsync(S3Link s3l)
        {
            try
            {
                TransferUtility fileTransferUtility = new TransferUtility(S3Client);
                string downloadpath = new KnownFolder(KnownFolderType.Downloads).Path;
                //string downloadpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos);
                string finalPath = Path.Combine(downloadpath, s3l.Key);
                var request = new TransferUtilityDownloadRequest()
                {
                    BucketName = s3l.BucketName,
                    Key = s3l.Key,
                    FilePath = finalPath
                };
                await fileTransferUtility.DownloadAsync(request);
                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    // add error logging
                }
                else
                {
                    //add error handling
                }
                return false;
            }
            
        }
        public async Task<IActionResult> AddReview([Bind("UserId,MovieId,Rate,Content")] UserReviewModel userReviewModel)
        {
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            User user = await service.GetUserAsync(userReviewModel.UserId);
            Movie movie = await service.GetMovieAsync(userReviewModel.MovieId);

            Review review = new Review
            {
                User = user,
                Movie = movie,
                CreatedDate = DateTime.Now.ToString("MM/dd/yyyy h:mm tt"),
                Rate = userReviewModel.Rate,
                Content = userReviewModel.Content
            };

            //save review
            Review savedReview = await service.InsertReview(review);
            //update movie
            if(movie.Reviews == null)
            {
                List<Review> reviews4Movie = new List<Review>();
                movie.Reviews = reviews4Movie;
            }
            movie.Reviews.Add(savedReview);
            await service.UpdateMovie(movie);
    
            return RedirectToAction("NavWatch", new { userId = user.Id, movieId = movie.Id});
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            Movie movieB4Delete = await service.GetMovieAsync(id);
            await service.DeleteMovieAsync(id);
            return RedirectToAction("DisplayMovies",new { userid = movieB4Delete.UploadedBy.Id});
        }

    }
}
