using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Used for retrieving per DTO settings from configuration file.
    /// </summary>
    public class DtoConfigurationSettings
    {
        public int? AmqpResponseTimeOut { get; set; }
        public bool? LogQueries { get; set; }
    }
}
