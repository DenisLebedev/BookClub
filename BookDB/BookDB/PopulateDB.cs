using System;
using System.Collections.Generic;
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

        private XElement bookEl;
        private XElement ratingEl;

        /// <summary>
        /// The default constructor will load our two
        /// xml file needed to create fake data.
        /// </summary>
        public PopulateDB()
        {

            bookEl = XElement.Load("../../books.xml");
            ratingEl = XElement.Load("../../ratings.xml");

        }


        public void AddDataInDB()
        {

            IEnumerable<Book> books = ListOfBooks();
            Console.WriteLine(books);
            
        }

        public IEnumerable<Author> ListOfAuthor()
        {
            IEnumerable<Author> list =
                 from item in bookEl.Descendants("book")
                 select CreateAuthorObject(item);

            return list;
        }

        public Author CreateAuthorObject(XElement item)
        {
            Console.WriteLine("Creating author");

            Author obj = new Author();
            obj.FirstName = item.Element("author").Attribute("firstName")?.Value;
            obj.LastName = item.Element("author").Attribute("lastName")?.Value;

            return obj;
        }

        public IEnumerable<Book> ListOfBooks()
        {
            IEnumerable<Book> list =
                from item in bookEl.Descendants("book")
                select CreateBookObject(item);

            return list;
        }

        private Book CreateBookObject(XElement item)
        {
            Console.WriteLine("Creating book");

            Book obj = new Book();
            obj.Title = item.Element("title")?.Value;
            obj.Description = item.Element("description")?.Value;
            obj.BookId = Int32.Parse(item.Attribute("id").Value);
            return obj;
        }

        static void Main(string[] args)
        {

            //books.xml  ratings.xml
            
            PopulateDB app = new PopulateDB();
            app.AddDataInDB();

            Console.Read();

        }


    }
}
