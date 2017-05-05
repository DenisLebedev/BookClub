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
        public ActionResult Register()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "UserName,Password,LastName,FirstName,Email,Country")] User user)
        {
            if (ModelState.IsValid)
            {
                User temp = db.Users.Find(user.UserName);

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



        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login([Bind(Include = "UserName,Password")] User user, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                
                User userObj = (from item in db.Users
                                where item.UserName == user.UserName 
                                    && item.Password == user.Password
                                select item).FirstOrDefault<User>();


                if (userObj != null)
                {
                    FormsAuthentication.RedirectFromLoginPage(userObj.UserName, false);
                }
            }

            ModelState.AddModelError("", "Error: password or username invalid");
            return View();
        }

        
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        } 
             
       

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
