using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common
{
    public sealed class SiifRequestHeaderDto
    {
        public string? CodPci { get; set; } = default!;
        public string? LoginUsuarioSiif { get; set; } = default!;
        public string? Consecutivo { get; set; } = default!;
        public string? Hash { get; set; }
    }
}
