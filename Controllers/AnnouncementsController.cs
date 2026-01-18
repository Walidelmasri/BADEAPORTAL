using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BADEAPORTAL.Models;
using BADEAPORTAL.Services;
using BADEAPORTAL.Models.Announcements;

namespace BADEAPORTAL.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementsService _announcements;
        private readonly IMemoPdfService _memoPdf;

        public AnnouncementsController(
            IAnnouncementsService announcements,
            IMemoPdfService memoPdf)
        {
            _announcements = announcements;
            _memoPdf = memoPdf;
        }

        // GET: /Announcements?page=1
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int PageSize = 10;

            var (items, totalCount) = await _announcements.GetPagedAsync(page, PageSize);

            var vm = new AnnouncementsIndexVm
            {
                Page = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize),
                Items = items.Select(a => new AnnouncementSummaryVm
                {
                    Id = a.Id,
                    Title = a.Title,
                    Excerpt = BuildExcerpt(a.BodyHtml, 180),
                    IsMemo = a.IsMemo,
                    CreatedAtUtc = a.CreatedAtUtc,
                    CreatedByName = a.CreatedByName
                }).ToList()
            };

            return View(vm);
        }

        // GET: /Announcements/Create
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new AnnouncementCreateVm
            {
                IsMemo = true,          // default memo
                NotifyInApp = true,
                NotifyEmail = true,
                FromKind = "USER"
            };
            return View(vm);
        }


        // POST: /Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = new AnnouncementCreateDto
            {
                Title = vm.Title,
                BodyHtml = vm.BodyHtml,
                IsMemo = vm.IsMemo,
                MemoTo = vm.MemoTo,
                MemoThrough = vm.MemoThrough,
                MemoFrom = vm.MemoFrom,
                MemoSubject = vm.MemoSubject,
                MemoClassification = vm.MemoClassification,
                FromKind = vm.FromKind,
                FromDeptCode = vm.FromDeptCode,
                NotifyInApp = vm.NotifyInApp,
                NotifyEmail = vm.NotifyEmail
            };

            var id = await _announcements.CreateAsync(dto);

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Announcements/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var entity = await _announcements.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var vm = new AnnouncementDetailsVm
            {
                Id = entity.Id,
                Title = entity.Title,
                BodyHtml = entity.BodyHtml,
                IsMemo = entity.IsMemo,
                MemoTo = entity.MemoTo,
                MemoThrough = entity.MemoThrough,
                MemoFrom = entity.MemoFrom,
                MemoSubject = entity.MemoSubject,
                MemoClassification = entity.MemoClassification,
                CreatedAtUtc = entity.CreatedAtUtc,
                CreatedByName = entity.CreatedByName
            };

            return View(vm);
        }

        // GET: /Announcements/DownloadMemo/5
        [HttpGet]
        public async Task<IActionResult> DownloadMemo(int id)
        {
            var entity = await _announcements.GetByIdAsync(id);
            if (entity == null || !entity.IsMemo)
            {
                return NotFound();
            }

            var request = new MemoPdfRequest
            {
                To = entity.MemoTo ?? string.Empty,
                Through = entity.MemoThrough,
                From = entity.MemoFrom ?? string.Empty,
                Subject = entity.MemoSubject ?? entity.Title,
                Classification = entity.MemoClassification ?? string.Empty,
                BodyHtml = entity.BodyHtml,
                CreatedAtUtc = entity.CreatedAtUtc,
                CreatedByName = entity.CreatedByName
            };

            var pdfBytes = _memoPdf.GenerateMemoPdf(request);

            var fileName = $"Memo-{entity.Id}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        private static string BuildExcerpt(string html, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Strip tags
            var text = Regex.Replace(html, "<.*?>", string.Empty);
            text = System.Net.WebUtility.HtmlDecode(text).Trim();

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "…";
        }
        // GET: /Announcements/Modal/5
        [HttpGet]
        public async Task<IActionResult> Modal(int id)
        {
            var entity = await _announcements.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var vm = new AnnouncementDetailsVm
            {
                Id = entity.Id,
                Title = entity.Title,
                BodyHtml = entity.BodyHtml,
                IsMemo = entity.IsMemo,
                MemoTo = entity.MemoTo,
                MemoThrough = entity.MemoThrough,
                MemoFrom = entity.MemoFrom,
                MemoSubject = entity.MemoSubject,
                MemoClassification = entity.MemoClassification,
                CreatedAtUtc = entity.CreatedAtUtc,
                CreatedByName = entity.CreatedByName
            };

            return PartialView("_AnnouncementDetailsModalBody", vm);
        }


    }
}
