using System;
using System.Collections.Generic;

namespace Sales
{
    public partial class Books
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Books book)
            {
                if (this.Id == book.Id)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }



        }
    }
}
