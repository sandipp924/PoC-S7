using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos
{
    /// <summary>
    /// Interface that specifies the api contract of reference service.
    /// </summary>
    public interface IReferenceService : IService
    {
        SymbologyInfo Get(SymbologyInfoQuery query);
    }
}
