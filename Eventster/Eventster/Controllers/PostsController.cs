using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Web;
using Eventster.Models;
//using System.Web.Mvc;
using TweetSharp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;


namespace Eventster.Controllers
{
    public class PostsController : Controller
    {
        public ActionResult Index()
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                return View();
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult Index(Tweet twts)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                string key = "eaTMngVt6Sr3pkLePBKtADnzV";
                string secret = "Vthx5wASD9ylmnxxbN3aWo69mEQIkJpSA24LSLwK4FaCuIvAdk";
                string token = "47937529-64ArtdTmPvjzI6scKmyXDLRmfOBEIVSseWDwE7lYb";
                string tokenSecret = "QyfYNGdw70bRgsf2cbGqkqWKUV1x42g880wO2iWm2XIWR";

                var service = new TweetSharp.TwitterService(key, secret);
                service.AuthenticateWith(token, tokenSecret);

                string message = twts.tweets;

                service.SendTweet(new SendTweetOptions
                {
                    Status = message
                });

                var responseText = service.Response.StatusCode;

                twts.tweets = "";
                TempData["tweet_msg"] = "<script>alert('Successful tweet.');</script>";
                return View();
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
