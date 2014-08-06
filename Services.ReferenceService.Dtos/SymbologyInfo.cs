using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos
{
    [Route("/Reference/Symbology/{SyCode}", verbs: "GET")]
    public class SymbologyInfoQuery
    {
        public string SyCode { get; set; }
    }

    public class SymbologyInfo
    {
        public string SyCode { get; set; }
        public string Cusip { get; set; }
        public string LoanXId { get; set; }
        public string Description { get; set; }
    }
}
