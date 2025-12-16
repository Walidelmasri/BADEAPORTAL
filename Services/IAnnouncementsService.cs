using System.Collections.Generic;
using System.Threading.Tasks;
using BADEAPORTAL.Models;

namespace BADEAPORTAL.Services
{
    public interface IAnnouncementsService
    {
        Task<(IReadOnlyList<Announcement> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
        Task<Announcement?> GetByIdAsync(int id);
        Task<int> CreateAsync(AnnouncementCreateDto dto);
    }
}
