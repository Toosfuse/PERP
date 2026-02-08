using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ERP.Controllers
{
    public partial class FileUploadController : Controller
    {
        public IWebHostEnvironment WebHostEnvironment { get; set; }
        private readonly ERPContext _context;

        public FileUploadController(IWebHostEnvironment webHostEnvironment, ERPContext context)
        {
            WebHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public async Task<ActionResult> Async_Save(IEnumerable<IFormFile> files, string guid)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    var extension = Path.GetExtension(file.FileName);
                    var fileModel = new FileDB
                    {
                        CreatedOn = DateTime.Now,
                        FileType = file.ContentType,
                        Extension = extension,
                        Name = fileName,
                        Description = "",
                        Guid = guid,
                        IsTemp = true
                    };
                    using (var dataStream = new MemoryStream())
                    {
                        await file.CopyToAsync(dataStream);
                        fileModel.Data = dataStream.ToArray();
                    }
                    _context.FileDBs.Add(fileModel);
                    //_context.SaveChanges();
                    await _context.SaveChangesAsync();

                }
            }
            return Json(new { success = true });
        }

        public async Task<IActionResult> Async_Remove(string[] fileNames, string guid)
        {
            if (fileNames != null)
            {
                foreach (var fullName in fileNames)
                {
                    var fileName = Path.GetFileNameWithoutExtension(fullName);
                    var fileid = _context.FileDBs.Where(x => x.Name == fileName && x.Guid == guid).Select(p => p.FileID).Single();
                    var file = await _context.FileDBs.FindAsync(fileid);
                    if (file != null)
                    {
                        _context.FileDBs.Remove(file);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return Json(new { success = true });
        }

        public async Task<IActionResult> Async_RemovebyID(int id)
        {
            if (id != null)
            {
                var file = await _context.FileDBs.FindAsync(id);
                if (file != null)
                {
                    _context.FileDBs.Remove(file);
                }
                await _context.SaveChangesAsync();
            }
            return Json(new { status = "OK" });
        }

        public async Task<IActionResult> DownloadFileDatabase(int id)
        {
            var file = await _context.FileDBs.Where(x => x.FileID == id).FirstOrDefaultAsync();
            if (file == null) return null;
            return File(file.Data, file.FileType, file.Name + file.Extension);
        }
    }
}