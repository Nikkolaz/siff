using SSF.Interop.SIIFNacion.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Responses
{
    public class BaseRequestResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public List<Parameter> Parameters { get; set; }
    }
}
