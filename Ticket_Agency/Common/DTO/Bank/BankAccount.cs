using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class BankAccount
    {
        [Required]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "The field must contain exactly 16 digits.")]
        [DataMember]
        public long AccountNumber { get; set; }

        [Required]
        [DataMember]
        public double AvailableAssets { get; set; }

        public BankAccount() { }

        public BankAccount(BankAccountTableEntity account)
        {
            this.AccountNumber = account.AccountNumber;
            this.AvailableAssets = account.AvailableAssets;
        }
    }
}
