using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Auth
{
    public sealed class SiifTokenResultDto
    {
        public string? AccessToken { get; set; } = default!;
        public string? IdToken { get; set; } = default!;
        public string? TokenType { get; set; } = "Bearer";
        public int ExpiresInSeconds { get; set; }
        public DateTimeOffset ExpiresAtUtc { get; set; }

        public string? BearerToken => IdToken;
    }
}
