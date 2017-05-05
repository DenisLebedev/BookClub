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

        //[Authorize]
        /*[HttpPost]
        public ActionResult Index()
        {
            return View(RecommendedBooks(User.Identity.Name));
        }
        
        private List<Review> RecommendedBooks(string userN)
        {

            List<Review> userRead = (from item in db.Reviews
                   where item.UserName == userN
                   select item).ToList();

            //any, all
            IEnumerable<IGrouping<string, Review>> allusersRev = 
                from bk in db.Reviews
                where bk.UserName != userN
                group bk by bk.UserName into groupedUser
                select groupedUser;


            int? highRat = 0;          
            string commonU = null;
            foreach(var person in allusersRev)
            {
                int? temp = 0;

                foreach (var review in person)
                {
                    int? rating = userRead.Where(x => x.Book.Title ==
                            review.Book.Title).FirstOrDefault()?.Rating;
                    if(rating != null)
                        temp += rating * review.Rating;
                }

                if (temp > highRat)
                {
                    highRat = temp;
                    commonU = person.Key;
                }

            }

            List<Review> tempList = (from item in db.Reviews
                                     where item.UserName == commonU
                                         && item.Rating >= 0
                                         && !userRead.Contains(item)
                                     orderby item.Rating descending
                                     select item).Take(10).ToList();

            foreach(Review t in tempList)
            {
                Review temp = userRead.Where(x => x.Book == t.Book).FirstOrDefault();

                if (temp != null)
                    tempList.Remove(temp);
            }


            return tempList; 
        }*/


        // GET: Home
        public ActionResult BookDetail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            ViewBag.averageRat =  AverageRating(book);

            if (ViewBag.averageRat == null)
                ViewBag.averageRat = "None";

            return View(book);
        }


        private double? AverageRating(Book bk)
        {

            var avg = (from t in db.Reviews
                       where t.Book.BookId == bk.BookId
                       select new
                       {
                           Rating = t.Rating,
                           //Views = t.Book.Views
                           }).ToList().Average(x=> x.Rating);

            /*double? temp = 0;
            foreach (var t in avg)
                temp += t.Rating;

            return temp / avg.ElementAt(0).Views;*/
            return avg;
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
        public ActionResult Login([Bind(Include = "UserName,Password")] User user, string returnUrl)
        {
            //db.Users.Where(x => x.UserName == user.UserName).Count() != 0
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

            //ViewBag.ReturnUrl = returnUrl;
            ModelState.AddModelError("", "Error: password or username invalid");
            return View();
        }

        
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
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
