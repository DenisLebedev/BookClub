using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ASPBookClub.Models;

namespace ASPBookClub.Controllers
{
    public class UserController : Controller
    {
        private BookClubEntities db = new BookClubEntities();

        // GET: User
        //[Authorize (User = username)]
        public ActionResult Index(string username)
        {
            //ViewBag or class
            return View(RecommendedBooks(username));
        }
        
        private List<Review> RecommendedBooks(string userN)
        {
            /*List<Book> list = (from item in db.Books
                               orderby (item.Views) descending
                               select item).Take(10).ToList();*/

            List<Review> list = (from item in db.Reviews
                   where item.UserName == userN
                   select item).ToList();

            IEnumerable<IGrouping<string, Review>> test = 
                from bk in db.Reviews
                /*where list.Where(x => x.Book.Title == bk.Book.Title)
                      .First() != null*/
                group bk by bk.UserName into groupedUser
                orderby groupedUser.Key
                select groupedUser;


            int? highRat = 0;
            int? temp = 0;
            string commonU = "";
            foreach(var person in test)
            {
                foreach(var review in person)
                {
                    int? rating = list.Where(x => x.Book.Title ==
                            review.Book.Title).FirstOrDefault()?.Rating;
                    if(rating != null)
                        temp += rating * review.Rating;
                }

                if (temp > highRat)
                {
                    highRat = temp;
                    commonU = person.Key;
                }

                temp = 0;
            }

      
            return (from item in db.Reviews
                    where item.UserName == commonU
                    orderby item.Rating descending
                    select item).Take(10).ToList(); 
        }

        // GET: User/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

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
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }



        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "UserName,Password")] User user)
        {
            //db.Users.Where(x => x.UserName == user.UserName).Count() != 0
            if (ModelState.IsValid)
            {
                
                User userObj = (from item in db.Users
                                where item.UserName == user.UserName 
                                    && item.Password == user.Password
                                select item).FirstOrDefault();


                if (userObj != null)
                {
                    return RedirectToAction("Index", "User", new { userObj.UserName });
                }
            }

            //ViewBag.ReturnUrl = returnUrl;
            ModelState.AddModelError("", "Error: password or username invalid");
            return View(user);
        }




        // GET: User/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserName,Password,LastName,FirstName,Email,Country")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: User/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
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
