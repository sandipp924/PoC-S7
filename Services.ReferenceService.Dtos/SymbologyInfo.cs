using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos
{
    /// <summary>
    /// Query DTO class used by reference service.
    /// </summary>
    [Route("/Reference/Symbology/{SyCode}", verbs: "GET")]
    public class SymbologyInfoQuery
    {
        public string SyCode { get; set; }

        // This is for testing parallel execution as well as time-out.
        public int TimeToWait { get; set; }
    }

    /// <summary>
    /// Response DTO class that contains symbology information in response to a symbology query.
    /// </summary>
    /// <seealso cref="SymbologyInfoQuery"/>
    public class SymbologyInfo
    {
        public string SyCode { get; set; }
        public string Cusip { get; set; }
        public string LoanXId { get; set; }
        public string Description { get; set; }
    }
}
