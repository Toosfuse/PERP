using ERP.Data;
using ERP.Models.asset;
using ERP.Services;
using ERP.ViewModels.Assest;
using ERP.ViewModels.AssestViewModel;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Controllers
{
    public class AssestController : Controller
    {
        private readonly ERPContext _context;
        private readonly IServices _services;

        public AssestController(ERPContext context, IServices services)
        {
            _context = context;
            _services = services;
        }

        #region User
        public IActionResult RegisterUser()
        {
            return View();
        }

        public async Task<IActionResult> AssestUsers_read([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _context.AssestUsers
                .OrderByDescending(x => x.Id)
                .Select(x => new AssestUserViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Family = x.Family,
                    AssestUserTypes = x.AssestUserTypes,
                    IsActive = x.IsActive,
                    CreatedAt = _services.iGregorianToPersianDateTime(x.CreatedAt)
                })
                .ToListAsync();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser([DataSourceRequest] DataSourceRequest request, AssestUserViewModel model)
        {
            ModelState.Remove("CreatedAt");

            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var exists = await _context.AssestUsers.AnyAsync(x =>
                x.Name == model.Name &&
                x.Family == model.Family &&
                x.AssestUserTypes == model.AssestUserTypes);

            if (exists)
            {
                ModelState.AddModelError("", "??? ???? ????? ??? ??? ???");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }

            var entity = new AssestUser
            {
                Name = model.Name.Trim(),
                Family = model.Family.Trim(),
                AssestUserTypes = model.AssestUserTypes,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.AssestUsers.Add(entity);
            await _context.SaveChangesAsync();

            model.Id = entity.Id;
            model.IsActive = entity.IsActive;
            model.CreatedAt = _services.iGregorianToPersianDateTime(entity.CreatedAt);

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssestUsers_update([DataSourceRequest] DataSourceRequest request, AssestUserViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = await _context.AssestUsers.FindAsync(model.Id);
            if (entity != null)
            {
                entity.Name = model.Name.Trim();
                entity.Family = model.Family.Trim();
                entity.AssestUserTypes = model.AssestUserTypes;
                entity.IsActive = model.IsActive;
                entity.CreatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            model.CreatedAt = _services.iGregorianToPersianDateTime(entity.CreatedAt);
            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssestUsers_destroy([DataSourceRequest] DataSourceRequest request, AssestUserViewModel model)
        {
            var entity = await _context.AssestUsers.FindAsync(model.Id);
            if (entity != null)
            {
                _context.AssestUsers.Remove(entity);
                await _context.SaveChangesAsync();
            }

            return Json(new[] { model }.ToDataSourceResult(request));
        }
        #endregion

        #region Category
        public IActionResult RegisterCategory()
        {
            return View();
        }

        public async Task<IActionResult> Categories_Read([DataSourceRequest] DataSourceRequest request)
        {
            var query = _context.AssestCategories
                .AsNoTracking()
                .Include(x => x.Parent)
                .Select(x => new AssestCategoryViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    ParentId = x.ParentId,
                    ParentTitle = x.Parent != null ? x.Parent.Title : null,
                    IsActive = x.IsActive
                });

            var data = await query.ToListAsync();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Categories_Create([DataSourceRequest] DataSourceRequest request, AssestCategoryViewModel model)
        {
            ModelState.Remove("ParentTitle");

            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = new AssestCategory
            {
                Title = model.Title?.Trim(),
                ParentId = model.ParentId > 0 ? model.ParentId : null,
                IsActive = model.IsActive
            };

            _context.AssestCategories.Add(entity);
            await _context.SaveChangesAsync();

            model.Id = entity.Id;
            if (model.ParentId.HasValue)
            {
                model.ParentTitle = await _context.AssestCategories
                    .Where(p => p.Id == model.ParentId)
                    .Select(p => p.Title)
                    .FirstOrDefaultAsync() ?? null;
            }

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Categories_Update([DataSourceRequest] DataSourceRequest request, AssestCategoryViewModel model)
        {
            ModelState.Remove("ParentTitle");

            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = await _context.AssestCategories.FindAsync(model.Id);
            if (entity == null)
            {
                ModelState.AddModelError("", "????????? ???? ???");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }

            entity.Title = model.Title?.Trim();
            entity.ParentId = model.ParentId > 0 ? model.ParentId : null;
            entity.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            if (model.ParentId.HasValue)
            {
                model.ParentTitle = await _context.AssestCategories
                    .Where(p => p.Id == model.ParentId)
                    .Select(p => p.Title)
                    .FirstOrDefaultAsync() ?? null;
            }

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Categories_Destroy([DataSourceRequest] DataSourceRequest request, AssestCategoryViewModel model)
        {
            var entity = await _context.AssestCategories
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (entity != null)
            {
                DeleteCategoryRecursive(entity);
                await _context.SaveChangesAsync();
            }

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        private void DeleteCategoryRecursive(AssestCategory category)
        {
            if (category == null) return;

            foreach (var child in category.Children.ToList())
            {
                DeleteCategoryRecursive(child);
            }

            _context.AssestCategories.Remove(category);
        }

        public async Task<IActionResult> ParentCategories()
        {
            var data = await _context.AssestCategories
                .AsNoTracking()
                .Select(x => new { id = x.Id, title = x.Title ?? "???? ?????" })
                .OrderBy(x => x.title)
                .ToListAsync();

            return Json(data);
        }
        #endregion

        #region Asset
        public IActionResult RegisterAssest()
        {
            return View();
        }

        public async Task<IActionResult> Assets_Read([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _context.Assets
                .Include(a => a.Category)
                .OrderByDescending(a => a.Id)
                .Select(a => new AssetViewModel
                {
                    Id = a.Id,
                    AssetCode = a.AssetCode,
                    AssetName = a.AssetName,
                    CategoryId = a.CategoryId,
                    CategoryTitle = a.Category.Title,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt.ToString("yyyy/MM/dd HH:mm")
                }).ToListAsync();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assets_Create([DataSourceRequest] DataSourceRequest request, AssetViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var exists = await _context.Assets.AnyAsync(x => x.AssetCode == model.AssetCode);
            if (exists)
            {
                ModelState.AddModelError("AssetCode", "??? ?? ????? ????? ??? ??? ???");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }

            var entity = new Asset
            {
                AssetCode = model.AssetCode?.Trim(),
                AssetName = model.AssetName?.Trim(),
                CategoryId = model.CategoryId,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now
            };
            _context.Assets.Add(entity);
            await _context.SaveChangesAsync();

            model.Id = entity.Id;
            model.CreatedAt = entity.CreatedAt.ToString("yyyy/MM/dd HH:mm");
            model.CategoryTitle = (await _context.AssestCategories.FindAsync(entity.CategoryId))?.Title;

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assets_Update([DataSourceRequest] DataSourceRequest request, AssetViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = await _context.Assets.FindAsync(model.Id);
            if (entity == null)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            entity.AssetCode = model.AssetCode?.Trim();
            entity.AssetName = model.AssetName?.Trim();
            entity.CategoryId = model.CategoryId;
            entity.IsActive = model.IsActive;
            await _context.SaveChangesAsync();

            model.CreatedAt = entity.CreatedAt.ToString("yyyy/MM/dd HH:mm");
            model.CategoryTitle = (await _context.AssestCategories.FindAsync(entity.CategoryId))?.Title;

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assets_Destroy([DataSourceRequest] DataSourceRequest request, AssetViewModel model)
        {
            var entity = await _context.Assets.FindAsync(model.Id);
            if (entity != null)
            {
                _context.Assets.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return Json(new[] { model }.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GetAssets()
        {
            var data = await _context.Assets
                .AsNoTracking()
                .Select(x => new { id = x.Id, name = x.AssetName })
                .ToListAsync();
            return Json(data);
        }

        public async Task<IActionResult> GetCategories()
        {
            var data = await _context.AssestCategories
                .AsNoTracking()
                .Select(x => new { id = x.Id, name = x.Title })
                .ToListAsync();
            return Json(data);
        }

        public async Task<IActionResult> GetAssetUsers()
        {
            var data = await _context.AssestUsers
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new { id = x.Id, name = $"{x.Name} {x.Family}" })
                .ToListAsync();
            return Json(data);
        }

        public async Task<IActionResult> GetAssetsForAssignment()
        {
            var data = await _context.Assets
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new { id = x.Id, name = x.AssetName, code = x.AssetCode })
                .ToListAsync();
            return Json(data);
        }

        public async Task<IActionResult> GetAssetPropertiesForAssignment(int assetId)
        {
            var data = await _context.AssetProperties
                .AsNoTracking()
                .Where(x => x.AssetId == assetId && x.IsActive)
                .Select(x => new 
                { 
                    id = x.Id, 
                    name = x.PropertyName, 
                    serialNumber = x.SerialNumber,
                    model = x.Model,
                    brand = x.Brand,
                    value = x.PropertyValue
                })
                .ToListAsync();
            return Json(data);
        }
        #endregion

        #region AssetProperty
        public IActionResult RegisterAssetProperty()
        {
            return View();
        }

        public async Task<IActionResult> AssetProperties_Read([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _context.AssetProperties
                .Include(ap => ap.Asset)
                .Include(ap => ap.LastOwnerUser)
                .OrderByDescending(ap => ap.Id)
                .Select(ap => new AssetPropertyViewModel
                {
                    Id = ap.Id,
                    AssetId = ap.AssetId,
                    AssetName = ap.Asset.AssetName,
                    PropertyName = ap.PropertyName,
                    PropertyValue = ap.PropertyValue,
                    SerialNumber = ap.SerialNumber,
                    Model = ap.Model,
                    Brand = ap.Brand,
                    IsActive = ap.IsActive,
                    CreatedAt = ap.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
                    LastOwnerUserId = ap.LastOwnerUserId,
                    LastOwnerUserName = ap.LastOwnerUser != null ? $"{ap.LastOwnerUser.Name} {ap.LastOwnerUser.Family}" : ""
                }).ToListAsync();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetProperties_Create([DataSourceRequest] DataSourceRequest request, AssetPropertyViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = new AssetProperty
            {
                AssetId = model.AssetId,
                CategoryId = model.AssetId,
                PropertyName = model.PropertyName?.Trim(),
                PropertyValue = model.PropertyValue?.Trim(),
                SerialNumber = model.SerialNumber?.Trim(),
                Model = model.Model?.Trim(),
                Brand = model.Brand?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now,
                LastOwnerUserId = model.LastOwnerUserId
            };
            _context.AssetProperties.Add(entity);
            await _context.SaveChangesAsync();

            model.Id = entity.Id;
            model.CreatedAt = entity.CreatedAt.ToString("yyyy/MM/dd HH:mm");

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetProperties_Update([DataSourceRequest] DataSourceRequest request, AssetPropertyViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = await _context.AssetProperties.FindAsync(model.Id);
            if (entity == null)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            entity.PropertyName = model.PropertyName?.Trim();
            entity.PropertyValue = model.PropertyValue?.Trim();
            entity.SerialNumber = model.SerialNumber?.Trim();
            entity.Model = model.Model?.Trim();
            entity.Brand = model.Brand?.Trim();
            entity.IsActive = model.IsActive;
            entity.LastOwnerUserId = model.LastOwnerUserId;
            await _context.SaveChangesAsync();

            model.CreatedAt = entity.CreatedAt.ToString("yyyy/MM/dd HH:mm");

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetProperties_Destroy([DataSourceRequest] DataSourceRequest request, AssetPropertyViewModel model)
        {
            var entity = await _context.AssetProperties.FindAsync(model.Id);
            if (entity != null)
            {
                _context.AssetProperties.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return Json(new[] { model }.ToDataSourceResult(request));
        }
        #endregion

        #region AssetHistory
        public IActionResult RegisterAssetHistory()
        {
            return View();
        }

        public async Task<IActionResult> AssetHistories_Read([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _context.AssetHistories
                .Include(ah => ah.Asset)
                .Include(ah => ah.AssetProperty)
                .Include(ah => ah.AssestUser)
                .Include(ah => ah.AssestToUser)
                .OrderByDescending(ah => ah.Id)
                .Select(ah => new AssetHistoryViewModel
                {
                    Id = ah.Id,
                    AssetId = ah.AssetId,
                    AssetName = ah.Asset.AssetName,
                    AssetPropertyId = ah.AssetPropertyId,
                    PropertyName = ah.AssetProperty != null ? ah.AssetProperty.PropertyName : "",
                    FromUser = $"{ah.AssestUser.Name} {ah.AssestUser.Family}",
                    ToUser = $"{ah.AssestToUser.Name} {ah.AssestToUser.Family}",
                    AssignDate = ah.AssignDate.ToString("yyyy/MM/dd HH:mm"),
                    Description = ah.Description
                }).ToListAsync();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetHistories_Create([DataSourceRequest] DataSourceRequest request, AssetHistoryViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = new AssetHistory
            {
                AssetId = model.AssetId,
                AssetPropertyId = model.AssetPropertyId,
                FromUserId = int.Parse(model.FromUser.Split()[0]),
                ToUserId = int.Parse(model.ToUser.Split()[0]),
                AssignDate = DateTime.Now,
                Description = model.Description?.Trim()
            };
            _context.AssetHistories.Add(entity);
            await _context.SaveChangesAsync();

            model.Id = entity.Id;
            model.AssignDate = entity.AssignDate.ToString("yyyy/MM/dd HH:mm");

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetHistories_Update([DataSourceRequest] DataSourceRequest request, AssetHistoryViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = await _context.AssetHistories.FindAsync(model.Id);
            if (entity == null)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            entity.AssetId = model.AssetId;
            entity.AssetPropertyId = model.AssetPropertyId;
            entity.Description = model.Description?.Trim();
            await _context.SaveChangesAsync();

            model.AssignDate = entity.AssignDate.ToString("yyyy/MM/dd HH:mm");

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetHistories_Destroy([DataSourceRequest] DataSourceRequest request, AssetHistoryViewModel model)
        {
            var entity = await _context.AssetHistories.FindAsync(model.Id);
            if (entity != null)
            {
                _context.AssetHistories.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return Json(new[] { model }.ToDataSourceResult(request));
        }
        #endregion
    }
}
