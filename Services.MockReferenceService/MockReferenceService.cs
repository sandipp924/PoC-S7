using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Dtos;
using ServiceStack;
using System.Threading;

namespace Services
{
    public class MockReferenceService : IReferenceService
    {
        private int _callCount;
        private Random _random = new Random();

        public MockReferenceService()
        {
        }

        public SymbologyInfo Any(SymbologyInfoQuery query)
        {
            Thread.Sleep(_random.Next(4000, 8000));

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
