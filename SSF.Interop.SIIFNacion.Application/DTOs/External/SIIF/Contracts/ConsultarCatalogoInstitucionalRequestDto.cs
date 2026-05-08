using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts
{
    public sealed class ConsultarCatalogoInstitucionalRequestDto
    {
        public string? CodAmbitoField { get; set; } = default!;
        public string? ListaCodPCIField { get; set; } = default!;
    }
}
