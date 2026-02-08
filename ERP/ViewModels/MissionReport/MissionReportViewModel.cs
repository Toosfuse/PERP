using ERP.Models;

namespace ERP.ViewModels.MissionReport
{
    public class SaveForm1Dto
    {
        public int MissionReportID { get; set; }
        public string S_persiandate { get; set; }
        public string Number_Hokm { get; set; }
        public string AzTaraf { get; set; }
        public string DropdownVar002 { get; set; }
        public string DropdownVar001 { get; set; }
        public string Req_name { get; set; }
        public string Number_Personeli { get; set; }
        public string DropdownVar003 { get; set; }
        public string Mamuriat_Sazman { get; set; }
        public string Shahr_Mamuriat { get; set; }
        public string Modat_mamuriat { get; set; }
        public string Noe_Mamuriat { get; set; }
        public string Start_persiandate_var { get; set; }
        public string? Start_Hour { get; set; }
        public string End_persiandate_var { get; set; }
        public string? End_Hour { get; set; }
        public string Janeshin { get; set; }
        public string Sharh_Mamuriat { get; set; }
        public string Guid { get; set; }
        //public string ManagerUserID { get; set; }
    }

    public class SaveForm2Dto
    {
        public int MissionReportID { get; set; }
        public string Radio_Tayid1 { get; set; }
        public string Tozihat_Tayid1 { get; set; }
        public string Guid { get; set; }
    }

    public class SaveForm3Dto
    {
        public int MissionReportID { get; set; }
        public string Rahgiri { get; set; }
        public string TozihatGozaresh { get; set; }
        public List<DispatchPersonnel> DispatchedPersonnel { get; set; }
        public List<MetPersonnel> MetPersonnel { get; set; }
        public List<MissionExpense> MissionExpenses { get; set; }
        public string Guid { get; set; }
    }

    public class SaveForm4Dto
    {
        public int MissionReportID { get; set; }
        public string Radio_Tayid2 { get; set; }
        public string Tozihat_Tayid2 { get; set; }
        public string Guid { get; set; }
    }

    public class SaveForm5Dto
    {
        public int MissionReportID { get; set; }
        public string TozihatSarparastedari { get; set; }
        public string Guid { get; set; }
    }

    public class SaveForm6Dto
    {
        public int MissionReportID { get; set; }
        public string Location { get; set; }
        public string Desclocation { get; set; }
        public string Guid { get; set; }
    }

    public class SaveForm7Dto
    {
        public int MissionReportID { get; set; }
        public string Start_persiandate_var2 { get; set; }
        public string? Start_Hour2 { get; set; }
        public string End_persiandate_var3 { get; set; }
        public string? End_Hour2 { get; set; }
        public string TozihatKargozini { get; set; }
        public string Guid { get; set; }
    }
}

