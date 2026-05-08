namespace SSF.Interop.SIIFNacion.Application.Common.Interfaces
{
    public interface IFileExportService<T>
    {
        byte[] Export(IEnumerable<T> data);
    }
}
