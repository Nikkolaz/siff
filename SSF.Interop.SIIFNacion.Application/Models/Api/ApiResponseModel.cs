using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Models.Api
{
    public class ApiResponseModel
    {
        public int ApiId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }
}
