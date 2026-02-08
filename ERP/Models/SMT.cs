using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class SMT
    {
        [Key]
        public long Id {  get; set; }
        public string DataValue {  get; set; }
        public DateTime DateCreate { get; set; }= DateTime.Now;
        public bool IsDelete { get; set; }=false;
    }
}
