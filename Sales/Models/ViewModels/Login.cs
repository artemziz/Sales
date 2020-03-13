using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sales.Models.ViewModels
{
    public class Login
    {
        [Required(ErrorMessage = "Не введен промокод")]
        public string Code { get; set; }
    }
}
