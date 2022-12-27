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
        public long ID { get; set; }

        [DataMember]
        public string OwnerUsername { get; set; }

        [DataMember]
        public string AccountNumber { get; set; }

        [DataMember]
        public double AvailableAssets { get; set; }

        public BankAccountTableEntity() { }
        public BankAccountTableEntity(BankAccount account)
        {
            this.PartitionKey = "BankAccount";
            this.RowKey = account.OwnerUsername;
            var id =  DateTime.Now.ToString("yyyyMMddHHmmssffff");
            this.ID = Int64.Parse(id);
            this.AccountNumber = account.AccountNumber;
            this.AvailableAssets = account.AvailableAssets;
            this.OwnerUsername = account.OwnerUsername;
        }
    }
}
