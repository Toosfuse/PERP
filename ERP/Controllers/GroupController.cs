using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class GroupController : Controller
    {
        private readonly ERPContext _context;

        public GroupController(ERPContext context)
        {
            _context = context;
        }

        // نمایش لیست گروه‌ها با تعداد کاربران
        public async Task<IActionResult> Index()
        {
            var groups = await _context.Groups
                .Include(g => g.UserGroups)
                .Select(g => new GroupViewModel
                {
                    GroupID = g.GroupID,
                    Name = g.Name,
                    MemberCount = g.UserGroups.Count
                })
                .ToListAsync();
            return View(groups);
        }

        // ایجاد گروه جدید
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(Group model)
        {
            if (ModelState.IsValid)
            {
                _context.Groups.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ویرایش گروه
        public async Task<IActionResult> Edit(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        [HttpPost]

        public async Task<IActionResult> Edit(int id, Group model)
        {
            if (id != model.GroupID)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // حذف گروه
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        // تأیید حذف گروه
        [HttpPost, ActionName("Delete")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _context.Groups
                .Include(g => g.UserGroups)
                .FirstOrDefaultAsync(g => g.GroupID == id);
            if (group != null)
            {
                // حذف دستی ردیف‌های مرتبط در UserGroups
                _context.UserGroups.RemoveRange(group.UserGroups);
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // مدیریت کاربران گروه با دو لیست
        public async Task<IActionResult> ManageUsers(int id)
        {
            var group = await _context.Groups
                .Include(g => g.UserGroups)
                .FirstOrDefaultAsync(g => g.GroupID == id);

            if (group == null)
            {
                return NotFound();
            }
            var allUsers = await _context.Set<Users>().ToListAsync();
            var model = new ManageUsersViewModel
            {
                GroupID = group.GroupID,
                GroupName = group.Name,
                Members = allUsers
                    .Where(u => group.UserGroups.Any(ug => ug.UserID == u.Id))
                    .Select(u => new UserViewModel
                    {
                        UserID = u.Id,
                        FullName = $"{u.FirstName} {u.LastName}"
                    }).ToList(),
                NonMembers = allUsers
                    .Where(u => !group.UserGroups.Any(ug => ug.UserID == u.Id))
                    .Select(u => new UserViewModel
                    {
                        UserID = u.Id,
                        FullName = $"{u.FirstName} {u.LastName}"
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToGroup(int groupId, string userId)
        {
            if (!await _context.UserGroups.AnyAsync(ug => ug.GroupID == groupId && ug.UserID == userId))
            {
                _context.UserGroups.Add(new UserGroup { GroupID = groupId, UserID = userId });
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromGroup(int groupId, string userId)
        {
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.GroupID == groupId && ug.UserID == userId);
            if (userGroup != null)
            {
                _context.UserGroups.Remove(userGroup);
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }
    }

    public class GroupViewModel
    {
        public int GroupID { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
    }

    public class ManageUsersViewModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public List<UserViewModel> Members { get; set; } = new List<UserViewModel>();
        public List<UserViewModel> NonMembers { get; set; } = new List<UserViewModel>();
    }

    public class UserViewModel
    {
        public string UserID { get; set; }
        public string FullName { get; set; }
    }
}