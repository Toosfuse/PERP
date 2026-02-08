using ERP.Models.asset;
using System;

namespace ERP.ViewModels.Assest
{
    public class AssestUserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public AssestUserTypes AssestUserTypes { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; } 
    }
    public class AssestUserPageViewModel
    {
        public List<AssestUserViewModel> Users { get; set; } = new List<AssestUserViewModel>();
    }
}
