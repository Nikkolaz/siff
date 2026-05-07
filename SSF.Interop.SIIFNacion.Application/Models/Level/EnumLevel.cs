using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Models.Level
{
    public enum EnumLevel
    {
        [Description("Error")]
        Error = 1,
        [Description("Warning")]
        Warning = 2,
        [Description("Informative")]
        Informative = 3,
        [Description("Debug")]
        Debug = 4
    }
}
