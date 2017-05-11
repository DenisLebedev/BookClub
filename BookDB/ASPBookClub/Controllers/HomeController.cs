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
    /// <summary>
    /// This class handle the basic task of a library online.
    /// Capacity to add new books, authors. See details
    /// of an author and a book. This class deal with
    /// authorized and non-authorized user. Also, 
    /// a recommendation system is made for authorized user. The
    /// display will change in the variation of an authorized
    /// and a non-authorized user.
    /// </summary>
    public class HomeController : Controller
    {
        private BookClubEntities db = new BookClubEntities();


        /**
         * URL: http://waldo2.dawsoncollege.qc.ca/1532444/project/
         * ISSUE:  the redirect works while running c# code, but is not working for
         *         waldo2. The issue happen only when the user try to connect else 
         *         everything works.
         */


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


        public ActionResult SeeAllBooks()
        {
            return View((from t in db.Books
                         select t).ToList());
        }

        /// <summary>
        /// Filter our database to find the 10 most viewed
        /// books
        /// </summary>
        /// <returns> list of books that has the highest number of views </returns>
        private List<Book> TopViewedBooks()
        {
            return (from item in db.Books
                    orderby (item.Views) descending
                    select item).Take(10).ToList();
        }

        /// <summary>
        /// Processing all the users that have read the same book as the 
        /// given user. Then with all the users found a mathematical operation is made 
        /// to find the best match. To continue, we take 10 books that the given user
        /// did not read from the best matched user.
        /// </summary>
        /// <param name="userN"> username currently connected </param>
        /// <returns></returns>
        private List<Book> RecommendedBooks(string userN)
        {
            //represent all the reviews made by the given user
            List<Review> userRead = (from item in db.Reviews
                                     where item.UserName == userN
                                     select item).ToList();

            //Grouping each user with their reviews
            IEnumerable<IGrouping<string, Review>> allusersRev =
                (from bk in db.Reviews
                where bk.UserName != userN
                group bk by bk.UserName into groupedUser
                select groupedUser).ToList();


            //Used variable to find the highest rating
            int? highRat = 0;

            //Will hold the most common user (best match)
            string commonU = null;

            //Iterating throught the grouped list
            foreach (var person in allusersRev)
            {
                //Will hold ratings
                int? temp = 0;

                //Iterating to all the reviews made by person X
                foreach (var review in person)
                {
                    //Grabing the rating that person X made for the book that the given user read
                    int? rating = userRead.Where(x => x.BookId ==
                            review.BookId).FirstOrDefault()?.Rating;

                    //Rating is nullable
                    if (rating != null)
                        temp += rating * review.Rating;
                }

                //If temp is bigger than our highest rating we should swap
                //and save the username of that user that has a better score
                if (temp > highRat)
                {
                    highRat = temp;
                    commonU = person.Key;
                }

            }

           
            /**
             * Taking 10 books from the best matched user.
             * These books should match his name and a rating
             * higher than -1. Also, this book should not match
             * the book that the given user already read.
             */
             return (from item in db.Reviews
                                     where item.UserName == commonU // match what the best user read
                                         && item.Rating >= 0
                                         && item.UserName != userN //remove what the given user read
                                     orderby item.Rating descending
                                     select item.Book).Take(10).ToList();

        }


        // GET: Book/Details/5
        /// <summary>
        /// The get method will ensure some viewbag that will hold
        /// the average rating, but only for a authorized user.
        /// Also, the book that will be returned is has new rating values
        /// for the user purpose.
        /// The display in the view will also change depending the state of 
        /// the user.
        /// </summary>
        /// <param name="id">the book id</param>
        /// <returns> deep copy of a book </returns>
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
                /**
                 * If the book exist that mean the user
                 * could enter inside the website so we increment
                 * the number of view and save the changes
                 */ 
                book.Views += 1;
                db.SaveChanges();
            }

           
            if (User.Identity.Name != "")
            {
                
                //Creating a copy of the given book and change the rating for the UI
                Book copy  = IncrementRatingForDisplay(book);

                //Calculating the average
                ViewBag.averageRat = AverageRating(copy);

                //The method can return null so a 'default' value is necessary
                if (ViewBag.averageRat == null)
                    //The user will know that no review was made
                    ViewBag.averageRat = "None";
                else
                    //The choice about how much a person will round the rating should be give
                    ViewBag.averageRat = 
                        Math.Round(ViewBag.averageRat,2);

                return View(copy);
            }

            return View(book);
        }

        /// <summary>
        /// Calculate the average rating made
        /// by all the users from the give book.
        /// </summary>
        /// <param name="bk"> a book object </param>
        /// <returns>nullable integer</returns>
        private double? AverageRating(Book bk)
        {

            /**
             * Grab all the books that has the same id
             * which should be unique and return a  
             * anonymous object. Then we create a list to
             * process the result and get the average from 
             * it using existant method.
             */
            var avg = (from t in bk.Reviews
                       select new
                       {
                           Rating = t.Rating,
                       }).ToList().Average(x=> x.Rating);

            return avg;
        }


        /// <summary>
        /// The method will take the data from the given book
        /// and create a deep copy of it. This way nothing will
        /// change in our database and avoid errors. It is important
        /// because this method will change the rating of the given book
        /// specifically for the user view.
        /// </summary>
        /// <param name="book"> book object</param>
        /// <returns>deep copy fo a book object with a new rating </returns>
        private Book IncrementRatingForDisplay(Book book)
        {
            //Copy everything from the given book, but not the ratings
            Book copy = new Book()
            {
                BookId = book.BookId,
                Authors = book.Authors,
                Description = book.Description,
                Title = book.Title
            };

            //Ratings are an ICollection
            ICollection<Review> reviewChanged = new List<Review>();
    
            //Adding each review with a little change
            foreach (var t in book.Reviews)
            {

                reviewChanged.Add(new Review() {
                    Book = t.Book,
                    BookId = t.BookId,
                    Content = t.Content,
                    ReviewId = t.ReviewId,
                    User = t.User,
                    UserName = t.UserName,
                    //Changing the rating
                    Rating = IncrementRating(t.Rating)

                });                                
            }
            
            //Adding the new reviews
            copy.Reviews = reviewChanged;
            return copy;
        } 


        /// <summary>
        /// Increment the rating for the UI.
        /// </summary>
        /// <param name="rating">integer that represent the current rating</param>
        /// <returns></returns>
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
            else if (rating == 5)
                rating = 5;
            /*else
                //Default value
                rating = null;*/

            return rating;
        }


        // GET: Book/Create
        /// <summary>
        /// The get method will ensure that the necessary
        /// viewbags are made to display the dropdown and used to 
        /// redirect to the right view.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult CreateBook()
        {

            //Creation of viewbags
            CreateListForDropDown();

            return View();
        }

        // POST: Book/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The post method will ensure that the given data
        /// is right and will adda new book to the database.
        /// The method is only available for an autorized user.
        /// </summary>
        /// <param name="book">the new book object</param>
        /// <param name="searchListOne">the choosed author in the first drop down</param>
        /// <param name="searchListTwo">the choosed author in the second drop down</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateBook([Bind(Include = "BookId,Title,Description")] Book book,
            string searchListOne, string searchListTwo)
        {

            if(book == null)
                return HttpNotFound();

            //The first one must be selected
            if (searchListOne == "default")
            {
                //Recreating the dropdowns 
                CreateListForDropDown();

                //Meaningful message
                ModelState.AddModelError("", "Error: you must select an author from the first dropdown");
                return View(book);
            }

            if (ModelState.IsValid)
            {   
                //The way that value of the viewbags are setted allow to use the split method
                string[] authLnFn = searchListOne.Split(' ');

                //Index 0 :Last name, Index 1: First name, the method return the current author
                Author auth1 = GetAuthorPerName(authLnFn[1], authLnFn[0]);

                //The user chooed 2 authors
                if (searchListTwo != "default")
                {
                    //Perform the same action on the second selected element
                    authLnFn = searchListTwo.Split(' ');

                    Author auth2 = GetAuthorPerName(authLnFn[1], authLnFn[0]);

                    //If the two objects are the same the user should restart
                    if (auth1 == auth2)
                    {
                        //Recreate the dropdowns
                        CreateListForDropDown();

                        //Meaningful message
                        ModelState.AddModelError("", "Error: you selected the same author twice");
                        return View(book);
                    }

                    //If they are not the same object we can easily add them
                    book.Authors.Add(auth1);
                    book.Authors.Add(auth2);

                } else
                {
                    //Else we only add one book
                    book.Authors.Add(auth1);
                }



                //Add the book
                db.Books.Add(book);

                //Save changes
                db.SaveChanges();

                //Redirect to the main page
                return RedirectToAction("Index");
            }

            return View(book);
        }


        /// <summary>
        /// Creates 2 viewbag that will be used to display
        /// two dropdown in the CreateBook method. 
        /// The viewbags will hold all authors so the user
        /// can select one
        /// </summary>
        private void CreateListForDropDown()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            //First value that the user will see or choose if they do not want to link a author
            items.Add(new SelectListItem { Text = "None", Value = "default", Selected = true });

            //Iterating through all authors in our database
            foreach (Author t in db.Authors)
            {
                //For each author we create a new item
                items.Add(new SelectListItem
                {   
                    //What the user will see
                    Text = (t.LastName + ", " + t.FirstName),
                    //What the data is actually
                    Value = (t.LastName + " " + t.FirstName)
                });
            }

            //Adding to the viewBags
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
        /// <summary>
        /// The get request will print the details of an author.
        /// An author has written many books and so we will
        /// create a new author object and give him all the books
        /// that author written sorted by the number of views.
        /// The method is only available for an autorized user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns> redirect to a view wiht a new author object</returns>
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

            /**
             * The author will contain a list of books sorted
             * that why creating a new object is necessary.  
             */
            Author temp = new Author()
            {
                Books = author.Books.OrderByDescending(x => x.Views).ToList(),
                FirstName = author.FirstName,
                LastName = author.LastName
                
            };

            return View(temp);
        }


        // GET: Author/Create
        /// <summary>
        /// Theg et method just redirect to the
        /// right view and only an authorized user can have
        /// access.
        /// </summary>
        /// <returns>redirect to the right view</returns>
        [Authorize]
        public ActionResult CreateAuthor()
        {
            return View();
        }

        // POST: Author/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Ensure that the new author object that will be created will match
        /// the database conditions. The first and last name should be
        /// unique when we are adding new authors due to our settings.
        /// </summary>
        /// <param name="author">new author object</param>
        /// <returns>if there is issues then I redirect to the create view again</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateAuthor([Bind(Include = "AuthorId,LastName,FirstName")] Author author)
        {
            //Invalid input
            if (author.FirstName == "" || author.LastName == "")
            {
                ModelState.AddModelError("", "Error: The name given is empty");
                return View(author);
            }

            //If the user is not connected we do not go in - extra validation
            if (ModelState.IsValid || User.Identity.Name != "")
            {
                //If temp has 0 that mean we did not found an author with
                //the given first and last name
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

        /// <summary>
        /// The get method will create a new review object
        /// with already some data inside using the given id.
        /// Only an authorized user should have access to this plugin.
        /// </summary>
        /// <param name="id">the id of a book</param>
        /// <returns>redirect to the right view</returns>
        [Authorize]
        public ActionResult CreateReview(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            //Grab the data necessary to create a new review
            Book book = db.Books.Find(id);
            User user = db.Users.Find(User.Identity.Name);

            //Ensure that the book and the user exist
            if (book == null || user == null)
                return HttpNotFound();

            //Creation of a new review object
            Review review = new Review()
            {
                BookId = book.BookId,
                Book = book,
                UserName = user.UserName,
                User = user
            };

            //Send the review object with already data inside
            return View(review);
        }

        /// <summary>
        /// The post method will save the new review made by an authorized
        /// user. Through the process data validation is made to ensure
        /// that we enter a new review with existant data.
        /// </summary>
        /// <param name="review">review object</param>
        /// <param name="rating">nullable integer</param>
        /// <returns>redirect to the view if the data given is invalid</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateReview([Bind(Include = "Bookid, UserName, Rating, Content")] Review review, 
            Nullable<int> rating)
        {

            //Should not have null values
            if (review == null || rating == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Grabbing data from the get
            User user = db.Users.Find(review.UserName);
            Book book = db.Books.Find(review.BookId);

            //Data validation
            if (user == null || book == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                //The rating can only be in a range of 0 to 5 inclusive
                if (rating >= 0 && rating < 6)
                {
                    //Set the review elements
                    review.Rating = rating;                
                    review.Book = book;
                    review.User = user;

                    //Decrement the rating for our database
                    DecrementOneRatingObj(review);


                    //Adding
                    book.Reviews.Add(review);
                    user.Reviews.Add(review);
                    db.Reviews.Add(review);
                    db.SaveChanges();

                    //Redirect to the main page
                    return RedirectToAction("Index");
                }
            }

            //Clear message
            ModelState.AddModelError("", "Error: the rating code can only be between 0 and 5");


            return View(review);
        }

        /// <summary>
        /// Will change the current rating of this object
        /// to a valid value inside our database
        /// </summary>
        /// <param name="t"> review object</param>
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
