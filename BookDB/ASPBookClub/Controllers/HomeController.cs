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
        /// <summary>
        /// This is a get method that will render the main page for an authenticated
        /// and a non-authenticated user. For an authenticated user I will display
        /// the top 10 recommended books from the best matching user else I will
        /// show the top 10 most viewed books.
        /// </summary>
        /// <returns> list of books that I will display to the user </returns>
        public ActionResult Index()
        {
            List<Book> list;
            //Ternary operator to identify if a user is logged or not
            list = User.Identity.Name != "" ? 
                RecommendedBooks(User.Identity.Name) : TopViewedBooks();


                return View(list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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

           

            List<Book> tempList = (from item in db.Reviews
                                     where item.UserName == commonU
                                         && item.Rating >= 0
                                         && item.UserName != userN
                                     orderby item.Rating descending
                                     select item.Book).ToList();



            /*List<Book> finalList = new List<Book>();
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

            }*/

            return tempList;
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
            Book copy = null;
            if (User.Identity.Name != "")
            {
                
                copy  = IncrementRatingForDisplay(book);
                ViewBag.averageRat = AverageRating(copy);
                if (ViewBag.averageRat == null)
                    ViewBag.averageRat = "None";
                else
                    ViewBag.averageRat = 
                        Math.Round(ViewBag.averageRat,2);
            }

            return View(copy);
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


        private Book IncrementRatingForDisplay(Book book)
        {
            Book copy = new Book()
            {
                BookId = book.BookId,
                Authors = book.Authors,
                Description = book.Description,
                Title = book.Title
            };

            ICollection<Review> reviewChanged = new List<Review>();
    

            foreach (var t in book.Reviews)
            {

                reviewChanged.Add(new Review() {
                    Book = t.Book,
                    BookId = t.BookId,
                    Content = t.Content,
                    ReviewId = t.ReviewId,
                    User = t.User,
                    UserName = t.UserName,
                    Rating = IncrementRating(t.Rating)

                });                                
            }

            copy.Reviews = reviewChanged;
            return copy;
        } 


        private int? IncrementRating(int? rating)
        {
            if (rating == -5)
                rating = 1;
            else if (rating == -3)
                rating = 2;
            else if (rating == 0)
                rating = 3;
            else if (rating == 3)
                rating = 4;
            

            return rating;
        }


        // GET: Book/Create
        [Authorize]
        public ActionResult CreateBook()
        {

            CreateListForDropDown();



            return View();
        }

        // POST: Book/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateBook([Bind(Include = "BookId,Title,Description")] Book book,
            string searchListOne, string searchListTwo)
        {

            if(book == null)
                return HttpNotFound();

            if (searchListOne == "default")
            {
                ModelState.AddModelError("", "Error: you must select an author from the first dropdown");
                return View(book);
            }

            if (ModelState.IsValid)
            {
                string[] authLnFn = searchListOne.Split(' ');
                Author auth1 = GetAuthorPerName(authLnFn[1], authLnFn[0]);

                if (searchListTwo != "default")
                {
                    authLnFn = searchListTwo.Split(' ');
                    Author auth2 = GetAuthorPerName(authLnFn[1], authLnFn[0]);

                    if (auth1 == auth2)
                    {
                        CreateListForDropDown();
                        ModelState.AddModelError("", "Error: you selected the same author twice");
                        return View(book);
                    }

                    book.Authors.Add(auth1);
                    book.Authors.Add(auth2);
                } else
                {
                    book.Authors.Add(auth1);
                }




                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(book);
        }


        private void CreateListForDropDown()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "None", Value = "default", Selected = true });
            foreach (Author t in db.Authors)
            {
                items.Add(new SelectListItem
                {
                    Text = (t.LastName + ", " + t.FirstName),
                    Value = (t.LastName + " " + t.FirstName)
                });
            }

            ViewBag.SearchListOne = items;
            ViewBag.SearchListTwo = items;
        }

        private Author GetAuthorPerName(string fn, string ln)
        {
            return (from t in db.Authors
                    where t.LastName == ln &&
                          t.FirstName == fn
                    select t).FirstOrDefault();
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
        public ActionResult CreateReview([Bind(Include = "Bookid, UserName, Rating, Content")] Review review, 
            Nullable<int> rating)
        {

            if (review == null || rating == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(review.UserName);
            Book book = db.Books.Find(review.BookId);

            if (user == null || book == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                if (rating >= 0 && rating < 6)
                {
                    
                    review.Rating = rating;
                    
                    review.Book = book;
                    review.User = user;
                    DecrementOneRatingObj(review);
                    
                    //adding order
                    db.Reviews.Add(review);
                    db.SaveChanges();
                    return RedirectToAction("Index");
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
