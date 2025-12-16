using BADEAPORTAL.Models;

namespace BADEAPORTAL.Services
{
    public interface IMemoPdfService
    {
        byte[] GenerateMemoPdf(MemoPdfRequest request);
    }
}
