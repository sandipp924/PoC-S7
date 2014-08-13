using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Dtos;
using ServiceStack;
using System.Threading;
using Common;
using System.Diagnostics;

namespace Services
{
    /// <summary>
    /// Reference service implementation.
    /// </summary>
    public class ReferenceService : IReferenceService
    {
        #region Member Vars

        private int _callCount; 

        #endregion

        #region GetSymbologyInfo

        /// <summary>
        /// Gets symbology information based on the specified query parameters.
        /// </summary>
        /// <param name="query">Contains information that identifies the symbology information to return.</param>
        /// <returns>SymbologyInfo object for the specified query. If no such symbology information exists then returns null.</returns>
        public SymbologyInfo Get(SymbologyInfoQuery query)
        {
            this.GetType().DebugFormat("Request for {0} on process id {1}, thread id {2}",
                query.SyCode, Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId);

            Thread.Sleep(query.TimeToWait);
            
            _callCount++;
            return new SymbologyInfo
            {
                SyCode = query.SyCode,
                LoanXId = _callCount.ToString(),
                Cusip = _callCount.ToString(),
                Description = "Time: " + DateTime.Now.ToString("h:mm:ss tt")
            };
        } 

        #endregion
    }
}
