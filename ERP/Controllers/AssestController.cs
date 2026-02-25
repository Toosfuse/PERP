using ERP.Data;
using ERP.Models.asset;
using ERP.Services;
using ERP.ViewModels.Assest;
using ERP.ViewModels.AssestViewModel;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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
                ModelState.AddModelError("", "این کاربر قبلا ثبت شده است");
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
                ModelState.AddModelError("", "دستهبندی یافت نشد");
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
                .Select(x => new { id = x.Id, title = x.Title ?? "بدون نام" })
                .OrderBy(x => x.title)
                .ToListAsync();

            return Json(data);
        }
        #endregion

        #region Asset
        public IActionResult RegisterAsset()
        {
            return RedirectToAction("AssetManagement");
        }

        public IActionResult AssetManagement()
        {
            return View();
        }

        public async Task<IActionResult> Assets_Read([DataSourceRequest] DataSourceRequest request, bool showDeleted = false)
        {
            var histories = await _context.AssetHistories
                .AsNoTracking()
                .Include(h => h.ToUser)
                .GroupBy(h => h.AssetId)
                .Select(g => new { AssetId = g.Key, Latest = g.OrderByDescending(h => h.AssignDate).FirstOrDefault() })
                .ToListAsync();

            var query = _context.Assets.AsNoTracking();
            
            if (showDeleted)
                query = query.Where(x => x.IsDeleted);
            else
                query = query.Where(x => !x.IsDeleted);

            var data = await query
                .Include(x => x.Category)
                .Include(x => x.CurrentOwner)
                .OrderByDescending(x => x.Id)
                .Select(x => new AssetViewModel
                {
                    Id = x.Id,
                    AssetCode = x.AssetCode ?? "",
                    AssetName = x.AssetName ?? "",
                    PurchaseDate = x.PurchaseDate ?? "",
                    CategoryId = x.CategoryId,
                    CategoryTitle = x.Category != null ? x.Category.Title : "",
                    CurrentOwnerId = x.CurrentOwnerId,
                    CurrentOwnerName = x.CurrentOwner != null ? x.CurrentOwner.Name + " " + x.CurrentOwner.Family + (x.CurrentOwner.AssestUserTypes == AssestUserTypes.Vahed ? " - واحد" : "") : "",
                    IsActive = x.IsActive,
                    IsDeleted = x.IsDeleted,
                    CreatedAt = _services.iGregorianToPersianDateTime(x.CreatedAt),
                    LastModifiedDate = "",
                    LastModifiedBy = ""
                })
                .ToListAsync();

            foreach (var item in data)
            {
                var history = histories.FirstOrDefault(h => h.AssetId == item.Id)?.Latest;
                if (history != null)
                {
                    item.LastModifiedDate = _services.iGregorianToPersianDateTime(history.AssignDate);
                    item.LastModifiedBy = history.ToUser != null ? history.ToUser.Name + " " + history.ToUser.Family : "";
                }
                else
                {
                    item.LastModifiedDate = item.CreatedAt;
                    item.LastModifiedBy = "";
                }
            }

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assets_Create([DataSourceRequest] DataSourceRequest request, AssetViewModel model)
        {
            ModelState.Remove("CategoryTitle");
            ModelState.Remove("CurrentOwnerName");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("LastModifiedDate");
            ModelState.Remove("AssetName");
            ModelState.Remove("PurchaseDate");

            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = new Asset
            {
                AssetCode = model.AssetCode?.Trim() ?? "",
                AssetName = model.AssetName?.Trim() ?? "",
                PurchaseDate = _services.iGregorianToPersianDateTime(DateTime.Now),
                CategoryId = model.CategoryId,
                CurrentOwnerId = model.CurrentOwnerId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Assets.Add(entity);
            await _context.SaveChangesAsync();

            var category = await _context.AssestCategories.FindAsync(model.CategoryId);
            var owner = await _context.AssestUsers.FindAsync(model.CurrentOwnerId);

            model.Id = entity.Id;
            model.AssetName = entity.AssetName;
            model.PurchaseDate = entity.PurchaseDate;
            model.CategoryTitle = category?.Title;
            model.CurrentOwnerName = owner != null ? owner.Name + " " + owner.Family + (owner.AssestUserTypes == AssestUserTypes.Vahed ? " - واحد" : "") : "";
            model.IsActive = entity.IsActive;
            model.CreatedAt = _services.iGregorianToPersianDateTime(entity.CreatedAt);
            model.LastModifiedDate = _services.iGregorianToPersianDateTime(entity.CreatedAt);
            model.LastModifiedBy = "";

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assets_CreateWithItems([FromBody] JObject data)
        {
            try
            {
                var assetCode = data["assetCode"]?.ToString();
                var assetName = data["assetName"]?.ToString();
                var categoryId = data["categoryId"]?.Value<int>() ?? 0;
                var currentOwnerId = data["currentOwnerId"]?.Value<int>() ?? 0;
                var items = data["items"] as JArray;

                if (string.IsNullOrWhiteSpace(assetCode))
                    return Json(new { success = false, message = "کد اموال الزامی است" });

                var exists = await _context.Assets.AnyAsync(x => x.AssetCode == assetCode.Trim());
                if (exists)
                    return Json(new { success = false, message = "این کد اموال قبلاً ثبت شده است" });

                var asset = new Asset
                {
                    AssetCode = assetCode.Trim(),
                    AssetName = assetName.Trim(),
                    CategoryId = categoryId,
                    CurrentOwnerId = currentOwnerId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();

                if (items != null && items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var assetItem = new AssetItem
                        {
                            AssetId = asset.Id,
                            PartName = item["partName"]?.ToString()?.Trim() ?? "",
                            Color = item["color"]?.ToString()?.Trim() ?? "",
                            Description = item["description"]?.ToString()?.Trim() ?? ""
                        };
                        _context.AssetItems.Add(assetItem);
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, id = asset.Id, message = "ثبت با موفقیت انجام شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assets_Update([DataSourceRequest] DataSourceRequest request, AssetViewModel model)
        {
            ModelState.Remove("CategoryTitle");
            ModelState.Remove("CurrentOwnerName");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("LastModifiedDate");
            ModelState.Remove("LastModifiedBy");
            ModelState.Remove("PurchaseDate");

            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = await _context.Assets.FindAsync(model.Id);
            if (entity == null)
            {
                ModelState.AddModelError("", "اموال یافت نشد");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }

            var exists = await _context.Assets.AnyAsync(x => x.AssetCode == model.AssetCode.Trim() && x.Id != model.Id);
            if (exists)
            {
                ModelState.AddModelError("", "این کد اموال قبلاً ثبت شده است");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }

            int oldOwnerId = entity.CurrentOwnerId;
            entity.AssetCode = model.AssetCode.Trim();
            entity.AssetName = model.AssetName.Trim();
            entity.CategoryId = model.CategoryId;
            entity.CurrentOwnerId = model.CurrentOwnerId;
            entity.IsActive = model.IsActive;

            if (oldOwnerId != model.CurrentOwnerId)
            {
                var history = new AssetHistory
                {
                    AssetId = model.Id,
                    FromUserId = oldOwnerId,
                    ToUserId = model.CurrentOwnerId,
                    AssignDate = DateTime.Now,
                    Description = "تغییر مالک از طریق ویرایش اموال"
                };
                _context.AssetHistories.Add(history);
            }

            await _context.SaveChangesAsync();

            var category = await _context.AssestCategories.FindAsync(model.CategoryId);
            var owner = await _context.AssestUsers.FindAsync(model.CurrentOwnerId);

            model.CategoryTitle = category?.Title;
            model.CurrentOwnerName = owner != null ? owner.Name + " " + owner.Family + (owner.AssestUserTypes == AssestUserTypes.Vahed ? " - واحد" : "") : "";
            model.CreatedAt = _services.iGregorianToPersianDateTime(entity.CreatedAt);
            model.PurchaseDate = entity.PurchaseDate;

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assets_Destroy([DataSourceRequest] DataSourceRequest request, AssetViewModel model)
        {
            var entity = await _context.Assets.FindAsync(model.Id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GetCategories()
        {
            var data = await _context.AssestCategories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new { id = x.Id, title = x.Title })
                .OrderBy(x => x.title)
                .ToListAsync();

            return Json(data);
        }

        public async Task<IActionResult> GetUsers()
        {
            var data = await _context.AssestUsers
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new { id = x.Id, title = x.Name + " " + x.Family + (x.AssestUserTypes == AssestUserTypes.Vahed ? " - واحد" : "") })
                .OrderBy(x => x.title)
                .ToListAsync();

            return Json(data);
        }
        #endregion

        #region AssetHistory
        public IActionResult AssetHistory(int? assetId)
        {
            return View(assetId);
        }

        public async Task<IActionResult> GetAssetCode(int assetId)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .Where(x => x.Id == assetId)
                .Select(x => x.AssetCode)
                .FirstOrDefaultAsync();

            return Json(new { assetCode = asset ?? "" });
        }

        public async Task<IActionResult> AssetHistory_Read([DataSourceRequest] DataSourceRequest request, string assetCode)
        {
            assetCode = assetCode?.Trim();
            var query = _context.AssetHistories.AsNoTracking();

            if (!string.IsNullOrEmpty(assetCode))
            {
                var assetIds = await _context.Assets
                    .Where(x => x.AssetCode.Trim() == assetCode)
                    .Select(x => x.Id)
                    .ToListAsync();

                if (assetIds.Any())
                {
                    query = query.Where(x => assetIds.Contains(x.AssetId));
                }
            }

            var data = await query
                .Include(x => x.Asset)
                .Include(x => x.FromUser)
                .Include(x => x.ToUser)
                .OrderByDescending(x => x.Id)
                .Select(x => new AssetHistoryViewModel
                {
                    Id = x.Id,
                    AssetId = x.AssetId,
                    AssetCode = x.Asset.AssetCode,
                    AssetName = x.Asset.AssetName,
                    FromUserId = x.FromUserId,
                    FromUserName = x.FromUser.Name + " " + x.FromUser.Family + (x.FromUser.AssestUserTypes == AssestUserTypes.Vahed ? " - واحد" : ""),
                    ToUserId = x.ToUserId,
                    ToUserName = x.ToUser.Name + " " + x.ToUser.Family + (x.ToUser.AssestUserTypes == AssestUserTypes.Vahed ? " - واحد" : ""),
                    AssignDate = _services.iGregorianToPersianDateTime(x.AssignDate),
                    Description = x.Description
                })
                .ToListAsync();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetHistory_Create([DataSourceRequest] DataSourceRequest request, AssetHistoryViewModel model)
        {
            ModelState.Remove("AssetCode");
            ModelState.Remove("AssetName");
            ModelState.Remove("FromUserName");
            ModelState.Remove("ToUserName");
            ModelState.Remove("AssignDate");

            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var asset = await _context.Assets.FindAsync(model.AssetId);
            if (asset == null)
            {
                ModelState.AddModelError("", "اموال یافت نشد");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }

            var history = new AssetHistory
            {
                AssetId = model.AssetId,
                FromUserId = model.FromUserId,
                ToUserId = model.ToUserId,
                AssignDate = DateTime.Now,
                Description = model.Description?.Trim()
            };

            _context.AssetHistories.Add(history);
            asset.CurrentOwnerId = model.ToUserId;
            await _context.SaveChangesAsync();

            var fromUser = await _context.AssestUsers.FindAsync(model.FromUserId);
            var toUser = await _context.AssestUsers.FindAsync(model.ToUserId);

            model.Id = history.Id;
            model.AssetCode = asset.AssetCode;
            model.AssetName = asset.AssetName;
            model.FromUserName = fromUser != null ? fromUser.Name + " " + fromUser.Family + (fromUser.AssestUserTypes == AssestUserTypes.Vahed ? " - واحد" : "") : "";
            model.ToUserName = toUser != null ? toUser.Name + " " + toUser.Family + (toUser.AssestUserTypes == AssestUserTypes.Vahed ? " - واحد" : "") : "";
            model.AssignDate = _services.iGregorianToPersianDateTime(history.AssignDate);

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GetAssets()
        {
            var data = await _context.Assets
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new { id = x.Id, title = x.AssetCode + " - " + x.AssetName })
                .OrderBy(x => x.title)
                .ToListAsync();

            return Json(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetHistory_Destroy([DataSourceRequest] DataSourceRequest request, AssetHistoryViewModel model)
        {
            var entity = await _context.AssetHistories.FindAsync(model.Id);
            if (entity != null)
            {
                _context.AssetHistories.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return Json(new[] { model }.ToDataSourceResult(request));
        }

        #region AssetItem
        public async Task<IActionResult> AssetItems_Read([DataSourceRequest] DataSourceRequest request, int assetId)
        {
            var data = await _context.AssetItems
                .AsNoTracking()
                .Where(x => x.AssetId == assetId)
                .Select(x => new AssetItemViewModel
                {
                    Id = x.Id,
                    AssetId = x.AssetId,
                    PartName = x.PartName,
                    Color = x.Color,
                    Description = x.Description
                })
                .ToListAsync();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetItems_Create([FromForm] int assetId, [FromForm] string partName, [FromForm] string color, [FromForm] string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(partName))
                    return Json(new { success = false, message = "نام قطعه الزامی است" });

                if (assetId <= 0)
                    return Json(new { success = false, message = "AssetId باید بزرگتر از 0 باشد" });

                var asset = await _context.Assets.FindAsync(assetId);
                if (asset == null)
                    return Json(new { success = false, message = $"اموال با ID {assetId} وجود ندارد" });

                var entity = new AssetItem
                {
                    AssetId = assetId,
                    PartName = partName?.Trim() ?? "",
                    Color = color?.Trim() ?? "",
                    Description = description?.Trim() ?? ""
                };

                _context.AssetItems.Add(entity);
                await _context.SaveChangesAsync();

                return Json(new { success = true, id = entity.Id });
            }
            catch (Exception ex)
            {
                var innerEx = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = innerEx });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetItems_Update([DataSourceRequest] DataSourceRequest request, AssetItemViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));

            var entity = await _context.AssetItems.FindAsync(model.Id);
            if (entity != null)
            {
                entity.PartName = model.PartName?.Trim();
                entity.Color = model.Color?.Trim();
                entity.Description = model.Description?.Trim();
                await _context.SaveChangesAsync();
            }

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetItems_Destroy([DataSourceRequest] DataSourceRequest request, AssetItemViewModel model)
        {
            var entity = await _context.AssetItems.FindAsync(model.Id);
            if (entity != null)
            {
                _context.AssetItems.Remove(entity);
                await _context.SaveChangesAsync();
            }

            return Json(new[] { model }.ToDataSourceResult(request));
        }
        #endregion

        public async Task<IActionResult> GetAssetsByUser(int userId)
        {
            var data = await _context.Assets
                .AsNoTracking()
                .Where(x => x.CurrentOwnerId == userId && !x.IsDeleted)
                .Select(x => new { assetCode = x.AssetCode, assetName = x.AssetName })
                .OrderBy(x => x.assetCode)
                .ToListAsync();

            return Json(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferAssets(int fromUserId, int toUserId)
        {
            try
            {
                var assets = await _context.Assets
                    .Where(x => x.CurrentOwnerId == fromUserId && !x.IsDeleted)
                    .ToListAsync();

                if (assets.Count == 0)
                    return Json(new { success = false, message = "هیچ وسیله ای برای انتقال وجود ندارد" });

                foreach (var asset in assets)
                {
                    var history = new AssetHistory
                    {
                        AssetId = asset.Id,
                        FromUserId = fromUserId,
                        ToUserId = toUserId,
                        AssignDate = DateTime.Now,
                        Description = "انتقال دسته جمعی وسایل"
                    };
                    _context.AssetHistories.Add(history);
                    asset.CurrentOwnerId = toUserId;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"{assets.Count} وسیله با موفقیت انتقال یافت" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
      

        public async Task<IActionResult> DeletedAssets_Read([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _context.Assets
                .AsNoTracking()
                .Where(x => x.IsDeleted)
                .Include(x => x.Category)
                .Include(x => x.CurrentOwner)
                .OrderByDescending(x => x.Id)
                .Select(x => new AssetViewModel
                {
                    Id = x.Id,
                    AssetCode = x.AssetCode,
                    AssetName = x.AssetName,
                    CategoryId = x.CategoryId,
                    CategoryTitle = x.Category.Title,
                    CurrentOwnerId = x.CurrentOwnerId,
                    CurrentOwnerName = x.CurrentOwner.Name + " " + x.CurrentOwner.Family,
                    IsActive = x.IsActive,
                    CreatedAt = _services.iGregorianToPersianDateTime(x.CreatedAt)
                })
                .ToListAsync();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletedAssets_Restore([DataSourceRequest] DataSourceRequest request, AssetViewModel model)
        {
            var entity = await _context.Assets.FindAsync(model.Id);
            if (entity != null)
            {
                entity.IsDeleted = false;
                await _context.SaveChangesAsync();
            }

            return Json(new[] { model }.ToDataSourceResult(request));
        }

        public IActionResult UserAssets(int userId, string userName)
        {
            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            return View();
        }

        public IActionResult TransferAssets(int userId, string userName)
        {
            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            return View();
        }

        public async Task<IActionResult> UserAssets_Read([DataSourceRequest] DataSourceRequest request, int userId)
        {
            var data = await _context.Assets
                .AsNoTracking()
                .Where(x => x.CurrentOwnerId == userId && !x.IsDeleted)
                .Include(x => x.Category)
                .OrderByDescending(x => x.Id)
                .Select(x => new AssetViewModel
                {
                    Id = x.Id,
                    AssetCode = x.AssetCode,
                    AssetName = x.AssetName,
                    CategoryTitle = x.Category.Title,
                    IsActive = x.IsActive,
                    CreatedAt = _services.iGregorianToPersianDateTime(x.CreatedAt)
                })
                .ToListAsync();

            return Json(data.ToDataSourceResult(request));
        }
        #endregion
    }
}
