using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class BankAccountTableEntity : TableEntity
    {
        [DataMember]
        public long AccountNumber { get; set; }

        [DataMember]
        public double AvailableAssets { get; set; }

        public BankAccountTableEntity() { }
        public BankAccountTableEntity(BankAccount account)
        {
            this.PartitionKey = "BankAccount";
            this.RowKey = account.AccountNumber.ToString();
            this.AccountNumber = account.AccountNumber;
            this.AvailableAssets = account.AvailableAssets;
        }
    }
}
