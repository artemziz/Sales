using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales.Models.ViewModels
{
    public class Shop
    {
        public List<Books> Books { get; set; }
        public List<Books> Basket { get; set; }
    }
}
