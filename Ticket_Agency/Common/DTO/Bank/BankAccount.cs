using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class BankAccount
    {
        public long ID { get; set; }
        [Required]
        [StringLength(16, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 16)]
        public string AccountNumber { get; set; }
        [Required]
        public double AvailableAssets { get; set; }

        public BankAccount() { }


    }
}
