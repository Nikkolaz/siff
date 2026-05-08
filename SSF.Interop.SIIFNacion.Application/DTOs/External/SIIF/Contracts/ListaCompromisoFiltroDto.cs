using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts
{
    public sealed class ListaCompromisoFiltroDto
    {
        public string? PCI { get; set; } = default!;
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public string? TipoGasto { get; set; } = default!;
        public string? Rango { get; set; } = default!;
        public string? Vigencia { get; set; } = default!;
        public int Page { get; set; } = 0;
        public int Size { get; set; } = 0;
     
    }

     public sealed class ListaCompromisoPaginadaRequestDto
    {
        public PaginationDto Pagination { get; set; } = new();
       // public ListaCompromisoFiltroDto LstCompEnt { get; set; } = new();
        public ListaCompromisoFiltroDto ListaCompromiso { get; set; } = default!;
    }
    
}
