using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BADEAPORTAL.Data;
using BADEAPORTAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BADEAPORTAL.Services
{
    public sealed class AnnouncementsService : IAnnouncementsService
    {
        private readonly PortalDbContext _db;
        private readonly IUserProfileService _userProfile;

        public AnnouncementsService(PortalDbContext db, IUserProfileService userProfile)
        {
            _db = db;
            _userProfile = userProfile;
        }

        public async Task<(IReadOnlyList<Announcement> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;

            var query = _db.Announcements
                .OrderByDescending(a => a.CreatedAtUtc);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Announcement?> GetByIdAsync(int id)
        {
            return await _db.Announcements
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<int> CreateAsync(AnnouncementCreateDto dto)
        {
            var user = _userProfile.GetCurrentUser();

            var createdByName =
                user.FullName ??
                user.DisplayName ??
                user.EmailOrUpn ??
                "Unknown";

            var createdByUpn = user.EmailOrUpn ?? "unknown@local";

            var entity = new Announcement
            {
                Title = dto.Title,
                BodyHtml = dto.BodyHtml,
                IsMemo = dto.IsMemo,

                MemoTo = dto.MemoTo,
                MemoThrough = dto.MemoThrough,
                MemoFrom = dto.MemoFrom,
                MemoSubject = dto.MemoSubject,
                MemoClassification = dto.MemoClassification,

                CreatedAtUtc = DateTime.UtcNow,
                CreatedByName = createdByName,
                CreatedByUpn = createdByUpn
            };

            _db.Announcements.Add(entity);
            await _db.SaveChangesAsync();

            return entity.Id;
        }
    }
}
