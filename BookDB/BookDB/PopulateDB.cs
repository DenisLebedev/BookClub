using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Generic;

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
            //IEnumerable<Book> books = ListOfBooks(bookEl);        
            //Console.WriteLine(books);

            foreach(Author i in authors)
            {
                Console.WriteLine(i.FirstName + " " + i.LastName + "\n");
            }
            
        }

        public static IEnumerable<Author> ListOfAuthor(XElement bookEl)
        {
            IEnumerable<Author> list =
                 (from item in bookEl.Descendants("book")
                 select CreateAuthorObject(item));

            /*List<Author> test = list.GroupBy(item => item.AuthorId)
                .Select(grp => grp.First()).ToList();
                */
            return list;
        }

        public static IEnumerable<Book> ListOfBooks(XElement bookEl)
        {
            IEnumerable<Book> list =
                from item in bookEl.Descendants("book")
                select CreateBookObject(item);

            return list;
        }

        public static Author CreateAuthorObject(XElement item)
        {
            Author obj = new Author();
            string temp = item.Attribute("id")?.Value;
            int value;
            if (Int32.TryParse(temp, out value))
                obj.AuthorId = Int32.Parse(temp);
            //else if (temp == null)
            //    obj.AuthorId = null;

            obj.FirstName = item.Element("author").Attribute("firstName")?.Value;
            obj.LastName = item.Element("author").Attribute("lastName")?.Value;

            return obj;
        }


        /*public bool Equals(Author obj)
        {
            //if(obj.FirstName == )

            return false;
        }*/




        private static Book CreateBookObject(XElement item)
        {
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
            //app.AddDataInDB();
            PopulateDB.AddDataInDB();
            Console.Read();

        }

    }
}
