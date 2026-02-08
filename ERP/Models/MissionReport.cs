
namespace ERP.Models
{
    public class MissionReport
    {
        public int MissionReportID { get; set; } // Primary key
        public string S_persiandate { get; set; }        // تاریخ درخواست
        public string Number_Hokm { get; set; }        // شماره حکم
        public string? AzTaraf { get; set; }        // از طرف
        public string DropdownVar002 { get; set; }        // ماموریت (داخلی یا خارجی)
        public string DropdownVar001 { get; set; }        // ماهیت ماموریت (فعالیت اجرایی، بازدید، تلفیقی، آموزشی، سایر)
        public string Req_name { get; set; }
        public string Req_UserID { get; set; }
        public string Number_Personeli { get; set; }
        public string DropdownVar003 { get; set; }        // واحد سازمانی
        public string Mamuriat_Sazman { get; set; }        // ماموریت به اداره/سازمان/شرکت
        public string Shahr_Mamuriat { get; set; } // شهر ماموریت
        public string Modat_mamuriat { get; set; }       // مدت
        public string Noe_Mamuriat { get; set; }        // نوع ماموریت (روز یا ساعت)
        public string Start_persiandate_var { get; set; }   // تاریخ شروع
        public string? Start_Hour { get; set; }// ساعت شروع
        public string End_persiandate_var { get; set; } // تاریخ اتمام
        public string? End_Hour { get; set; } // ساعت اتمام
        public string Janeshin { get; set; }
        public string? Sharh_Mamuriat { get; set; }
        public string? Radio_Tayid1 { get; set; }
        public string? Tozihat_Tayid1 { get; set; }
        public string? Rahgiri { get; set; }
        public string? TozihatGozaresh { get; set; }
        public string? Radio_Tayid2 { get; set; }
        public string? Tozihat_Tayid2 { get; set; }
        public string? TozihatSarparastedari { get; set; }
        public string? Location { get; set; }
        public string? Desclocation { get; set; }
        public string? Start_persiandate_var2 { get; set; }
        public string? Start_Hour2 { get; set; }
        public string? End_persiandate_var3 { get; set; }
        public string? End_Hour2 { get; set; }
        public string? TozihatKargozini { get; set; }
        public List<DispatchPersonnel> DispatchedPersonnel { get; set; } = new List<DispatchPersonnel>();
        public List<MetPersonnel> MetPersonnel { get; set; } = new List<MetPersonnel>();
        public List<MissionExpense> MissionExpenses { get; set; } = new List<MissionExpense>();
        public string State { get; set; } // Workflow state
        public string Guid { get; set; } // Unique identifier for the form instance
        public string ManagerUserID { get; set; } // Manager selection
    }
    public class DispatchPersonnel
    {
        public int Id { get; set; }
        public int MissionReportID { get; set; }
        // نام و نام خانوادگی
        public string FullName { get; set; }
    }

    public class MetPersonnel
    {
        public int Id { get; set; }
        public int MissionReportID { get; set; }
        // نام و نام خانوادگی
        public string FullName { get; set; }
    }

    public class MissionExpense
    {
        public int Id { get; set; }
        public int MissionReportID { get; set; }
        // بابت
        public string Description { get; set; }
        // مبلغ / ریال
        public int Amount { get; set; }
    }
}