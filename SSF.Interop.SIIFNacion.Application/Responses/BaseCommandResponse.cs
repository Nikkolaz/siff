using SSF.Interop.SIIFNacion.Application.DTOs.Common;

namespace SSF.Interop.SIIFNacion.Application.Responses
{
    public class BaseCommandResponse
    {
        public int Id { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public List<Parameter> Parameters { get; set; }
    }
}