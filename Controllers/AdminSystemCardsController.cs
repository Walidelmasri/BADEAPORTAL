using BADEAPORTAL.Data;
using BADEAPORTAL.Models;
using BADEAPORTAL.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BADEAPORTAL.Controllers
{
    [Authorize]
    public class AdminSystemCardsController : Controller
    {
        private readonly PortalDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AdminSystemCardsController(PortalDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var cards = await _db.PortalSystemCards
                .AsNoTracking()
                .OrderBy(x => x.CardId)
                .ToListAsync();

            return View(cards);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SystemCardFormVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SystemCardFormVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var card = new PortalSystemCard
            {
                SysId = vm.SysId,
                SysNameEn = vm.SysNameEn.Trim(),
                SysNameAr = vm.SysNameAr.Trim(),
                DescriptionEn = vm.DescriptionEn?.Trim(),
                DescriptionAr = vm.DescriptionAr?.Trim(),
                CategoryEn = vm.CategoryEn?.Trim(),
                CategoryAr = vm.CategoryAr?.Trim(),
                AppUrl = vm.AppUrl.Trim(),
                RoleGroup = vm.RoleGroup?.Trim(),
                IsPublic = vm.IsPublic,
                IsActive = vm.IsActive,
                CreatedAt = DateTime.Now
            };

            card.LogoPath = await SaveLogoAsync(vm.LogoFile) ?? vm.LogoPath;

            _db.PortalSystemCards.Add(card);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var card = await _db.PortalSystemCards.FindAsync(id);

            if (card == null)
                return NotFound();

            var vm = new SystemCardFormVm
            {
                CardId = card.CardId,
                SysId = card.SysId,
                SysNameEn = card.SysNameEn,
                SysNameAr = card.SysNameAr,
                DescriptionEn = card.DescriptionEn,
                DescriptionAr = card.DescriptionAr,
                CategoryEn = card.CategoryEn,
                CategoryAr = card.CategoryAr,
                AppUrl = card.AppUrl,
                LogoPath = card.LogoPath,
                RoleGroup = card.RoleGroup,
                IsPublic = card.IsPublic,
                IsActive = card.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SystemCardFormVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var card = await _db.PortalSystemCards.FindAsync(vm.CardId);

            if (card == null)
                return NotFound();

            card.SysId = vm.SysId;
            card.SysNameEn = vm.SysNameEn.Trim();
            card.SysNameAr = vm.SysNameAr.Trim();
            card.DescriptionEn = vm.DescriptionEn?.Trim();
            card.DescriptionAr = vm.DescriptionAr?.Trim();
            card.CategoryEn = vm.CategoryEn?.Trim();
            card.CategoryAr = vm.CategoryAr?.Trim();
            card.AppUrl = vm.AppUrl.Trim();
            card.RoleGroup = vm.RoleGroup?.Trim();
            card.IsPublic = vm.IsPublic;
            card.IsActive = vm.IsActive;
            card.UpdatedAt = DateTime.Now;

            var uploadedLogo = await SaveLogoAsync(vm.LogoFile);
            if (!string.IsNullOrWhiteSpace(uploadedLogo))
                card.LogoPath = uploadedLogo;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var card = await _db.PortalSystemCards
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CardId == id);

            if (card == null)
                return NotFound();

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var card = await _db.PortalSystemCards.FindAsync(id);

            if (card == null)
                return NotFound();

            card.IsActive = 0;
            card.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveLogoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid logo file type.");

            if (file.Length > 2 * 1024 * 1024)
                throw new InvalidOperationException("Logo file size must be 2MB or less.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "system-logos");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"logo_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            return $"/uploads/system-logos/{fileName}";
        }
    }
}