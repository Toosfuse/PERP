using ERP.Data;
using ERP.ModelsEMP;
using ERP.Services;
using ERP.ViewModels;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using Stimulsoft.Blockly.Model;
using Stimulsoft.System.Windows.Forms;
using System;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;


namespace ERP.Controllers
{
    public class PackagOrdersController : Controller
    {
        private readonly EMPContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IServices _services;

        public PackagOrdersController(EMPContext context, IWebHostEnvironment env, IServices services)
        {
            _context = context;
            _env = env;
            _services = services;
        }

        // بررسی فرمت سریال
        public bool CheckSerialFormat(string Serial)
        {
            if (string.IsNullOrEmpty(Serial) || Serial.Length < 13)
                return false;
            if (!Serial.StartsWith("18"))
                return false;

            string[] patterns = {
                @"^\d{2}-\d{1}-\d{2}-\d{8}$", // 18-6-10-45100000
                @"^\d{13}$",                 // 1861045100000
                @"^\d{14}$"                  // 18610451000000
            };
            return patterns.Any(p => System.Text.RegularExpressions.Regex.IsMatch(Serial, p));
        }

        // بررسی تکراری بودن شماره سفارش
        public bool CheckSerilaOrder(string OrderNumber)
        {
            if (string.IsNullOrWhiteSpace(OrderNumber))
                return false;
            return _context.Orders.Any(x => x.OrderNumber == OrderNumber.Trim());
        }


        public bool CheckCodeProduct(string CodeProduct)
        {
            if (string.IsNullOrWhiteSpace(CodeProduct))
                return false;
            return _context.Orders.Any(x => x.CodeProduct == CodeProduct.Trim());
        }



        // GET: PackagOrders/Index
        public async Task<IActionResult> Index(int? cityId, string search)
        {
            ViewBag.Cities = await _context.Cities
                .Select(c => new SelectListItem { Value = c.CityId.ToString(), Text = c.CityName ?? "نامشخص" })
                .OrderBy(c => c.Text)
                .ToListAsync();

            var ordersQuery = _context.Orders.Include(o => o.City).AsQueryable();

            if (cityId.HasValue && cityId > 0)
            {
                ordersQuery = ordersQuery.Where(o => o.CityId == cityId);
                ViewBag.SelectedCityId = cityId;
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    (o.OrderRegNumber != null && o.OrderRegNumber.Contains(search)) ||
                    (o.CustomerName != null && o.CustomerName.Contains(search)) ||
                    (o.City != null && o.City.CityName.Contains(search)));
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderId)
                .Select(o => new OrderViewModel
                {
                    Id = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    OrderRegNumber = o.OrderNumber,
                    RegDate =_services.iGregorianToPersian(o.RegDate),
                    CityId = o.CityId,
                    CityName = o.City != null ? o.City.CityName ?? "نامشخص" : "نامشخص",
                    StartSerial = o.StartSerial,
                    EndSerial = o.EndSerial,
                    CustomerName = o.CustomerName,
                    CodeProduct = o.CodeProduct
                })
                .ToListAsync();

           

