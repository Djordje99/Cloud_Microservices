﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class User : TableEntity
    {
        static private int instanceCounter = 0;

        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public List<int> PurchaseHistory { get; set; }
        [DataMember]
        public int BankAccountID { get; set; }

        public User(string indexNo)
        {
            PartitionKey = "User";
            RowKey = indexNo;
        }

        public User(UserDict user)
        {
            this.PartitionKey = "User";
            this.RowKey = user.Username;
            this.Username = user.Username;
            this.Email = user.Email;
            this.Password = user.Password;
            this.BankAccountID = user.BankAccountID;
            this.PurchaseHistory = user.PurchaseHistory;
            this.ID = instanceCounter++;
        }

        public User() { }
    }
}