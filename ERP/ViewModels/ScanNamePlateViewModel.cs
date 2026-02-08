namespace ERP.ViewModels
{
    public class ScanNamePlateViewModel
    {
        public int OrderID { get; set; }
        public string CodeProduct { get; set; }
        public string OrderNumber { get; set; }
        public int PackageId { get; set; }
        public string StartSerial { get; set; }
        public string EndSerial { get; set; }
        public string CityName { get; set; }
        public long TotalCount => (long.Parse(EndSerial) - long.Parse(StartSerial) + 1);
        public int ScannedCount { get; set; }
        public List<MeterSerialInfo> ScannedMeters { get; set; } = new List<MeterSerialInfo>();
    }

    public class MeterSerialInfo
    {
        public string Serial { get; set; }
        public string DateInsert { get; set; } // فرمت شمسی یا میلادی
        public string FullName { get; set; }
    }
}