            return View(orders);
        }
        private int GetCurrentUserId()
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return userId;
            return 1; // در پروژه واقعی بهتر است خطا یا لاگین اجباری باشد
        }

        #region CreatAccessDB
        [HttpGet]
        public async Task<IActionResult> CreateKeyAccessDB(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new { success = false, message = "شناسه سفارش نامعتبر است." });
            }

            var package = _context.Packages.SingleOrDefault(x => x.OrderId == id);
            if (package != null)
            {
                var order=_context.Orders.SingleOrDefault(x => x.OrderId == package.OrderId).CityId;
                var city = _context.Cities.SingleOrDefault(x => x.CityId == order.Value).CityNameEn;
            }
            if (package == null)
            {
                return Json(new { success = false, message = "سفارش یافت نشد." });
            }

            string str = package.PackagingId.ToString();
            string dbFolder = Path.Combine(_env.ContentRootPath, "DataAccess");
            Directory.CreateDirectory(dbFolder);
            string fileName = "key.accdb"; // می‌تونی منحصربه‌فرد کنی: $"key_{id}.accdb"
            string path = Path.Combine(dbFolder, fileName);

            bool result = NewSaveAsOutFileKey(str, "", path, "AccessDB");

            if (!result || !System.IO.File.Exists(path))
            {
                return Json(new { success = false, message = "خطایی در ساخت فایل کلیدی رخ داد." });
            }

            // موفقیت: فایل را برای دانلود برمی‌گردانیم
            var fileBytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(fileBytes, "application/vnd.ms-access", fileName);
        }
        public bool NewSaveAsOutFileKey(string strPackagingId, string title, string path, string strTypeOut)
        {
            bool res = false;



            try
            {
                DataTable dt = GetSerialKeyPackage(strPackagingId);
                if (dt != null)
                {
                    if (strTypeOut == "Excel")
                    {
                        WriteExcelWithNPOI("xls", dt, path);
                    }
                    else
                        if (strTypeOut == "AccessDB")
                    {
                        WritAccessDb(dt, path);
                    }


                    res = true;
                }
                else
                {
                    res = false;
                    return res;
                }

            }
            catch (Exception ex)
            {
              
            }

            return true;
        }

        public DataTable GetSerialKeyPackage(string strPackagingId)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable DTResults = new DataTable();
                //**********************************************************************************************sql exec procedure
                             using var cmd = _context.Database.GetDbConnection().CreateCommand();

                //string connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\MainProj\DB\kontor_Database.accdb";
                //string connString ="Provider=Microsoft.ACE.OLEDB.12.0;Data Source=kontor_Database.accdb";
                using (SqlConnection cnn = new SqlConnection(cmd.Connection.ConnectionString))
                {
                    cnn.Open();
                    //OleDbDataReader reader = new OleDbDataReader();
                    //OleDbCommand command = new OleDbCommand("SELECT * from  Users WHERE LastName='@1'", connection);
                    //command.Parameters.AddWithValue("@1", userName)
                    // OleDbCommand command = new OleDbCommand("SELECT * from  tblobisekontor order by id ", connection);
                    //OleDbDataReader reader = command.ExecuteReader();

                    SqlDataAdapter sqldataAdapter;

                    sqldataAdapter = new SqlDataAdapter(" EXEC  GetSerialKeyPackage @packageid = " + strPackagingId, cnn);
                    sqldataAdapter.Fill(ds);
                    //int i = 0;
                    //for (i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                    //{
                    //listBox2.Items.Add(ds.Tables[0].Rows[i].ItemArray[0] + " -- " + ds.Tables[0].Rows[i].ItemArray[1] + " -- " + ds.Tables[0].Rows[i].ItemArray[2] + " -- " + ds.Tables[0].Rows[i].ItemArray[3] + " -- " + ds.Tables[0].Rows[i].ItemArray[4]);
                    // }
                }

                DTResults = ds.Tables[0];
                return DTResults;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void WriteExcelWithNPOI(String extension, DataTable dt, string path)
        {
            // dll referred NPOI.dll and NPOI.OOXML 

            IWorkbook workbook;

            //if (extension == "xlsx")
            //{
            //    workbook = new XSSFWorkbook();
            //}
            //else
            if (extension == "xls")
            {
                workbook = new HSSFWorkbook();
            }
            else
            {
                throw new Exception("This format is not supported");
            }

            ISheet sheet1 = workbook.CreateSheet("Sheet 1");

            //make a header row 
            IRow row1 = sheet1.CreateRow(0);

            for (int j = 0; j < dt.Columns.Count; j++)
            {

                ICell cell = row1.CreateCell(j);

                String columnName = dt.Columns[j].ToString();
                cell.SetCellValue(columnName);
            }

            //loops through data 
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet1.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {

                    ICell cell = row.CreateCell(j);
                    String columnName = dt.Columns[j].ToString();
                    cell.SetCellValue(dt.Rows[i][columnName].ToString());
                }
            }
            FileStream file = new FileStream(path, FileMode.Create);
            workbook.Write(file);
            file.Close();
            /* using (var exportData = new MemoryStream())
             {
                 Response.Clear();
                 workbook.Write(exportData);
                 //if (extension == "xlsx") //xlsx file format 
                 //{
                 //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                 //    Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "tpms_Dict.xlsx"));
                 //    Response.BinaryWrite(exportData.ToArray());
                 //}
                 //else
                 if (extension == "xls")  //xls file format 
                 {
                     Response.ContentType = "application/vnd.ms-excel";
                     Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "tpms_dict.xls"));
                     Response.BinaryWrite(exportData.GetBuffer());
                 }
                 Response.End();
             } */
        }
        public void WritAccessDb(DataTable dt, string path)
        {
            //The connection strings needed: One for SQL and one for Access
            //String accessConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\...\\test.accdb;";
            // path = "";
            String accessConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + " ;";
            //String accessConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\DBAccesskey\key.accdb ; " + "  Jet OLEDB:Database Password = 123456; ";
            //String accessConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\DBAccesskey\key.accdb ; "; 
            //Make adapters for each table we want to export


            //Create an empty Access file that we will fill with data from the data set
            //ADOX.Catalog catalog = new ADOX.Catalog();
            //catalog.Create(accessConnectionString);

            //Create an Access connection and a command that we'll use
            OleDbConnection accessConnection = new OleDbConnection(accessConnectionString);
            OleDbCommand command = new OleDbCommand();
            command.Connection = accessConnection;
            command.CommandType = System.Data.CommandType.Text;
            accessConnection.Open();

            //This loop creates the structure of the database

            // String columnsCommandText = "(";
            //foreach (DataColumn column in dt.Columns)
            //{
            //    String columnName = column.ColumnName;
            //    String dataTypeName = column.DataType.Name;
            //    String sqlDataTypeName = getSqlDataTypeName(dataTypeName);
            //    columnsCommandText += "[" + columnName + "] " + sqlDataTypeName + ",";
            //}
            //columnsCommandText = columnsCommandText.Remove(columnsCommandText.Length - 1);
            //columnsCommandText += ")";

            // command.CommandText = "CREATE TABLE " + table.TableName + columnsCommandText;
            /*
            //****************************Create table accessDB
            ADOX.Table tblSerialKey = new ADOX.Table();
            tblSerialKey.Name = "serialkey";
            tblSerialKey.Columns.Append("Radif", DataTypeEnum.adInteger);
            tblSerialKey.Columns.Append("PackagingId", DataTypeEnum.adInteger);
            tblSerialKey.Columns.Append("Serialkey", DataTypeEnum.adVarChar);

            catalog.Tables.Append(tblSerialKey);
            */
            //****************************
            /*
            command.CommandText = "CREATE TABLE " + "serialkey " + "[] int";
                command.ExecuteNonQuery();
            */

            //This loop fills the database with all information
            //foreach (DataTable table in dataSet.Tables)
            //{
            command.CommandText = "  delete from AkEkBkMk ";
            command.ExecuteNonQuery();
            foreach (DataRow row in dt.Rows)
            {
                String commandText = "INSERT INTO " + "AkEkBkMk" + " VALUES (";
                foreach (var item in row.ItemArray)
                {
                    commandText += "'" + item.ToString() + "',";
                }
                commandText = commandText.Remove(commandText.Length - 1);
                commandText += ")";

                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
            //}

            accessConnection.Close();
        }

        #endregion
        // GET: PackagOrders/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.RegDatePersian = DateTime.Now.ToPersianDateTime().ToString("yyyy/MM/dd");

            await LoadDropdownsAsync();

            ViewData["Title"] = "ایجاد سفارش بسته‌بندی جدید";

            var model = new OrderViewModel
            {
                Packages = new PackageViewModel
                {
                    ShowInBarcodeScan = true
                }
            };

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model, string RegDatePersian)
        {
            

            model.OrderRegNumber = model.OrderNumber?.Trim();
            model.Packages ??= new PackageViewModel();

            // ==============================================================
            // اعتبارسنجی فیلدهای الزامی
            // ==============================================================

            if (string.IsNullOrWhiteSpace(model.OrderNumber))
                ModelState.AddModelError("OrderNumber", "شماره سفارش الزامی است.");

            if (string.IsNullOrWhiteSpace(model.CodeProduct))
                ModelState.AddModelError("CodeProduct", "کد محصول الزامی است.");

            if (string.IsNullOrWhiteSpace(model.StartSerial))
                ModelState.AddModelError("StartSerial", "سریال اولیه الزامی است.");

            if (string.IsNullOrWhiteSpace(model.EndSerial))
                ModelState.AddModelError("EndSerial", "سریال انتهایی الزامی است.");

            if (model.CityId == null || model.CityId <= 0)
                ModelState.AddModelError("CityId", "انتخاب شهر مقصد الزامی است.");

            if (CheckSerilaOrder(model.OrderNumber?.Trim()))
                ModelState.AddModelError("OrderNumber", "شماره سفارش قبلاً ثبت شده است.");

            if (CheckCodeProduct(model.CodeProduct?.Trim()))
                ModelState.AddModelError("CodeProduct", "کد محصول قبلاً برای سفارش دیگری ثبت شده است.");

            // چک فرمت و ترتیب سریال‌ها
            if (!string.IsNullOrWhiteSpace(model.StartSerial) && !string.IsNullOrWhiteSpace(model.EndSerial))
            {
                if (!CheckSerialFormat(model.StartSerial.Trim()))
                    ModelState.AddModelError("StartSerial", "فرمت سریال اولیه صحیح نیست.");
                else if (!CheckSerialFormat(model.EndSerial.Trim()))
                    ModelState.AddModelError("EndSerial", "فرمت سریال انتهایی صحیح نیست.");
                else if (long.TryParse(model.StartSerial.Trim(), out long start) &&
                         long.TryParse(model.EndSerial.Trim(), out long end))
                {
                    if (start > end)
                        ModelState.AddModelError("StartSerial", "سریال اولیه باید کوچک‌تر یا مساوی سریال انتهایی باشد.");
                    else
                    {
                        string conflictMessage = "";
                        var (hasConflict, conflictCount) = await IsAnySerialInRangeExistsMeter(model.StartSerial.Trim(), model.EndSerial.Trim());
                        if (hasConflict) conflictMessage += $"تعداد {conflictCount} سفارش از این بازه قبلاً ثبت شده است.\n";

                        int orderIdConflict = await GetConflictingOrderIdAsync(model.StartSerial.Trim(), model.EndSerial.Trim());
                        if (orderIdConflict > 0) conflictMessage += $"تداخل با سفارش شماره {orderIdConflict}\n";

                        int packageIdConflict = await GetConflictingPackageIdAsync(model.StartSerial.Trim(), model.EndSerial.Trim());
                        if (packageIdConflict > 0) conflictMessage += $"تداخل با پکیج شماره {packageIdConflict}\n";

                        var (hasDeviceConflict, deviceCount) = await IsAnySerialInRangeExistMeterDevice(model.StartSerial.Trim(), model.EndSerial.Trim());
                        if (hasDeviceConflict) conflictMessage += $"تعداد {deviceCount} سریال در تست کنتور ثبت شده است.\n";

                        if (!string.IsNullOrEmpty(conflictMessage))
                            ModelState.AddModelError("StartSerial", conflictMessage.TrimEnd('\n'));
                    }
                }
            }

            // پاک کردن فیلدهای نمایشی
            ModelState.Remove("CityName");
            //ModelState.Remove("RegDatePersian");
            ModelState.Remove("OrderRegNumber");
            //ModelState.Remove("StartSerial");
            //ModelState.Remove("EndSerial");
            ModelState.Remove("CustomerName");

            ModelState.Remove("Id");

            ModelState.Remove("OrderRegNumber");


            if (model.Packages != null)
            {
                var displayFields = new[] { "MeterTypeName", "MeterBaseName", "CoverTypeName", "PackageTypeName",
                    "ModuleTypeName", "BoardTypeName", "AccessoriesTypeName", "RsPortTypeName",
                    "MeterCount", "ProfileName", "CheckSum", "FrimWareVersion", "ShowInBarcodeScan","StartSerial","EndSerial" };

                foreach (var field in displayFields)
                    ModelState.Remove($"Packages.{field}");
            }
            // ==============================================================
            // مرحله ۳: فیلتر کردن خطاها — فقط خطاهای واقعی نمایش داده شوند
            // ==============================================================

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors.Select(e => new
                    {
                        Field = x.Key,
                        Message = e.ErrorMessage
                    }))
                    // فقط خطاهای فارسی و واقعی (که خودت اضافه کردی) را نگه دار
                    .Where(err =>
                        err.Message.Contains("تداخل") ||
                        err.Message.Contains("قبلاً ثبت شده") ||
                        err.Message.Contains("فرمت سریال") ||
                        err.Message.Contains("کوچک‌تر") ||
                        err.Message.Contains("الزامی است") ||
                        err.Message.Contains("انتخاب کنید") ||
                        err.Message.Contains("لطفاً")
                        
                        
                       
)
                    .ToList();

                // فقط اگر خطای واقعی داشت، به SweetAlert بفرست
                if (errors.Any())
                {
                    ViewBag.ModelStateErrors = errors;
                }

                await LoadDropdownsAsync();
                ViewBag.RegDatePersian = RegDatePersian;
                return View(model);
            }

            // ==============================================================
            // ذخیره در دیتابیس
            // ==============================================================

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    CityId = model.CityId.Value,
                    OrderNumber = model.OrderNumber?.Trim(),
                    OrderRegNumber = model.OrderRegNumber,
                    StartSerial = model.StartSerial?.Trim(),
                    EndSerial = model.EndSerial?.Trim(),
                    CodeProduct = model.CodeProduct?.Trim(),
                    RegDate = DateTime.Parse(model.RegDate),
                    RegisterTime = DateTime.Now,
                    OperatorId = GetCurrentUserId(),
                    CustomerName = model.CustomerName?.Trim()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var package = new Package
                {
                    OrderId = order.OrderId,
                    MeterTypeId = model.Packages.MeterTypeId > 0 ? model.Packages.MeterTypeId : null,
                    CoverTypeId = model.Packages.CoverTypeId > 0 ? model.Packages.CoverTypeId : null,
                    PackageTypeId = model.Packages.PackageTypeId > 0 ? model.Packages.PackageTypeId : null,
                    MeterBaseId = model.Packages.MeterBaseId > 0 ? model.Packages.MeterBaseId : null,
                    ModuleTypeId = model.Packages.ModuleTypeId > 0 ? model.Packages.ModuleTypeId : null,
                    BoardTypeId = model.Packages.BoardTypeId > 0 ? model.Packages.BoardTypeId : null,
                    AccessoriesTypeId = model.Packages.AccessoriesTypeId > 0 ? model.Packages.AccessoriesTypeId : null,
                    RsportTypeId = model.Packages.RsPortTypeId > 0 ? model.Packages.RsPortTypeId : null,
                    StartSerial = model.StartSerial?.Trim(),
                    EndSerial = order.EndSerial?.Trim(),
                    MeterCount = CalculateMeterCount(order.StartSerial, order.EndSerial),
                    PackageCount = model.Packages.PackageCount ?? 1,
                    FrimwareVersion = model.Packages.FrimWareVersion?.Trim(),
                    CheckSum = model.Packages.CheckSum?.Trim(),
                    ProfileName = model.Packages.ProfileName?.Trim(),
                    ShowInBarcodeScan = model.Packages.ShowInBarcodeScan,
                    RegisterTime = DateTime.Now,
                    OperatorId = GetCurrentUserId()
                };

                _context.Packages.Add(package);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "سفارش با موفقیت ثبت شد.";
                TempData.Remove("Success");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "خطا در ثبت سفارش: " + ex.Message;
                await LoadDropdownsAsync();
                ViewBag.RegDatePersian = RegDatePersian;
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Packages)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            var model = new OrderViewModel
            {
                Id = order.OrderId,
                CityId = order.CityId,
                OrderNumber = order.OrderNumber,
                OrderRegNumber = order.OrderRegNumber,
                StartSerial = order.StartSerial,
                EndSerial = order.EndSerial,
                CodeProduct = order.CodeProduct,
                RegDate =_services.iGregorianToPersian(order.RegDate),
                CustomerName = order.CustomerName,

                Packages = order.Packages != null && order.Packages.Any() ? new PackageViewModel
                {
                    Id = order.Packages.First().PackagingId,
                    MeterTypeId = order.Packages.First().MeterTypeId ?? 0,
                    CoverTypeId = order.Packages.First().CoverTypeId ?? 0,
                    PackageTypeId = order.Packages.First().PackageTypeId ?? 0,
                    MeterBaseId = order.Packages.First().MeterBaseId ?? 0,
                    ModuleTypeId = order.Packages.First().ModuleTypeId ?? 0,
                    BoardTypeId = order.Packages.First().BoardTypeId ?? 0,
                    AccessoriesTypeId = order.Packages.First().AccessoriesTypeId ?? 0,
                    RsPortTypeId = order.Packages.First().RsportTypeId ?? 0,
                    StartSerial = order.Packages.First().StartSerial,
                    EndSerial = order.Packages.First().EndSerial,
                    FrimWareVersion = order.Packages.First().FrimwareVersion,
                    CheckSum = order.Packages.First().CheckSum,
                    ProfileName = order.Packages.First().ProfileName,
                    ShowInBarcodeScan = order.Packages.First().ShowInBarcodeScan ?? true,
                }
                : new PackageViewModel()
            };
            string conflictMessage = "";
            var (hasConflict, conflictCount) = await IsAnySerialInRangeExistsMeter(model.StartSerial.Trim(), model.EndSerial.Trim());
            if (hasConflict) conflictMessage += $"تعداد {conflictCount} سفارش از این بازه قبلاً ثبت شده است.\n";
            var (hasDeviceConflict, deviceCount) = await IsAnySerialInRangeExistMeterDevice(model.StartSerial.Trim(), model.EndSerial.Trim());
            if (hasDeviceConflict) conflictMessage += $"تعداد {deviceCount} سریال در تست کنتور ثبت شده است.\n";
           
            if (conflictMessage.Length>0)
                ViewBag.disable= true;
            else
            ViewBag.disable = false;
            // تبدیل تاریخ میلادی به شمسی برای نمایش
            if (order.RegDate.HasValue)
            {
                ViewBag.RegDatePersian = order.RegDate.Value.ToPersianDateTime().ToString("yyyy/MM/dd");
            }
            else
            {
                ViewBag.RegDatePersian = DateTime.Now.ToPersianDateTime().ToString("yyyy/MM/dd");
            }

            await LoadDropdownsAsync();
            ViewData["Title"] = "ویرایش سفارش بسته‌بندی";
            return View("Edit", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderViewModel model, string RegDatePersian)
        {
            var order = await _context.Orders
               .Include(o => o.Packages)
               .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

           

            model.OrderRegNumber = model.OrderNumber?.Trim();
            model.Packages ??= new PackageViewModel();

            // ==============================================================
            // اعتبارسنجی فیلدهای الزامی
            // ==============================================================

           

            // چک شماره سفارش - فقط اگر متفاوت باشد
            if (!order.OrderNumber.Equals(model.OrderNumber?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                string trimmedOrderNumber = model.OrderNumber?.Trim();
                if (!string.IsNullOrEmpty(trimmedOrderNumber) && _context.Orders.Any(x => x.OrderNumber == trimmedOrderNumber))
                    ModelState.AddModelError("OrderNumber", "شماره سفارش قبلاً ثبت شده است.");
            }

            // چک کد محصول - فقط اگر متفاوت باشد
            if (!order.CodeProduct.Equals(model.CodeProduct?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                string trimmedCodeProduct = model.CodeProduct?.Trim();
                if (!string.IsNullOrEmpty(trimmedCodeProduct) && _context.Orders.Any(x => x.CodeProduct == trimmedCodeProduct))
                    ModelState.AddModelError("CodeProduct", "کد محصول قبلاً برای سفارش دیگری ثبت شده است.");
            }

            // چک فرمت و ترتیب سریال‌ها
            if (!string.IsNullOrWhiteSpace(model.StartSerial) && !string.IsNullOrWhiteSpace(model.EndSerial))
            {
                if (!CheckSerialFormat(model.StartSerial.Trim()))
                    ModelState.AddModelError("StartSerial", "فرمت سریال اولیه صحیح نیست.");
                else if (!CheckSerialFormat(model.EndSerial.Trim()))
                    ModelState.AddModelError("EndSerial", "فرمت سریال انتهایی صحیح نیست.");
                else if (long.TryParse(model.StartSerial.Trim(), out long start) &&
                         long.TryParse(model.EndSerial.Trim(), out long end))
                {
                    if (start > end)
                        ModelState.AddModelError("StartSerial", "سریال اولیه باید کوچک‌تر یا مساوی سریال انتهایی باشد.");
                    else
                    {
                        // چک تداخل فقط اگر سریال‌ها تغییر کردند
                        bool serialChanged = !order.StartSerial.Equals(model.StartSerial?.Trim()) || 
                                           !order.EndSerial.Equals(model.EndSerial?.Trim());

                        if (serialChanged)
                        {
                            string conflictMessage = "";
                            var (hasConflict, conflictCount) = await IsAnySerialInRangeExistsMeter(model.StartSerial.Trim(), model.EndSerial.Trim());
                            if (hasConflict) conflictMessage += $"تعداد {conflictCount} سفارش از این بازه قبلاً ثبت شده است.\n";

                            int orderIdConflict = await GetConflictingOrderIdAsync(model.StartSerial.Trim(), model.EndSerial.Trim());
                            if (orderIdConflict > 0 && orderIdConflict != id) 
                                conflictMessage += $"تداخل با سفارش شماره {orderIdConflict}\n";

                            int packageIdConflict = await GetConflictingPackageIdAsync(model.StartSerial.Trim(), model.EndSerial.Trim());
                            if (packageIdConflict > 0) 
                                conflictMessage += $"تداخل با پکیج شماره {packageIdConflict}\n";

                            var (hasDeviceConflict, deviceCount) = await IsAnySerialInRangeExistMeterDevice(model.StartSerial.Trim(), model.EndSerial.Trim());
                            if (hasDeviceConflict) 
                                conflictMessage += $"تعداد {deviceCount} سریال در تست کنتور ثبت شده است.\n";

                            if (!string.IsNullOrEmpty(conflictMessage))
                                ModelState.AddModelError("StartSerial", conflictMessage.TrimEnd('\n'));
                        }
                    }
                }
            }

            // پاک کردن فیلدهای نمایشی
            ModelState.Remove("CityName");
            //ModelState.Remove("RegDatePersian");
            ModelState.Remove("OrderRegNumber");
            ModelState.Remove("StartSerial");
            ModelState.Remove("EndSerial");
            ModelState.Remove("CustomerName");
            ModelState.Remove("Id");

            if (model.Packages != null)
            {
                var displayFields = new[] { "MeterTypeName", "MeterBaseName", "CoverTypeName", "PackageTypeName",
                    "ModuleTypeName", "BoardTypeName", "AccessoriesTypeName", "RsPortTypeName",
                    "MeterCount", "ProfileName", "CheckSum", "FrimWareVersion", "ShowInBarcodeScan", "StartSerial", "EndSerial" };

                foreach (var field in displayFields)
                    ModelState.Remove($"Packages.{field}");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors.Select(e => new
                    {
                        Field = x.Key,
                        Message = e.ErrorMessage
                    }))
                    .Where(err =>
                        err.Message.Contains("تداخل") ||
                        err.Message.Contains("قبلاً ثبت شده") ||
                        err.Message.Contains("فرمت سریال") ||
                        err.Message.Contains("کوچک‌تر") ||
                        err.Message.Contains("الزامی است") ||
                        err.Message.Contains("انتخاب کنید") ||
                        err.Message.Contains("لطفاً"))
                    .ToList();

                if (errors.Any())
                {
                    ViewBag.ModelStateErrors = errors;
                }

                await LoadDropdownsAsync();
                ViewBag.RegDatePersian = RegDatePersian;
                return View(model);
            }

            // ==============================================================
            // ذخیره در دیتابیس - به‌روزرسانی رکورد‌های موجود
            // ==============================================================

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // به‌روزرسانی Order موجود
                order.CityId = model.CityId.Value;
                order.OrderNumber = model.OrderNumber?.Trim();
                order.OrderRegNumber = model.OrderRegNumber;
                order.StartSerial = model.StartSerial?.Trim();
                order.EndSerial = model.EndSerial?.Trim();
                order.CodeProduct = model.CodeProduct?.Trim();
                order.RegDate = DateTime.Parse(model.RegDate);
                order.CustomerName = model.CustomerName?.Trim();

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                // به‌روزرسانی Package موجود
                var package = order.Packages.FirstOrDefault();
                if (package != null)
                {
                    package.MeterTypeId = model.Packages.MeterTypeId > 0 ? model.Packages.MeterTypeId : null;
                    package.CoverTypeId = model.Packages.CoverTypeId > 0 ? model.Packages.CoverTypeId : null;
                    package.PackageTypeId = model.Packages.PackageTypeId > 0 ? model.Packages.PackageTypeId : null;
                    package.MeterBaseId = model.Packages.MeterBaseId > 0 ? model.Packages.MeterBaseId : null;
                    package.ModuleTypeId = model.Packages.ModuleTypeId > 0 ? model.Packages.ModuleTypeId : null;
                    package.BoardTypeId = model.Packages.BoardTypeId > 0 ? model.Packages.BoardTypeId : null;
                    package.AccessoriesTypeId = model.Packages.AccessoriesTypeId > 0 ? model.Packages.AccessoriesTypeId : null;
                    package.RsportTypeId = model.Packages.RsPortTypeId > 0 ? model.Packages.RsPortTypeId : null;
                    package.StartSerial = order.StartSerial?.Trim();
                    package.EndSerial = order.EndSerial?.Trim();
                    package.MeterCount = CalculateMeterCount(order.StartSerial, order.EndSerial);
                    package.PackageCount = model.Packages.PackageCount ?? 1;
                    package.FrimwareVersion = model.Packages.FrimWareVersion?.Trim();
                    package.CheckSum = model.Packages.CheckSum?.Trim();
                    package.ProfileName = model.Packages.ProfileName?.Trim();
                    package.ShowInBarcodeScan = model.Packages.ShowInBarcodeScan;

                    _context.Packages.Update(package);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "سفارش با موفقیت ویرایش شد.";
                TempData.Remove("Success");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "خطا در ویرایش سفارش: " + ex.Message;
                await LoadDropdownsAsync();
                ViewBag.RegDatePersian = RegDatePersian;
                return View(model);
            }



            
        }
      
        // بررسی تداخل سریال‌ها در بازه
        private async Task<(bool HasConflict, int ConflictCount)> IsAnySerialInRangeExistsMeter(string startSerial, string endSerial)
        {
            await using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "dbo.CheckSerialRangeExistsMeter";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@StartSerial", startSerial));
            cmd.Parameters.Add(new SqlParameter("@EndSerial", endSerial));

            var paramExists = new SqlParameter("@Exists", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var paramCount = new SqlParameter("@ConflictCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(paramExists);
            cmd.Parameters.Add(paramCount);

            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            bool hasConflict = paramExists.Value != DBNull.Value && (bool)paramExists.Value;
            int conflictCount = paramCount.Value != DBNull.Value ? (int)paramCount.Value : 0;

            return (hasConflict, conflictCount);
        }


        private async Task<int> GetConflictingOrderIdAsync(string startSerialStr, string endSerialStr)
        {
            if (!long.TryParse(startSerialStr, out long startSerial) ||
                !long.TryParse(endSerialStr, out long endSerial))
            {
                throw new ArgumentException("شماره سریال‌ها باید عددی معتبر باشند.");
            }

            await using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "dbo.CheckSerialRangeExistsOrders";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // پارامترهای ورودی
            cmd.Parameters.Add(new SqlParameter("@StartSerial", SqlDbType.BigInt) { Value = startSerial });
            cmd.Parameters.Add(new SqlParameter("@EndSerial", SqlDbType.BigInt) { Value = endSerial });

            // پارامتر خروجی
            var paramConflictingId = new SqlParameter("@ConflictingOrderId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(paramConflictingId);

            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            // خروجی: اگر تداخل باشد عدد OrderId، اگر نه 0
            int conflictingOrderId = paramConflictingId.Value == DBNull.Value ? 0 : (int)paramConflictingId.Value;

            return conflictingOrderId;
        }

        private async Task<int> GetConflictingPackageIdAsync(string startSerialStr, string endSerialStr)
        {
            if (!long.TryParse(startSerialStr, out long startSerial) ||
                !long.TryParse(endSerialStr, out long endSerial))
            {
                throw new ArgumentException("شماره سریال‌ها باید عددی معتبر باشند.");
            }

            await using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "dbo.CheckSerialRangeExistsPackages";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // پارامترهای ورودی
            cmd.Parameters.Add(new SqlParameter("@StartSerial", SqlDbType.BigInt) { Value = startSerial });
            cmd.Parameters.Add(new SqlParameter("@EndSerial", SqlDbType.BigInt) { Value = endSerial });

            // پارامتر خروجی
            var paramConflictingId = new SqlParameter("@ConflictingOrderId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(paramConflictingId);

            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            // خروجی: اگر تداخل باشد عدد OrderId، اگر نه 0
            int conflictingOrderId = paramConflictingId.Value == DBNull.Value ? 0 : (int)paramConflictingId.Value;

            return conflictingOrderId;
        }
        // بررسی تداخل سریال‌ها در بازه
        private async Task<(bool HasConflict, int ConflictCount)> IsAnySerialInRangeExistMeterDevice(string startSerial, string endSerial)
        {
            await using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "dbo.CheckSerialRangeExistsMeterDevices";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            startSerial = startSerial.Substring(startSerial.Length - 8);
            endSerial = endSerial.Substring(endSerial.Length - 8);
            cmd.Parameters.Add(new SqlParameter("@StartSerial", startSerial));
            cmd.Parameters.Add(new SqlParameter("@EndSerial", endSerial));

            var paramExists = new SqlParameter("@Exists", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var paramCount = new SqlParameter("@ConflictCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(paramExists);
            cmd.Parameters.Add(paramCount);

            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            bool hasConflict = paramExists.Value != DBNull.Value && (bool)paramExists.Value;
            int conflictCount = paramCount.Value != DBNull.Value ? (int)paramCount.Value : 0;

            return (hasConflict, conflictCount);
        }

        
        // متدهای کمکی
        private async Task LoadDropdownsAsync()
        {
            ViewBag.Cities = await _context.Cities
                .Select(c => new SelectListItem { Value = c.CityId.ToString(), Text = c.CityName ?? "نامشخص" })
                .OrderBy(x => x.Text)
                .ToListAsync();

            await LoadDropdownsForPackageAsync();
        }

        private async Task LoadDropdownsForPackageAsync()
        {
            ViewBag.MeterTypes = GetAppTypes(1);
            ViewBag.MeterBaseTypes = GetAppTypes(2);
            ViewBag.PackageTypes = GetAppTypes(3);
            ViewBag.CoverTypes = GetAppTypes(5);
            ViewBag.ModuleTypes = GetAppTypes(9);
            ViewBag.BoardTypes = GetAppTypes(10);
            ViewBag.AccessoriesTypes = GetAppTypes(6);
            ViewBag.RsPortTypes = GetAppTypes(7);
        }

        private List<SelectListItem> GetAppTypes(int classId)
        {
            return _context.AppTypes
                .Where(t => t.TypeClassId == classId)
                .Select(t => new SelectListItem { Value = t.TypeId.ToString(), Text = t.TypeName ?? "نامشخص" })
                .ToList();
        }

        private int? CalculateMeterCount(string startSerial, string endSerial)
        {
            if (string.IsNullOrWhiteSpace(startSerial) || string.IsNullOrWhiteSpace(endSerial))
                return null;

            if (long.TryParse(startSerial, out long start) && long.TryParse(endSerial, out long end))
                return start <= end ? (int?)(end - start + 1) : null;

            return null;
        }

        public async Task<IActionResult> Orders_Read([DataSourceRequest] DataSourceRequest request, int? cityId, string search)
        {
            var ordersQuery = _context.Orders.Include(o => o.City).AsQueryable();

            if (cityId.HasValue && cityId > 0)
                ordersQuery = ordersQuery.Where(o => o.CityId == cityId);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    (o.CustomerName != null && o.CustomerName.Contains(search)) ||
                    (o.City != null && o.City.CityName.Contains(search)));
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderId)
                .Select(o => new OrderViewModel
                {
                    Id = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    CodeProduct = o.CodeProduct,
                    CityName = o.City != null ? o.City.CityName ?? "نامشخص" : "نامشخص",
                    StartSerial = o.StartSerial ?? "-",
                    EndSerial = o.EndSerial ?? "-",
                    SerialCount = CalculateSerialCount(o.StartSerial, o.EndSerial),
                    RegDate =_services.iGregorianToPersian(o.RegDate)
                })
                .ToListAsync();

            

            return Json(orders.ToDataSourceResult(request));
        }


        private static long CalculateSerialCount(string start, string end)
        {
            if (long.TryParse(start, out long s) && long.TryParse(end, out long e) && s <= e)
                return e - s + 1;
            return 0;
        }

        public DateTime ConvertPersianDate(string persianDateString, bool useCurrentTime = true, TimeSpan? specificTime = null)
        {
            if (string.IsNullOrWhiteSpace(persianDateString))
                return DateTime.Now; // اگر خالی باشد، تاریخ امروز برگردان

            try
            {
                // جدا کردن سال/ماه/روز (فرمت yyyy/MM/dd یا 1404/10/16)
                var parts = persianDateString.Trim().Split('/');
                if (parts.Length != 3)
                    throw new FormatException("فرمت تاریخ شمسی اشتباه است. باید yyyy/MM/dd باشد.");

                int year = int.Parse(parts[0].Trim());
                int month = int.Parse(parts[1].Trim());
                int day = int.Parse(parts[2].Trim());

                PersianCalendar pc = new PersianCalendar();

                // ساعت را تنظیم کن
                int hour = 0, minute = 0, second = 0;
                if (useCurrentTime)
                {
                    DateTime now = DateTime.Now;
                    hour = now.Hour;
                    minute = now.Minute;
                    second = now.Second;
                }
                else if (specificTime.HasValue)
                {
                    hour = specificTime.Value.Hours;
                    minute = specificTime.Value.Minutes;
                    second = specificTime.Value.Seconds;
                }

                return pc.ToDateTime(year, month, day, hour, minute, second, 0);
            }
            catch (Exception ex)
            {
                // اگر خطای تاریخ بود، تاریخ فعلی برگردان
                return DateTime.Now;
            }
        }
        private string ConvertToPersianDateFull(DateTime? date)
        {
            if (!date.HasValue)
                return string.Empty; // یا "-" یا "نامشخص"

            PersianCalendar pc = new PersianCalendar();
            int year = pc.GetYear(date.Value);
            int month = pc.GetMonth(date.Value);
            int day = pc.GetDayOfMonth(date.Value);

            return $"{year:0000}/{month:00}/{day:00}";
        }
        public DateTime ShamsiStringToMiladi(string shamsiDate)
        {
            // جدا کردن سال، ماه، روز (فرمت yyyy/MM/dd یا yyyy/M/d)
            string[] parts = shamsiDate.Replace("۰", "0").Replace("۱", "1") // تبدیل ارقام فارسی به انگلیسی اگر لازم بود
                                       .Replace("۲", "2").Replace("۳", "3").Replace("۴", "4")
                                       .Replace("۵", "5").Replace("۶", "6").Replace("۷", "7")
                                       .Replace("۸", "8").Replace("۹", "9")
                                       .Split('/', '-');

            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            PersianCalendar pc = new PersianCalendar();
            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

    }
}