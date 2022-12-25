using Common.DTO;
using Common.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserStatefulService.TableHelper;

namespace DepartureStatefulService.Services
{
    public class DepartureService : IDepartureService
    {
        private IReliableStateManager _stateManager;
        private System.Threading.CancellationToken _cancellationToken;
        private Thread _tableThread;
        private long _dictCounter;
        private UserTableHelper _tableHepler;
        private IReliableDictionary<string, UserDict> departureDict;

        public DepartureService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
            this._cancellationToken.ThrowIfCancellationRequested();
            this._dictCounter = 0;
            this._tableHepler = new UserTableHelper("DepartureTable");
            //this._tableThread = new Thread(new ThreadStart(TableWriteThread));
        }

        public async Task<bool> CreateDeparture(Departure departure)
        {

            return true;
        }
    }
}
