using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Used for retrieving per service settings from configuration file.
    /// </summary>
    public class ServiceConfigurationSettings
    {
        public bool? UseMock { get; set; }
    }
}
