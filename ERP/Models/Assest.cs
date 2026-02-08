using ERP.ModelsEMP;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ERP.Models.asset
{
    //اطلاعات کاربر
    public class AssestUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }

        public AssestUserTypes AssestUserTypes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } =DateTime.Now;
        public ICollection<AssetHistory> AssetHistories { get; set; }
    }
    //دسته‌بندی اموال (مثلاً لپ‌تاپ، پرینتر، موبایل)
    public class AssestCategory
    {
        public int Id { get; set; }
        public string Title { get; set; }        
        public int? ParentId { get; set; }
        public bool IsActive { get; set; }
        public AssestCategory Parent { get; set; }
        public ICollection<AssestCategory> Children { get; set; }
    }
    //(اموال)
    public class Asset
    {
        public int Id { get; set; }
        public string AssetCode { get; set; }    
        public string AssetName { get; set; }      
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public AssestCategory Category { get; set; }
        public ICollection<AssetProperty> Properties { get; set; }
    }
    // جزئی هر اموال مشخصات
    public class AssetProperty
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int CategoryId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? LastOwnerUserId { get; set; }
        public DateTime? LastAssignedDate { get; set; }
        // Navigation
        public Asset Asset { get; set; }
        public AssestCategory Category { get; set; }
        public AssestUser LastOwnerUser { get; set; }
    }
   // (گردش / تحویل اموال و قطعات)
    public class AssetHistory
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int? AssetPropertyId { get; set; } // اگر فقط قطعه جابجا شد
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public DateTime AssignDate { get; set; }
        public string Description { get; set; }
        public Asset Asset { get; set; }
        public AssetProperty AssetProperty { get; set; }
        public AssestUser AssestUser { get; set; }
        public AssestUser AssestToUser { get; set; }
    }
     public enum AssestUserTypes
    {
        User = 0,
        vahed = 1,
      
    }
}
