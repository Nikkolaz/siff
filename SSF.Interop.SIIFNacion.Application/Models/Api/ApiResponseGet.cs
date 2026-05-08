namespace SSF.Interop.SIIFNacion.Application.Models
{
    public class ApiResponseGet
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public List<ParameterResponseGet> Parameters { get; set; }
    }
}