using ASPBookClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Net;
using System.Web.Routing;
using System.Linq.Expressions;

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
               
            }
        } 

        // GET: Book/Create
        [Authorize]
        public ActionResult CreateBook()
        {
            /*Author author1 = new Author();
            Author author2 = new Author();

            ViewBag.LnOne = new SelectList(db.Authors, "LastName", "LastName", (author1.LastName + 
                " " + author1.FirstName));
            ViewBag.LnTwo = new SelectList(db.Authors, "LastName", "LastName", author2.LastName);


            Book book = new Book();
            book.Authors.Add(author1);
            book.Authors.Add(author2);*/

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "None" , Value = "default", Selected = true});
            foreach (Author t in db.Authors)
            {
                items.Add(new SelectListItem { Text = (t.LastName + ", " + t.FirstName),
                    Value = (t.LastName + ", " + t.FirstName)});
            }

            ViewBag.SearchList = items;
            ViewBag.SelectedAuthOne = null;
            ViewBag.SelectedAuthTwo = null;
            return View();
        }

        // POST: Book/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateBook([Bind(Include = "BookId,Title,Description")] Book book)
        {

            if(ViewBag.SelectedAuthOne == null || ViewBag.SelectedAuthTwo == null || book == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                string temp = ViewBag.SelectedAuthOne;
                string[] auth = temp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

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
            
            if (ModelState.IsValid || User.Identity.Name != "")
            {

                int temp = (from t in db.Authors
                            where t.FirstName == author.FirstName &&
                                 t.LastName == author.LastName
                            select t).Count();

                if (temp == 0)
                {

                    db.Authors.Add(author);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ModelState.AddModelError("", "Error: the author exist already");

            return View(author);
        }

        [Authorize]
        public ActionResult CreateReview(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            Book book = db.Books.Find(id);
            User user = db.Users.Find(User.Identity.Name);

            if (book == null || user == null)
                return HttpNotFound();

            /*if(ViewBag.bookObj == null)
                return HttpNotFound();*/

            Review review = new Review()
            {
                BookId = book.BookId,
                Book = book,
                UserName = user.UserName,
                User = user
            };

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateReview([Bind(Include = "Bookid, UserName, Rating, Content")] Review review)
        {

            if (review == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(review.UserName);
            Book book = db.Books.Find(review.BookId);

            if (user == null || book == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                if (review.Rating > 0 || review.Rating < 6)
                {
                    review.Rating = (int?)review.Rating;
                    DecrementRatingsForDB();
                    
                    review.Book = book;
                    review.User = user;
                    DecrementOneRatingObj(review);
                    
                    //adding order
                    db.Reviews.Add(review);
                    db.SaveChanges();
                    RedirectToAction("Index");
                }
            }

            ModelState.AddModelError("", "Error: the rating code can only be between 0 and 5");


            return View(review);
        }

        private void DecrementOneRatingObj(Review t)
        {
            if (t.Rating == 1)
                t.Rating = -5;
            else if (t.Rating == 2)
                t.Rating = -3;
            else if (t.Rating == 3)
                t.Rating = 0;
            else if (t.Rating == 4)
                t.Rating = 3;

        }

        private void DecrementRatingsForDB()
        {
            foreach (var t in db.Reviews)
            {

                if (t.Rating == 1)
                    t.Rating = -5;
                else if (t.Rating == 2)
                    t.Rating = -3;
                else if (t.Rating == 3)
                    t.Rating = 0;
                else if (t.Rating == 4)
                    t.Rating = 3;

            }
        }

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
