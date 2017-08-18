using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Assignment2.Models;

namespace Assignment2.Controllers
{
    public class HomeController : Controller
    {
        private Assignment2DataContext dataContext;

        public HomeController(Assignment2DataContext context)
        {
            dataContext = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(dataContext.BlogPosts.Include(p => p.User).ToList());
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EditProfile(int id)
        {
            var profileToUpdate = (from m in dataContext.Users where m.UserId == id select m).FirstOrDefault();
            return View(profileToUpdate);
        }

        [HttpPost]
        public IActionResult ModifyProfile(User User)
        {
            var userid = HttpContext.Session.GetInt32("userId");

            var profileToUpdate = (from m in dataContext.Users where m.UserId == userid select m).FirstOrDefault();
            profileToUpdate.EmailAddress = User.EmailAddress;
            profileToUpdate.FirstName = User.FirstName;
            profileToUpdate.LastName = User.LastName;
            profileToUpdate.Password = User.Password;
            profileToUpdate.City = User.City;
            profileToUpdate.Country = User.Country;
            profileToUpdate.DateOfBirth = User.DateOfBirth;
            profileToUpdate.Address = User.Address;
            profileToUpdate.PostalCode = User.PostalCode;

            HttpContext.Session.SetString("emailAddress", User.EmailAddress);
            HttpContext.Session.SetString("firstName", User.FirstName);
            HttpContext.Session.SetString("lastName", User.LastName);

            dataContext.SaveChanges();
            return RedirectToAction("Index");

        }

        [HttpGet]
        public IActionResult EditBlogPost(int id)
        {
            var blogPostToUpdate = (from m in dataContext.BlogPosts where m.BlogPostId == id select m).FirstOrDefault();
            var photos = (from m in dataContext.Photos where m.BlogPostId == id select m);
            blogPostToUpdate.Content = blogPostToUpdate.Content.Replace("<br />", "\n");
            blogPostToUpdate.Photos = photos.ToList() ;
            
            return View(blogPostToUpdate);
        }

        [HttpPost]
        public IActionResult ModifyBlogPost(BlogPost BlogPost)
        {
            var postid = Convert.ToInt32(Request.Form["BlogPostId"]);

            var blogPostToUpdate = (from m in dataContext.BlogPosts where m.BlogPostId == postid select m).FirstOrDefault();
            blogPostToUpdate.Title = BlogPost.Title;
            blogPostToUpdate.Content = BlogPost.Content;
            blogPostToUpdate.Content = blogPostToUpdate.Content.Replace(System.Environment.NewLine, "<br />");
            
            dataContext.SaveChanges();
            return RedirectToAction("DisplayFullBlogPost", new { id = postid });

        }

        public IActionResult DeleteBlogPost(int id)
        {
            var blogPostToDelete = (from m in dataContext.BlogPosts where m.BlogPostId == id select m).FirstOrDefault();
            var comments = (from m in dataContext.Comments where m.BlogPostId == id select m);
            var photos = (from m in dataContext.Photos where m.BlogPostId == id select m);
            if (comments != null)
            {
                blogPostToDelete.Comments = (from m in dataContext.Comments where m.BlogPostId == id select m).Include(p => p.User).ToList();
            }
            if(photos != null)
            {
                blogPostToDelete.Photos = (from m in dataContext.Photos where m.BlogPostId == id select m).ToList();
            }
            foreach(var photo in blogPostToDelete.Photos)
            {
                dataContext.Remove(photo);
            }

            foreach(var comment in blogPostToDelete.Comments)
            {
                dataContext.Remove(comment);
            }
            dataContext.Remove(blogPostToDelete);
            dataContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddBlogPost()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddComment(int id)
        {
            return View(id);
        }

        [HttpGet]
        public IActionResult AddPhoto()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ViewPhotos()
        {
            var postId = Convert.ToInt32(ViewData["BlogPostId"]);
            var photos = (from m in dataContext.Photos where m.BlogPostId == postId select m);
            return View(photos.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(ICollection<IFormFile> files)
        {
            var postid = Convert.ToInt32(Request.Form["BlogPostId"]);

            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=cst8359;AccountKey=ecMPpNU6vimZKMDTJG4seALrY7Kq7UJYjgl0/yLanXn857C8xtUJ2sF4ciB6wy9gg+e/YeYbRTaly2DVOxWhXQ==");
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("chandlersphotostorage");
            await container.CreateIfNotExistsAsync();

            var permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            await container.SetPermissionsAsync(permissions);

            foreach (var file in files)
            {
                try
                {
                    // create the blob to hold the data
                    var blockBlob = container.GetBlockBlobReference(file.FileName);
                    if (await blockBlob.ExistsAsync())
                        await blockBlob.DeleteAsync();

                    using (var memoryStream = new MemoryStream())
                    {
                        // copy the file data into memory
                        await file.CopyToAsync(memoryStream);

                        // navigate back to the beginning of the memory stream
                        memoryStream.Position = 0;

                        // send the file to the cloud
                        await blockBlob.UploadFromStreamAsync(memoryStream);
                    }

                    // add the photo to the database if it uploaded successfully
                    var photo = new Photo();
                    photo.Url = blockBlob.Uri.AbsoluteUri;
                    photo.Filename = file.FileName;
                    photo.BlogPostId = postid;
                    
                    dataContext.Photos.Add(photo);
                    dataContext.SaveChanges();
                }
                catch
                {

                }
            }

            return RedirectToAction("EditBlogPost", new { id = postid });
        }

        [HttpGet]
        public IActionResult ViewBadWords()
        {
            return View(dataContext.BadWords.ToList());
        }

        [HttpPost]
        public IActionResult AddBadWord(BadWord badWord)
        {
            dataContext.BadWords.Add(badWord);
            dataContext.SaveChanges();

            return RedirectToAction("ViewBadWords");
        }

        [HttpGet]
        public IActionResult DisplayFullBlogPost(int id)
        {
            var postToDisplay = (from m in dataContext.BlogPosts where m.BlogPostId == id select m).Include(p=>p.User).FirstOrDefault();
            var comments = (from m in dataContext.Comments where m.BlogPostId == id select m);
            var photos = (from m in dataContext.Photos where m.BlogPostId == id select m);
            var badWords = (from m in dataContext.BadWords select m);
            if(comments != null)
            {
                postToDisplay.Comments = (from m in dataContext.Comments where m.BlogPostId == id select m).Include(p=>p.User).ToList();
            }
            if (photos != null)
            {
                postToDisplay.Photos = (from m in dataContext.Photos where m.BlogPostId == id select m).ToList();
            }
            foreach (var comment in comments)
            {
                foreach(var badword in badWords)
                {
                    comment.Content = Regex.Replace(comment.Content, badword.Word, "******", RegexOptions.IgnoreCase);
                }
            }
            return View(postToDisplay);
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult CreateBlogPost(BlogPost blogpost)
        {
            int userId = HttpContext.Session.GetInt32("userId").GetValueOrDefault();
            blogpost.UserId = userId;
            blogpost.Posted = DateTime.Now;
            blogpost.Content = blogpost.Content.Replace(System.Environment.NewLine, "<br />");
            blogpost.User = (from m in dataContext.Users where m.UserId == userId select m).FirstOrDefault();
            dataContext.BlogPosts.Add(blogpost);
            dataContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CreateComment(Comment comment)
        {
            comment.UserId = HttpContext.Session.GetInt32("userId").GetValueOrDefault();
            var blogpostid = comment.BlogPostId = Int32.Parse(Request.Form["BlogPostId"]);
            dataContext.Comments.Add(comment);
            dataContext.SaveChanges();
            
            return RedirectToAction("DisplayFullBlogPost", new { id = blogpostid });
        }

        [HttpPost]
        public IActionResult RegisterAction(User user)
        {
            try
            {
                dataContext.Users.Add(user);
                dataContext.SaveChanges();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult LoginAction(User user)
        {
            if (!dataContext.Users.Any(x => x.EmailAddress == user.EmailAddress))
            {
                return RedirectToAction("Index");
            }

            var currentUser = dataContext.Users.Where(x => x.EmailAddress == user.EmailAddress).FirstOrDefault();

            string password = dataContext.Users.Where(x => x.EmailAddress == user.EmailAddress).Select(x => x.Password).Single();

            if(!(password.Equals(user.Password)))
            {
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetString("emailAddress", currentUser.EmailAddress);
            HttpContext.Session.SetString("firstName", currentUser.FirstName);
            HttpContext.Session.SetString("lastName", currentUser.LastName);
            HttpContext.Session.SetString("roleId", currentUser.RoleId.ToString());
            HttpContext.Session.SetInt32("userId", currentUser.UserId);

            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult DeletePhoto(int id)
        {
            
            var photoToDelete = (from m in dataContext.Photos where m.PhotoId == id select m).FirstOrDefault();
            var postId = photoToDelete.BlogPostId;
            dataContext.Remove(photoToDelete);
            dataContext.SaveChanges();

            return RedirectToAction("EditBlogPost", new { id = postId });
        }

        public IActionResult DeleteBadWord(int id)
        {
            var wordToDelete = (from m in dataContext.BadWords where m.BadWordId == id select m).FirstOrDefault();
            dataContext.Remove(wordToDelete);
            dataContext.SaveChanges();

            return RedirectToAction("ViewBadWords");

        }
    }
}
