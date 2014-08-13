using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Dtos;
using ServiceStack;
using System.Threading;
using System.Diagnostics;
using Common;

namespace Services
{
    /// <summary>
    /// Mock reference service implementation.
    /// </summary>
    public class MockReferenceService : IReferenceService
    {
        #region Member Vars

        private int _callCount;
        private Random _random = new Random(); 

        #endregion

        /// <summary>
        /// Gets symbology information based on the specified query parameters.
        /// </summary>
        /// <param name="query">Contains information that identifies the symbology information to return.</param>
        /// <returns>SymbologyInfo object for the specified query. If no such symbology information exists then returns null.</returns>
        public SymbologyInfo Get(SymbologyInfoQuery query)
        {
            _callCount++;

            this.GetType().DebugFormat("Request for {0} on process id {1}, thread id {2}",
                query.SyCode, Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId);

            Thread.Sleep(query.TimeToWait);

            return new SymbologyInfo
            {
                SyCode = query.SyCode,
                LoanXId = _callCount.ToString(),
                Cusip = _callCount.ToString(),
                Description = "Time: " + DateTime.Now.ToString("h:mm:ss tt")
            };
        }
    }
}
