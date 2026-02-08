using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers
{
    [Authorize]
    public class WorkflowController : Controller
    {
        private readonly ERPContext _context;

        public WorkflowController(ERPContext context)
        {
            _context = context;
        }
        // مدیریت بخش‌ها (WorkflowSection)
        public async Task<IActionResult> Sections()
        {
            var sections = await _context.WorkflowSections.ToListAsync();
            return View(sections);
        }

        public IActionResult CreateSection()
        {
            return View();
        }

        [HttpPost]
         
        public async Task<IActionResult> CreateSection(WorkflowSection section)
        {
            if (ModelState.IsValid)
            {
                section.CreatedDate = DateTime.Now;
                _context.WorkflowSections.Add(section);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Sections));
            }
            return View(section);
        }

        public async Task<IActionResult> EditSection(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.WorkflowSections.FindAsync(id);
            if (section == null)
            {
                return NotFound();
            }
            return View(section);
        }

        [HttpPost]
         
        public async Task<IActionResult> EditSection(int id, WorkflowSection section)
        {
            if (id != section.WorkflowSectionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(section);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SectionExists(section.WorkflowSectionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Sections));
            }
            return View(section);
        }

        public async Task<IActionResult> DeleteSection(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.WorkflowSections
                .FirstOrDefaultAsync(m => m.WorkflowSectionID == id);
            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        [HttpPost, ActionName("DeleteSection")]
         
        public async Task<IActionResult> DeleteSectionConfirmed(int id)
        {
            var section = await _context.WorkflowSections.FindAsync(id);
            _context.WorkflowSections.Remove(section);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Sections));
        }

        // مدیریت دسترسی‌ها (WorkflowAccess)
        public async Task<IActionResult> Access()
        {
            ViewBag.Users = await _context.Users
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToListAsync();
            ViewBag.Sections = await _context.WorkflowSections
                .Select(s => new { s.SectionCode, s.SectionName })
                .ToListAsync();
            var accesses = await _context.WorkflowAccesses.ToListAsync();
            return View(accesses);
        }

        public IActionResult CreateAccess()
        {
            ViewBag.Users = _context.Users
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToList();
            ViewBag.Sections = _context.WorkflowSections
                .Select(s => new { s.SectionCode, s.SectionName })
                .ToList();
            return View();
        }

        [HttpPost]
         
        public async Task<IActionResult> CreateAccess(WorkflowAccess access)
        {
            if (ModelState.IsValid)
            {
                access.CreatedDate = DateTime.Now;
                _context.WorkflowAccesses.Add(access);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Access));
            }
            ViewBag.Users = _context.Users
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToList();
            ViewBag.Sections = _context.WorkflowSections
                .Select(s => new { s.SectionCode, s.SectionName })
                .ToList();
            return View(access);
        }

        public async Task<IActionResult> EditAccess(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var access = await _context.WorkflowAccesses.FindAsync(id);
            if (access == null)
            {
                return NotFound();
            }
            ViewBag.Users = _context.Users
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToList();
            ViewBag.Sections = _context.WorkflowSections
                .Select(s => new { s.SectionCode, s.SectionName })
                .ToList();
            return View(access);
        }

        [HttpPost]
         
        public async Task<IActionResult> EditAccess(int id, WorkflowAccess access)
        {
            if (id != access.WorkflowAccessID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(access);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccessExists(access.WorkflowAccessID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Access));
            }
            ViewBag.Users = _context.Users
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToList();
            ViewBag.Sections = _context.WorkflowSections
                .Select(s => new { s.SectionCode, s.SectionName })
                .ToList();
            return View(access);
        }

        public async Task<IActionResult> DeleteAccess(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var access = await _context.WorkflowAccesses
                .FirstOrDefaultAsync(m => m.WorkflowAccessID == id);
            if (access == null)
            {
                return NotFound();
            }

            return View(access);
        }

        [HttpPost, ActionName("DeleteAccess")]
         
        public async Task<IActionResult> DeleteAccessConfirmed(int id)
        {
            var access = await _context.WorkflowAccesses.FindAsync(id);
            _context.WorkflowAccesses.Remove(access);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Access));
        }

        private bool SectionExists(int id)
        {
            return _context.WorkflowSections.Any(e => e.WorkflowSectionID == id);
        }

        private bool AccessExists(int id)
        {
            return _context.WorkflowAccesses.Any(e => e.WorkflowAccessID == id);
        }

    }

}

