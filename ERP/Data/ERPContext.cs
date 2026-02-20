using ERP.Models;
using ERP.Models.asset;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection.Emit;
using System.Text.Json;


namespace ERP.Data
{
    public class ERPContext : IdentityDbContext<Users, Roles, string>
    {



        public ERPContext(DbContextOptions<ERPContext> options) : base(options)
        {
            Database.EnsureCreated();
        }


        public DbSet<FileDB> FileDBs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<WorkReport> WorkReports { get; set; }

        public DbSet<MeetingDecision> MeetingDecisions { get; set; }
        public DbSet<MeetingAccept> MeetingAccepts { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<ViewProcess> ViewProcesss { get; set; }

        public DbSet<GuestEntry> GuestEntries { get; set; }

        public DbSet<MissionReport> MissionReports { get; set; }
        public DbSet<WorkflowAccess> WorkflowAccesses { get; set; }
        public DbSet<WorkflowSection> WorkflowSections { get; set; }
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public DbSet<WorkflowStep> WorkflowSteps { get; set; }

        public DbSet<GoodsDeparture> GoodsDepartures { get; set; }
        public DbSet<GoodsConsigned> GoodsConsigneds { get; set; }
        public DbSet<GoodsEntry> GoodsEntries { get; set; }
        public DbSet<GoodsEntryItem> GoodsEntryItems { get; set; }
        public DbSet<ProductCheck> ProductChecks { get; set; } = null!;
        public DbSet<OutsourcingProduction> OutsourcingProductions { get; set; }
        public DbSet<OutsourcingItem> OutsourcingItems { get; set; }

        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }

        public DbSet<GoodsEntryItemGuard> GoodsEntryItemGuards { get; set; }

        public DbSet<GoodsConsignedItemGuard> GoodsConsignedItemGuards { get; set; }

        public DbSet<CompanyInquery> CompanyInquerys { get; set; }
        public DbSet<CompanyTender> CompanyTenders { get; set; }
        public DbSet<CoTenderItem> CoTenderItems { get; set; }
        public DbSet<CoInqueryItem> CoInqueryItems { get; set; }
        public DbSet<GuaranteeLetter> GuaranteeLetters { get; set; }
        public DbSet<SMT> smt { get; set; }
        public DbSet<SMTSecondary> smtSecondary { get; set; }

        public DbSet<AssestUser> AssestUsers { get; set; }
        public DbSet<AssestCategory> AssestCategories { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetProperty> AssetProperties { get; set; }
        public DbSet<AssetHistory> AssetHistories { get; set; }


        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }

        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }

        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChannelMember> ChannelMembers { get; set; }
        public DbSet<ChannelMessage> ChannelMessages { get; set; }

        public DbSet<ChatAccess> ChatAccesses { get; set; }
        public DbSet<GuestUser> GuestUsers { get; set; }
        public DbSet<GuestVerificationCode> GuestVerificationCodes { get; set; }
        public DbSet<GuestChatAccess> GuestChatAccesses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MeetingDecision>()
                .HasOne(d => d.Meeting)
                .WithMany(m => m.MeetingDecisions)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.Restrict);

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };

            builder.Entity<GuestEntry>()
                .Property(e => e.Guests)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<GuestInfo>>(v, jsonOptions) ?? new List<GuestInfo>(),
                    new ValueComparer<List<GuestInfo>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
            builder.Entity<ProductCheck>(entity =>
            {
                entity.HasIndex(e => e.Serial_Product)
                      .HasDatabaseName("IX_ProductChecks_Serial");
            });
            builder.Entity<GuestEntry>()
                .Property(e => e.Companions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>(),
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            // MissionReport JSON configurations
            builder.Entity<MissionReport>()
                .Property(e => e.DispatchedPersonnel)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<DispatchPersonnel>>(v, jsonOptions) ??
                         new List<DispatchPersonnel>(),
                    new ValueComparer<List<DispatchPersonnel>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            builder.Entity<MissionReport>()
                .Property(e => e.MetPersonnel)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<MetPersonnel>>(v, jsonOptions) ?? new List<MetPersonnel>(),
                    new ValueComparer<List<MetPersonnel>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            builder.Entity<MissionReport>()
                .Property(e => e.MissionExpenses)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<MissionExpense>>(v, jsonOptions) ?? new List<MissionExpense>(),
                    new ValueComparer<List<MissionExpense>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            builder.Entity<GoodsDeparture>()
                .Property(e => e.VehicleExit)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<VehicleExit>>(v, jsonOptions) ?? new List<VehicleExit>(),
                    new ValueComparer<List<Models.VehicleExit>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            builder.Entity<GoodsConsigned>()
                .Property(e => e.ConsignedItem)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<ConsignedItem>>(v, jsonOptions) ?? new List<ConsignedItem>(),
                    new ValueComparer<List<Models.ConsignedItem>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            builder.Entity<UserGroup>().HasKey(ug => new { ug.UserID, ug.GroupID });

            // ---------------- AssestUser ----------------
            builder.Entity<AssestUser>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                     .IsRequired()
                     .HasMaxLength(200); 

                entity.Property(e => e.Family)
                      .IsRequired()
                      .HasMaxLength(200);

                // نوع کاربر enum ذخیره در int
                entity.Property(e => e.AssestUserTypes)
                      .HasConversion<int>()
                      .IsRequired();

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");
            });

            // ---------------- AssestCategory ----------------
            builder.Entity<AssestCategory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.HasOne(e => e.Parent)
                      .WithMany(e => e.Children)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- Asset ----------------
            builder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AssetCode)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasIndex(e => e.AssetCode)
                      .IsUnique();

                entity.Property(e => e.AssetName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Properties)
                      .WithOne(p => p.Asset)
                      .HasForeignKey(p => p.AssetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------- AssetProperty ----------------
            builder.Entity<AssetProperty>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PropertyName).HasMaxLength(100);
                entity.Property(e => e.PropertyValue).HasMaxLength(200);
                entity.Property(e => e.SerialNumber).HasMaxLength(100);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.Brand).HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Asset)
                      .WithMany(a => a.Properties)
                      .HasForeignKey(e => e.AssetId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastOwnerUser)
                      .WithMany()
                      .HasForeignKey(e => e.LastOwnerUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- AssetHistory ----------------
            builder.Entity<AssetHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AssignDate)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Description)
                      .HasMaxLength(300);

                entity.HasOne(e => e.Asset)
                      .WithMany()
                      .HasForeignKey(e => e.AssetId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AssetProperty)
                      .WithMany()
                      .HasForeignKey(e => e.AssetPropertyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssestUser)
                      .WithMany()
                      .HasForeignKey(e => e.FromUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssestToUser)
                      .WithMany()
                      .HasForeignKey(e => e.ToUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- ChatMessage ----------------
            builder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);

             
            });

            // ---------------- ChatAccess ----------------
            builder.Entity<ChatAccess>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.AllowedUser)
                      .WithMany()
                      .HasForeignKey(e => e.AllowedUserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => new { e.UserId, e.AllowedUserId }).IsUnique();
            });

            // ---------------- GuestUser ----------------
            builder.Entity<GuestUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PhoneNumber);
                entity.HasIndex(e => e.UniqueToken).IsUnique();
            });

            // ---------------- GuestVerificationCode ----------------
            builder.Entity<GuestVerificationCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PhoneNumber);
            });

            // ---------------- GuestChatAccess ----------------
            builder.Entity<GuestChatAccess>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.GuestUser)
                      .WithMany()
                      .HasForeignKey(e => e.GuestUserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AllowedUser)
                      .WithMany()
                      .HasForeignKey(e => e.AllowedUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
