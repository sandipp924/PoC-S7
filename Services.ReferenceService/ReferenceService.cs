using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Dtos;
using ServiceStack;

namespace Services
{
    public class ReferenceService : IReferenceService
    {
        private int _callCount;

        public ReferenceService()
        {
        }

        public SymbologyInfo Any(SymbologyInfoQuery query)
        {
            _callCount++;
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
