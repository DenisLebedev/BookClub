﻿using System;
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
            IEnumerable<User> users = ListOfUser(ratingEl);
            IEnumerable<Review> reviews = ListOfReview(ratingEl, books, users);

            AddAllDataInDB(authors, users);


            /*foreach (Author i in authors)
            {
                Console.Write(i.FirstName);
                foreach (Book j in i.Books)
                    Console.Write("  " + j.Title);
                Console.WriteLine();
            }*/

            /*foreach(User u in users)
            {
                foreach (Review r in u.Reviews)
                    Console.WriteLine(u.FirstName + " " + r.Rating);
            }*/
            


            Console.WriteLine("Done");
        }

        private static void AddAllDataInDB (IEnumerable<Author> authors, IEnumerable<User> users)
        {
            using(BookClubDB db = new BookClubDB())
            {
                /*foreach(Author item in authors)
                 {
                     try
                     {
                         db.Authors.Add(item);
                         Console.WriteLine(item.AuthorId + " " + item.FirstName + " " + item.LastName);
                         db.SaveChanges();
                     }
                     catch (DbEntityValidationException dbEx)
                     {
                         Console.WriteLine("ErrorLine: -" + item.FirstName + "-" );
                         foreach (var validationErrors in dbEx.EntityValidationErrors)
                         {
                             foreach (var validationError in validationErrors.ValidationErrors)
                             {
                                 System.Console.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                             }
                         }
                     }
                 }*/

                //db.Authors.AddRange(authors);

                foreach (User item in users)
                 {
                    try
                    {
                        db.Users.Add(item);
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        Console.WriteLine("ErrorLine: -" + item.FirstName + "-");
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                System.Console.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                            }
                        }
                    }catch(Exception e)
                    {
                        Console.WriteLine("General Exception: " + e.InnerException);
                    }
                 }

                
                //db.Users.AddRange(users);
                /*try
                {

                    db.SaveChanges();
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message + e.InnerException + " <<<");
                }*/
            }
        } 

        private static IEnumerable<Author> ListOfAuthor(XElement bookEl)
        {
            IEnumerable<Author> list =
                 (from item in bookEl.Descendants("book")
                 select CreateAuthorObject(item)).
                 GroupBy(x => x.FirstName + x.LastName).Select(x => x.First()).Distinct();

            //list = list.GroupBy(x => x.FirstName + x.LastName).Select(x => x.First()).Distinct();
            /*List<Author> test = list.GroupBy(item => item.AuthorId)
                .Select(grp => grp.First()).ToList();
                */
            return list.ToList();
        }

        private static Author CreateAuthorObject(XElement item)
        {
            Author obj = new Author();
            string temp = item.Attribute("id")?.Value;
            int value;
            if (Int32.TryParse(temp, out value))
                obj.AuthorId = value;
            /*String test = item.Element("author").Attribute("firstName")?.Value;
            if (test == null)
                test = "";*/
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

                /*String test = item.Element("author").Attribute("firstName")?.Value;
                if (test == null)
                    test = "";*/
                authors.Where(
                    x => x.FirstName == GetValueOrDefault(item.Element("author").Attribute("firstName")?.Value) &&
                    x.LastName == item.Element("author").Attribute("lastName")?.Value).
                First()?.Books.Add(obj);

            }

            return obj;
        }

        private static IEnumerable<User> ListOfUser(XElement ratingEl)
        {
            IEnumerable<User> list =
                from item in ratingEl.Descendants("user")
                select CreateUserObject(item);

            return list.ToList();
        }

        private static IEnumerable<Review> ListOfReview(XElement ratingEl, 
            IEnumerable<Book> books, IEnumerable<User> users)
        {
            List<Review> list = new List<Review>();

            //for each user create a list of reviews
            foreach (XElement item in ratingEl.Descendants("user"))
            {
                IEnumerable<Review> temp;
                temp = CreateReviewPerUser(item.Descendants().Where(x => x.Attribute("rating") != null),
                        users.Where(x => x.UserName == item.Attribute("userId")?.Value).First()
                        , books);

                if (temp.Count() != 0)
                    foreach (Review i in temp)
                        list.Add(i);
            }

            return list.ToList();
        }

        private static IEnumerable<Review> CreateReviewPerUser(IEnumerable <XElement> item, User user,
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

                    //Console.WriteLine(books.Where(x => x.BookId == bookId).Count());

                    Book book = books.Where(x => x.BookId == bookId).First();
                    obj.BookId = book.BookId;
                    book.Views += 1;
                    obj.Rating = ratingNum;
                    obj.UserName = user.UserName;
                    list.Add(obj);
                    user.Reviews.Add(obj);
                }
            }
            return list;
        }

        private static User CreateUserObject(XElement item)
        {
            User obj = new User();

            //avoid repetition + 3 search
            string userName = GetValueOrDefault(item.Attribute("userId").Value, "");
            obj.UserName = userName;
            obj.FirstName = userName;
            obj.LastName = GetValueOrDefault(item.Attribute("lastName").Value, "Reader");
            obj.Password = userName;
            obj.Country = "CAN";
            obj.Email = null;
            return obj;
        }


        private static string GetValueOrDefault(string str, string def = "")
        {
            return str == null ? def : str;
        }


        static void Main(string[] args)
        {

            //books.xml  ratings.xml
            
            PopulateDB app = new PopulateDB();
            //app.AddDataInDB();
            PopulateDB.AddDataInDB();
            Console.Read();

        }

    }
}
