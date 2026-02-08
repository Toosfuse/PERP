namespace ERP.ViewModels
{
    public class PCBSerialNumberViewModel
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
        public int ScannedBordCount { get; set; }
        public List<PCBSerialNumber_MeterSerialInfo> PCBSerial_MeterSerialInfo { get; set; } = new List<PCBSerialNumber_MeterSerialInfo>();
    }

    public class PCBSerialNumber_MeterSerialInfo
    {
        public string Serial { get; set; }

        public string SeialBord { get; set; }
        public string PCBSerialNumber_Date { get; set; } // فرمت شمسی یا میلادی
        public string FullName { get; set; }
    }
}
