using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Infrastructure.Options
{

    public sealed class SiifOptions
    {
        public const string SectionName = "Siif";

        public string BaseUrl { get; set; } = default!;
        public string AuthenticationPath { get; set; } = "/siif/api/seg/authentication";
        public string User { get; set; } = default!;
        public string Password { get; set; } = default!;
        public int TokenExpirationSeconds { get; set; } = 60;
        public int TokenRenewalSafetyWindowSeconds { get; set; } = 10;
        public int TimeoutSeconds { get; set; } = 30;
    }

}
