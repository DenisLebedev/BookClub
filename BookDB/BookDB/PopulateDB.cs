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
            bookEl = XElement.Load("books.xml");
            ratingEl = XElement.Load("ratings.xml");

        }


        public void AddDataInDB()
        {
            var test = from item in bookEl.Descendants("book")
                       select new {};
        }



        static void Main(string[] args)
        {

            //books.xml  ratings.xml

            PopulateDB app = new PopulateDB();

        }


    }
}
