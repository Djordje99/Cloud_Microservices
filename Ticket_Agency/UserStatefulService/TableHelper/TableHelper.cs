using Common.DTO;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserStatefulService.TableHelper
{
    public class UserTableHelper
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public UserTableHelper()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("UserTable");
            _table.CreateIfNotExists();
        }

        public IQueryable<User> RetrieveAllUsers()
        {
            var results = from g in _table.CreateQuery<User>()
                          where g.PartitionKey == "User"
                          select g;
            return results;
        }

        public async Task<User> FindUser(string username)
        {
            var results = from g in _table.CreateQuery<User>()
                          where g.PartitionKey == "User"
                          select g;

            foreach (User user in results)
            {
                if (user.Username.Equals(username))
                {
                    return user;
                }
            }

            return null;
        }

        public async Task AddUser(User user)
        {
            TableOperation insertOperation = TableOperation.InsertOrReplace(user);
            _table.Execute(insertOperation);
        }
    }
}
