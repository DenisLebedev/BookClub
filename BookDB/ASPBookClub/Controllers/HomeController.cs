using ASPBookClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Net;


namespace ASPBookClub.Controllers
{
    public class HomeController : Controller
    {
        private BookClubEntities db = new BookClubEntities();

        // GET: Home
        public ActionResult Index()
        {
            List<Book> list;
            list = User.Identity.Name != "" ? 
                RecommendedBooks(User.Identity.Name) : TopViewedBooks();


                return View(list);
        }

        private List<Book> TopViewedBooks()
        {
            return (from item in db.Books
                    orderby (item.Views) descending
                    select item).Take(10).ToList();
        }
        private List<Book> RecommendedBooks(string userN)
        {

            List<Review> userRead = (from item in db.Reviews
                                     where item.UserName == userN
                                     select item).ToList();

            
            IEnumerable<IGrouping<string, Review>> allusersRev =
                from bk in db.Reviews
                where bk.UserName != userN
                group bk by bk.UserName into groupedUser
                select groupedUser;


            int? highRat = 0;
            string commonU = null;
            foreach (var person in allusersRev)
            {
                int? temp = 0;

                foreach (var review in person)
                {
                    int? rating = userRead.Where(x => x.BookId ==
                            review.BookId).FirstOrDefault()?.Rating;
                    if (rating != null)
                        temp += rating * review.Rating;
                }

                if (temp > highRat)
                {
                    highRat = temp;
                    commonU = person.Key;
                }

            }

           

            IEnumerable<Book> tempList = (from item in db.Reviews
                                     where item.UserName == commonU
                                         && item.Rating >= 0
                                     orderby item.Rating descending
                                     select item.Book);

            List<Book> finalList = new List<Book>();
            int counter = 0;
            for(int i = 0; i < tempList.Count() && counter < 10; i++)
            {
                Book temp = userRead.Where(x => 
                    x.BookId == tempList.ElementAt(i).BookId).FirstOrDefault()?.Book;


                if(temp != null)
                {
                    counter++;
                    finalList.Add(temp);
                }

            }

            return finalList;
        }


        // GET: Book/Details/5
        public ActionResult DetailsBook(int? id)
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
            else
            {
                book.Views += 1;
                db.SaveChanges();
            }

            if (User.Identity.Name != "")
            {
                IncrementRatingForDisplay();
                ViewBag.averageRat = AverageRating(book);
                if (ViewBag.averageRat == null)
                    ViewBag.averageRat = "None";
                else
                    ViewBag.averageRat = 
                        Math.Round(ViewBag.averageRat,2);
            }

            return View(book);
        }


        private double? AverageRating(Book bk)
        {

            var avg = (from t in db.Reviews
                       where t.Book.BookId == bk.BookId
                       select new
                       {
                           Rating = t.Rating,
                       }).ToList().Average(x=> x.Rating);

            return avg;
        }


        private void IncrementRatingForDisplay()
        {
            foreach (var t in db.Reviews)
            {

                    if (t.Rating == -5)
                        t.Rating = 1;
                    else if (t.Rating == -3)
                        t.Rating = 2;
                    else if (t.Rating == 0)
                        t.Rating = 3;
                    else if (t.Rating == 3)
                        t.Rating = 4;
                    /*else if (t.Rating == 5)
                        t.Rating = 5;*/
                
            }
        } 

        // GET: Book/Create
        [Authorize]
        public ActionResult CreateBook()
        {
            return View();
        }

        // POST: Book/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateBook([Bind(Include = "BookId,Title,Description,Views")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(book);
        }

        // GET: Book/Edit/5
        [Authorize]
        public ActionResult EditBook(int? id)
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
            return View(book);
        }

        // POST: Book/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditBook([Bind(Include = "BookId,Title,Description,Views")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(book);
        }



        // GET: Author/Details/5
        [Authorize]
        public ActionResult DetailsAuthor(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Author author = db.Authors.Find(id);
            if (author == null)
            {
                return HttpNotFound();
            }

            Author temp = new Author()
            {
                Books = author.Books.OrderByDescending(x => x.Views).ToList(),
                FirstName = author.FirstName,
                LastName = author.LastName
                
            };

            return View(temp);
        }


        // GET: Author/Create
        [Authorize]
        public ActionResult CreateAuthor()
        {
            return View();
        }

        // POST: Author/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateAuthor([Bind(Include = "AuthorId,LastName,FirstName")] Author author)
        {
            if (ModelState.IsValid)
            {
                db.Authors.Add(author);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(author);
        }

        [Authorize]
        public ActionResult CreateReview(int? id)
        {
            return View();
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateReview(int? id, [Bind(Include = "")] )
        {
            return View();
        }*/


        // GET: Author/Edit/5
        [Authorize]
        public ActionResult EditAuthor(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Author author = db.Authors.Find(id);
            if (author == null)
            {
                return HttpNotFound();
            }
            return View(author);
        }

        // POST: Author/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditAuthor([Bind(Include = "AuthorId,LastName,FirstName")] Author author)
        {
            if (ModelState.IsValid)
            {
                db.Entry(author).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(author);
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
