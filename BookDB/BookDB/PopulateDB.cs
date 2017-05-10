using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BookDB
{
    /// <summary>
    /// PopulateDB will populate our database with inital fake data
    /// to ensure that our applicationw work perfectly.
    /// </summary>
    class PopulateDB
    {


        /// <summary>
        /// Default constructor
        /// </summary>
        public PopulateDB()
        {

        }


        /// <summary>
        /// The method will call all the methods needed to add every given elements
        /// from the files into our database.
        /// </summary>
        public static void AddDataInDB()
        {
            //Calling the external files
            XElement bookEl = XElement.Load("../../books.xml");
            XElement ratingEl = XElement.Load("../../ratings.xml"); 
           
            //Load all the data into IEnumerable boject
            IEnumerable<Author> authors = ListOfAuthor(bookEl);
            IEnumerable<Book> books = ListOfBook(bookEl, authors);
            IEnumerable<User> users = ListOfUser(ratingEl, books);

            //Add everything in database
            AddAllDataInDB(authors, users);

        }

        /// <summary>
        /// Add every single data to the database.
        /// </summary>
        /// <param name="authors">list of all authors which contain all the data</param>
        /// <param name="users"> list of all users which contain all the data</param>
        private static void AddAllDataInDB (IEnumerable<Author> authors, IEnumerable<User> users)
        {
            using(BookClubDB db = new BookClubDB())
            {
               
                db.Authors.AddRange(authors);
                db.Users.AddRange(users);

                db.SaveChanges();

            }
        } 

        /// <summary>
        /// Create a list of authors object fully loaded
        /// of data and unique.
        /// </summary>
        /// <param name="bookEl">a xml file of authors</param>
        /// <returns>a list of unique authors</returns>
        private static IEnumerable<Author> ListOfAuthor(XElement bookEl)
        {

            //The group by ensure that the data will be unique inside the list
            IEnumerable<Author> list =
                 (from item in bookEl.Descendants("book")
                 select CreateAuthorObject(item)).
                 GroupBy(x => x.FirstName + x.LastName).Select(x => x.First()).Distinct();

            return list.ToList();
        }


        /// <summary>
        /// Creates a single valid author object with
        /// the right XElement of books given.
        /// </summary>
        /// <param name="item">XElement of books</param>
        /// <returns>a valid author object</returns>
        private static Author CreateAuthorObject(XElement item)
        {
            Author obj = null;
            string temp = item.Attribute("id")?.Value;
            int value;

            //Tryparse to see if it is a valid string
            if (Int32.TryParse(temp, out value))
            {
                obj = new Author();

                obj.AuthorId = value;

                //Grab data from the XElement
                obj.FirstName = GetValueOrDefault(item.Element("author").Attribute("firstName")?.Value);
                obj.LastName = item.Element("author").Attribute("lastName")?.Value;
            }

            return obj;
        }

        /// <summary>
        /// Grab the data from the file and creates
        /// new Book object for the database.
        /// </summary>
        /// <param name="bookEl">XElement of boosk</param>
        /// <param name="authors">List of valid authors</param>
        /// <returns>List of valid books</returns>
        private static IEnumerable<Book> ListOfBook(XElement bookEl, IEnumerable<Author> authors)
        {
            IEnumerable<Book> list =
                from item in bookEl.Descendants("book")
                select CreateBookObject(item, authors);

            return list.ToList();
        }


        /// <summary>
        /// Creates a single valid Book object with the given
        /// data from the file.
        /// </summary>
        /// <param name="item">XElement of books</param>
        /// <param name="authors">List of valid authors</param>
        /// <returns></returns>
        private static Book CreateBookObject(XElement item, IEnumerable<Author> authors)
        {

            Book obj = null;
            string temp = item.Attribute("id")?.Value;
            int value;

            //Ensure the data is correct
            if (Int32.TryParse(temp, out value))
            {
                obj = new Book();
                obj.BookId = value;
                obj.Title = item.Element("title")?.Value;
                obj.Description = item.Element("description")?.Value;

                //Default view
                obj.Views = 0;

                //Search for a valid author through the given list
                authors.Where(
                    x => x.FirstName == GetValueOrDefault(item.Element("author").Attribute("firstName")?.Value) &&
                    x.LastName == item.Element("author").Attribute("lastName")?.Value).
                First()?.Books.Add(obj);

            }

            return obj;
        }

        /// <summary>
        /// Creates a list of users with the necessary
        /// data and relate their reviews at the 
        /// same time using the given data file.
        /// </summary>
        /// <param name="ratingEl">XElement of rating</param>
        /// <param name="books">List of books</param>
        /// <returns></returns>
        private static IEnumerable<User> ListOfUser(XElement ratingEl, IEnumerable<Book> books)
        {
            //List of users
            return
                (from item in ratingEl.Descendants("user")
                select CreateUserObject(item, books)).ToList();

        }

        /// <summary>
        /// Creates a single User object with valid
        /// information inside. Also, links the rating.
        /// </summary>
        /// <param name="item">XElement of ratings</param>
        /// <param name="books">List of books</param>
        /// <returns></returns>
        private static User CreateUserObject(XElement item, IEnumerable<Book> books)
        {
            User obj = new User();

            //avoid repetition of 3 searches
            string userName = GetValueOrDefault(item.Attribute("userId").Value, "");
            obj.UserName = userName;
            obj.FirstName = userName;
            obj.LastName = GetValueOrDefault(item.Attribute("lastName")?.Value, "Reader");
            obj.Password = userName;
            //Default country
            obj.Country = "CAN";
            //Default email
            obj.Email = null;

            //If the user has a rating we will add one
            if (obj.UserName == item.Attribute("userId")?.Value)
                CreateReviewPerUser(item.Descendants().Where(x => x.Attribute("rating") != null),
                obj, books);
            return obj;
        }


        /// <summary>
        /// Create X number of reviews for the given user.
        /// A user may have multiple reviews.
        /// </summary>
        /// <param name="item">IEnumerable that represent all the reviews made by the given user</param>
        /// <param name="user">Reviews for the given user</param>
        /// <param name="books">List of books</param>
        private static void CreateReviewPerUser(IEnumerable<XElement> item, User user,
           IEnumerable<Book> books)
        {
            int bookId;
            int ratingNum;
            List<Review> list = new List<Review>();

            //For each review in the XElement
            foreach (XElement review in item)
            {
                //Parse the two ids
                if (Int32.TryParse(review.Attribute("bookId")?.Value, out bookId) &&
                    Int32.TryParse(review.Attribute("rating")?.Value, out ratingNum))
                {
                    
                    Review obj = new Review();

                    //Grab the Book object for that review
                    Book book = books.Where(x => x.BookId == bookId).First();

                    //Assign the necessary data to the review object
                    obj.Book = book;
                    obj.BookId = book.BookId;

                    //Increment the view for that book
                    book.Views += 1;
                    
                    //Assign the rating
                    obj.Rating = ratingNum;
                    
                    //Assign the given user
                    obj.UserName = user.UserName;
                    obj.User = user;

                    //Add the review to the list
                    list.Add(obj);
                    user.Reviews.Add(obj);
                }
            }
        }

        /// <summary>
        /// Return the value inside the given string
        /// if it is not null else it returning
        /// a default value that can be changed by the user.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="def">optional value that will be used if the string is null</param>
        /// <returns></returns>
        private static string GetValueOrDefault(string str, string def = "")
        {
            return str == null ? def : str;
        }


        /// <summary>
        /// Run the single method available
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            
            PopulateDB.AddDataInDB();

        }

    }
}
