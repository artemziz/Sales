using System;
using System.Collections.Generic;

namespace Sales
{
    public partial class Promocode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public bool IsUsed { get; set; }
    }
}
