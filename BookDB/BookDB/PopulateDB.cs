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
        /// The default constructor will load our two
        /// xml file needed to create fake data.
        /// </summary>
        public PopulateDB()
        {

        }


        public static void AddDataInDB()
        {
            XElement bookEl = XElement.Load("../../books.xml");
            XElement ratingEl = XElement.Load("../../ratings.xml"); 
           
            IEnumerable<Author> authors = ListOfAuthor(bookEl);
            IEnumerable<Book> books = ListOfBook(bookEl, authors);
            IEnumerable<User> users = ListOfUser(ratingEl, books);

            AddAllDataInDB(authors, users);
           

            Console.WriteLine("Done");
        }

        private static void AddAllDataInDB (IEnumerable<Author> authors, IEnumerable<User> users)
        {
            using(BookClubDB db = new BookClubDB())
            {
               
                db.Authors.AddRange(authors);
                db.Users.AddRange(users);

                db.SaveChanges();

            }
        } 

        private static IEnumerable<Author> ListOfAuthor(XElement bookEl)
        {
            IEnumerable<Author> list =
                 (from item in bookEl.Descendants("book")
                 select CreateAuthorObject(item)).
                 GroupBy(x => x.FirstName + x.LastName).Select(x => x.First()).Distinct();

            return list.ToList();
        }

        private static Author CreateAuthorObject(XElement item)
        {
            Author obj = new Author();
            string temp = item.Attribute("id")?.Value;
            int value;
            if (Int32.TryParse(temp, out value))
                obj.AuthorId = value;

            obj.FirstName = GetValueOrDefault(item.Element("author").Attribute("firstName")?.Value);
            obj.LastName = item.Element("author").Attribute("lastName")?.Value;
            return obj;
        }

        private static IEnumerable<Book> ListOfBook(XElement bookEl, IEnumerable<Author> authors)
        {
            IEnumerable<Book> list =
                from item in bookEl.Descendants("book")
                select CreateBookObject(item, authors);

            return list.ToList();
        }

        private static Book CreateBookObject(XElement item, IEnumerable<Author> authors)
        {

            Book obj = new Book();
            string temp = item.Attribute("id")?.Value;
            int value;
            if (Int32.TryParse(temp, out value))
            {
                obj.BookId = value;
                obj.Title = item.Element("title")?.Value;
                obj.Description = item.Element("description")?.Value;
                obj.Views = 0;

                authors.Where(
                    x => x.FirstName == GetValueOrDefault(item.Element("author").Attribute("firstName")?.Value) &&
                    x.LastName == item.Element("author").Attribute("lastName")?.Value).
                First()?.Books.Add(obj);

            }

            return obj;
        }

        private static IEnumerable<User> ListOfUser(XElement ratingEl, IEnumerable<Book> books)
        {
            return
                (from item in ratingEl.Descendants("user")
                select CreateUserObject(item, books)).ToList();

        }

        private static User CreateUserObject(XElement item, IEnumerable<Book> books)
        {
            User obj = new User();

            //avoid repetition + 3 search
            string userName = GetValueOrDefault(item.Attribute("userId").Value, "");
            obj.UserName = userName;
            obj.FirstName = userName;
            obj.LastName = GetValueOrDefault(item.Attribute("lastName")?.Value, "Reader");
            obj.Password = userName;
            obj.Country = "CAN";
            obj.Email = null;

            if (obj.UserName == item.Attribute("userId")?.Value)
                CreateReviewPerUser(item.Descendants().Where(x => x.Attribute("rating") != null),
                obj, books);
            return obj;
        }


        private static void CreateReviewPerUser(IEnumerable<XElement> item, User user,
           IEnumerable<Book> books)
        {
            int bookId;
            int ratingNum;
            List<Review> list = new List<Review>();

            foreach (XElement review in item)
            {

                if (Int32.TryParse(review.Attribute("bookId")?.Value, out bookId) &&
                    Int32.TryParse(review.Attribute("rating")?.Value, out ratingNum))
                {
                    Review obj = new Review();

                    Book book = books.Where(x => x.BookId == bookId).First();
                    obj.Book = book;
                    obj.BookId = book.BookId;
                    book.Views += 1;
                    obj.Rating = ratingNum;
                    obj.UserName = user.UserName;
                    obj.User = user;
                    list.Add(obj);
                    user.Reviews.Add(obj);
                }
            }
        }

       
        private static string GetValueOrDefault(string str, string def = "")
        {
            return str == null ? def : str;
        }


        static void Main(string[] args)
        {
            
            PopulateDB.AddDataInDB();
            Console.Read();

        }

    }
}
