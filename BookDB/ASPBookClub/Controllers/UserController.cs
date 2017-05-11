using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ASPBookClub.Models;
using System.Web.Security;

namespace ASPBookClub.Controllers
{
    public class UserController : Controller
    {
        private BookClubEntities db = new BookClubEntities();
       


        // GET: User/Create
        /// <summary>
        /// The get method will redirect to the right page where
        /// the user will be able to create a new account.
        /// </summary>
        /// <returns> redirect to the right view</returns>
        public ActionResult Register()
        {
            return View();
        }


        // POST: User/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Allow to create and a new user to our database following
        /// certain conditions. The username given should not be in the
        /// database already.
        /// </summary>
        /// <param name="user"></param>
        /// <returns> redirect to the right page depending on the success or failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "UserName,Password,LastName,FirstName,Email,Country")] User user)
        {
            if (ModelState.IsValid)
            {
                User temp = db.Users.Find(user.UserName);

                //User does not exist
                if (temp == null)
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Error: username already tooken");
            return View(user);
        }



        /// <summary>
        /// the get method redirect to the right
        /// view, but saves url given in a viewbag to return after.
        /// </summary>
        /// <param name="returnUrl"> represent the last page that the user was</param>
        /// <returns>redirect to the right view </returns>
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        /// <summary>
        /// Login a user that will be allow to have access extra options after.
        /// To ensure that the user does exist I compare the given username with
        /// all the usernames inside the database. I use a cookie to remember
        /// which user is logged.
        /// </summary>
        /// <param name="user">a new user object</param>
        /// <param name="returnUrl">represent the last page that the user was</param>
        /// <returns>redirect to the right page depending on the success or failure</returns>
        [HttpPost]
        public ActionResult Login([Bind(Include = "UserName,Password")] User user, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                
                
                User userObj = (from item in db.Users
                                where item.UserName == user.UserName 
                                    && item.Password == user.Password
                                select item).FirstOrDefault<User>();

                //User exist
                if (userObj != null)
                {
                  
                    //Use cookies to keep the user on
                    FormsAuthentication.RedirectFromLoginPage(userObj.UserName, false);
                }
            }

            //Recreate the viewbag because it is errased
            ViewBag.ReturnUrl = returnUrl;
            ModelState.AddModelError("", "Error: password or username invalid");
            return View();
        }

        
        /// <summary>
        /// Delete the cookie and redirect the user
        /// to the main page with the state of an unauthorized 
        /// user.
        /// </summary>
        /// <returns> redirect to the main page </returns>
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// Allow to free the ressources when the task
        /// is completed. Like close the database connection.
        /// </summary>
        /// <param name="disposing"> boolean </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
