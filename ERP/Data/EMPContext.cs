using System;
using System.Collections.Generic;
using ERP.ModelsEMP;
using Microsoft.EntityFrameworkCore;

namespace ERP.Data;

public partial class EMPContext : DbContext
{
    public EMPContext()
    {
    }

    public EMPContext(DbContextOptions<EMPContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivationLog> ActivationLogs { get; set; }

    public virtual DbSet<AppParam> AppParams { get; set; }

    public virtual DbSet<AppType> AppTypes { get; set; }

    public virtual DbSet<AppTypeClass> AppTypeClasses { get; set; }

    public virtual DbSet<Backm1> Backm1s { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<EmpActivationCode> EmpActivationCodes { get; set; }

    public virtual DbSet<FalshMemoryActivatorUser> FalshMemoryActivatorUsers { get; set; }

    public virtual DbSet<KeyManagement> KeyManagements { get; set; }

    public virtual DbSet<Meter> Meters { get; set; }

    public virtual DbSet<MeterDevice> MeterDevices { get; set; }

    public virtual DbSet<MeterDevicesHist> MeterDevicesHists { get; set; }

    public virtual DbSet<MeterRefer> MeterRefers { get; set; }

    public virtual DbSet<MeterReferValue> MeterReferValues { get; set; }

    public virtual DbSet<MetersByCity> MetersByCities { get; set; }

    public virtual DbSet<MetersHist> MetersHists { get; set; }

    public virtual DbSet<Mtfsoftversion> Mtfsoftversions { get; set; }

    public virtual DbSet<OnlineStation> OnlineStations { get; set; }

    public virtual DbSet<OperationsLog> OperationsLogs { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<PackagesLatest> PackagesLatests { get; set; }

    public virtual DbSet<PackagesOld> PackagesOlds { get; set; }

    public virtual DbSet<PackagingKasri> PackagingKasris { get; set; }

    public virtual DbSet<PackgingStation> PackgingStations { get; set; }

    public virtual DbSet<PrintPasswordUser> PrintPasswordUsers { get; set; }

    public virtual DbSet<ProfileLog> ProfileLogs { get; set; }

    public virtual DbSet<ReturnAll> ReturnAlls { get; set; }

    public virtual DbSet<ReturnOpen> ReturnOpens { get; set; }

    public virtual DbSet<ReturnedMajorProblem> ReturnedMajorProblems { get; set; }

    public virtual DbSet<ReturnedMeter> ReturnedMeters { get; set; }

    public virtual DbSet<ReturnedMinorProblem> ReturnedMinorProblems { get; set; }

    public virtual DbSet<ReturnedResponsible> ReturnedResponsibles { get; set; }

    public virtual DbSet<ReturnedStatus> ReturnedStatuses { get; set; }

    public virtual DbSet<Rm> Rms { get; set; }

    public virtual DbSet<StatisticsReturnedMeter> StatisticsReturnedMeters { get; set; }

    public virtual DbSet<TableTest> TableTests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserManagement> UserManagements { get; set; }

    public virtual DbSet<VerifiedProfileByQc> VerifiedProfileByQcs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivationLog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ActivationLog");

            entity.Property(e => e.CityId).HasColumnName("City_id");
            entity.Property(e => e.Date).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Mail).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Tel).HasMaxLength(50);
            entity.Property(e => e.UserCode).HasMaxLength(50);
            entity.Property(e => e.UserNameInsert).HasMaxLength(50);
        });

        modelBuilder.Entity<AppParam>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ParamName).HasMaxLength(50);
            entity.Property(e => e.ParamVal).HasMaxLength(250);
        });

        modelBuilder.Entity<AppType>(entity =>
        {
            entity.HasKey(e => e.TypeId);

            entity.Property(e => e.TypeDetails).HasMaxLength(50);
            entity.Property(e => e.TypeName).HasMaxLength(50);

            entity.HasOne(d => d.TypeClass).WithMany(p => p.AppTypes)
                .HasForeignKey(d => d.TypeClassId)
                .HasConstraintName("FK_AppTypes_AppTypeClasses");
        });

        modelBuilder.Entity<AppTypeClass>(entity =>
        {
            entity.HasKey(e => e.ClassId);

            entity.Property(e => e.ClassName).HasMaxLength(50);
        });

        modelBuilder.Entity<Backm1>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("backm1");

            entity.Property(e => e.DateInsert)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DatePackage)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DatePhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.InsertUserPk).HasColumnName("InsertUser_pk");
            entity.Property(e => e.PackagePk).HasColumnName("Package_pk");
            entity.Property(e => e.PackageUserPk).HasColumnName("PackageUser_pk");
            entity.Property(e => e.PackagingStationDate)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PackagingStation_Date");
            entity.Property(e => e.PackagingStationId).HasColumnName("PackagingStationID");
            entity.Property(e => e.PackagingStationLevel2Date)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PackagingStationLevel2_Date");
            entity.Property(e => e.PackagingStationLevel2UserPk).HasColumnName("PackagingStationLevel2_UserPK");
            entity.Property(e => e.PackagingStationUserPk).HasColumnName("PackagingStation_UserPK");
            entity.Property(e => e.PcbserialNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("PCBSerialNumber");
            entity.Property(e => e.PcbserialNumberDate)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PCBSerialNumber_Date");
            entity.Property(e => e.PcbserialNumberUserPk).HasColumnName("PCBSerialNumber_UserPk");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumberUserPk).HasColumnName("PhoneNumberUser_pk");
            entity.Property(e => e.Serial)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.Property(e => e.CityName).HasMaxLength(50);
            entity.Property(e => e.CityNameEn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CityProfileName).HasMaxLength(50);
        });

        modelBuilder.Entity<EmpActivationCode>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("EmpActivationCode");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<FalshMemoryActivatorUser>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<KeyManagement>(entity =>
        {
            entity.HasKey(e => new { e.CityId, e.MeterTypeId });

            entity.ToTable("KeyManagement");

            entity.Property(e => e.Ak)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("AK");
            entity.Property(e => e.Bk)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BK");
            entity.Property(e => e.Ek)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EK");
            entity.Property(e => e.Masterkey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReadingClientAk)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ReadingClientAK");
            entity.Property(e => e.ReadingClientEk)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ReadingClientEK");
        });

        modelBuilder.Entity<Meter>(entity =>
        {
            entity.HasIndex(e => e.PhoneNumber, "IX_PhoneNumber")
                .IsUnique()
                .HasFilter("([PhoneNumber] IS NOT NULL AND [Id]<>(1683760) AND [Id]<>(3158846) AND [Id]<>(1542201) AND [Id]<>(1268524) AND [Id]<>(1268525) AND [Id]<>(1268526) AND [Id]<>(1268527) AND [Id]<>(1268528) AND [Id]<>(1268529) AND [Id]<>(1268530) AND [Id]<>(1268532) AND [Id]<>(1268533) AND [Id]<>(1550626) AND [Id]<>(1432607) AND [Id]<>(2162916) AND [Id]<>(1859551) AND [Id]<>(1470182) AND [Id]<>(2554731) AND [Id]<>(3482050) AND [Id]<>(3482070) AND [Id]<>(3482048) AND [Id]<>(2341791) AND [Id]<>(3063712) AND [Id]<>(2952500) AND [Id]<>(2952501) AND [Id]<>(2952093) AND [Id]<>(2952097) AND [Id]<>(2952098) AND [Id]<>(3171084) AND [Id]<>(1728519) AND [Id]<>(2167075) AND [Id]<>(1728520) AND [Id]<>(2810099) AND [Id]<>(2810291) AND [Id]<>(2810295) AND [Id]<>(3492718) AND [Id]<>(3492719) AND [Id]<>(3492702) AND [Id]<>(3492689) AND [Id]<>(3492695) AND [Id]<>(1435425) AND [Id]<>(1542203) AND [Id]<>(1539019) AND [Id]<>(1775939) AND [Id]<>(1775940) AND [Id]<>(1951885) AND [Id]<>(3482053) AND [Id]<>(3482052) AND [Id]<>(2952504) AND [Id]<>(2952094) AND [Id]<>(2534845) AND [Id]<>(2534859) AND [Id]<>(2167078) AND [Id]<>(2767215) AND [Id]<>(3192577) AND [Id]<>(3492715) AND [Id]<>(3492709) AND [Id]<>(3492703) AND [Id]<>(3492706) AND [Id]<>(3492690) AND [Id]<>(3492683) AND [Id]<>(3492682) AND [Id]<>(3492669) AND [Id]<>(3492660) AND [Id]<>(3492668) AND [Id]<>(3492673) AND [Id]<>(3492677) AND [Id]<>(3492670) AND [Id]<>(3492649) AND [Id]<>(3492661) AND [Id]<>(3492644) AND [Id]<>(1435424) AND [Id]<>(1435422) AND [Id]<>(2951961) AND [Id]<>(3160539) AND [Id]<>(1542204) AND [Id]<>(1539021) AND [Id]<>(2301145) AND [Id]<>(1542207) AND [Id]<>(3192800) AND [Id]<>(2167077) AND [Id]<>(1951883) AND [Id]<>(1470181) AND [Id]<>(1550625) AND [Id]<>(2556108) AND [Id]<>(3482057) AND [Id]<>(3482047) AND [Id]<>(3482066) AND [Id]<>(3482059) AND [Id]<>(2952499) AND [Id]<>(2952095) AND [Id]<>(2534860) AND [Id]<>(2534861) AND [Id]<>(1921920) AND [Id]<>(3192856) AND [Id]<>(2894164) AND [Id]<>(2894061) AND [Id]<>(3039785) AND [Id]<>(3492710) AND [Id]<>(3492712) AND [Id]<>(3492714) AND [Id]<>(3492701) AND [Id]<>(1539020) AND [Id]<>(1435423) AND [Id]<>(1542202) AND [Id]<>(1784720) AND [Id]<>(1951879) AND [Id]<>(1951884) AND [Id]<>(1951877) AND [Id]<>(3482051) AND [Id]<>(3482061) AND [Id]<>(3482060) AND [Id]<>(2952503) AND [Id]<>(2952506) AND [Id]<>(2952096) AND [Id]<>(3192854) AND [Id]<>(2894048) AND [Id]<>(3492711) AND [Id]<>(3492717) AND [Id]<>(3492704) AND [Id]<>(3492697) AND [Id]<>(3492699) AND [Id]<>(3492708) AND [Id]<>(3492700) AND [Id]<>(3492693) AND [Id]<>(3492688) AND [Id]<>(3492691) AND [Id]<>(3492684) AND [Id]<>(3492686) AND [Id]<>(3492667) AND [Id]<>(3492647) AND [Id]<>(3492653) AND [Id]<>(3492632) AND [Id]<>(3492694) AND [Id]<>(3492685) AND [Id]<>(3492681) AND [Id]<>(3492680) AND [Id]<>(3492679) AND [Id]<>(3492664) AND [Id]<>(3492672) AND [Id]<>(3492675) AND [Id]<>(3492676) AND [Id]<>(3492671) AND [Id]<>(3492663) AND [Id]<>(3492666) AND [Id]<>(3492652) AND [Id]<>(3492654) AND [Id]<>(3492657) AND [Id]<>(3492638) AND [Id]<>(3492642) AND [Id]<>(3492643) AND [Id]<>(3492641) AND [Id]<>(3492629) AND [Id]<>(3492628) AND [Id]<>(3492535) AND [Id]<>(3492529) AND [Id]<>(1979426) AND [Id]<>(3492615) AND [Id]<>(3492611) AND [Id]<>(3492600) AND [Id]<>(3492596) AND [Id]<>(3492527) AND [Id]<>(3492595) AND [Id]<>(3492590) AND [Id]<>(3492705) AND [Id]<>(3492698) AND [Id]<>(3492696) AND [Id]<>(3492687) AND [Id]<>(3492692) AND [Id]<>(3492678) AND [Id]<>(3492656) AND [Id]<>(3492662) AND [Id]<>(3492674) AND [Id]<>(3492659) AND [Id]<>(3492665) AND [Id]<>(3492648) AND [Id]<>(3492655) AND [Id]<>(3492640) AND [Id]<>(3492639) AND [Id]<>(3492631) AND [Id]<>(3492630) AND [Id]<>(3492651) AND [Id]<>(3492626) AND [Id]<>(3492633) AND [Id]<>(3492547) AND [Id]<>(3492546) AND [Id]<>(3492530) AND [Id]<>(3492521) AND [Id]<>(3492622) AND [Id]<>(1921214) AND [Id]<>(1979412) AND [Id]<>(3492618) AND [Id]<>(3492617) AND [Id]<>(3492614) AND [Id]<>(3492613) AND [Id]<>(3492634) AND [Id]<>(3492650) AND [Id]<>(3492658) AND [Id]<>(3492534) AND [Id]<>(3492532) AND [Id]<>(3492531) AND [Id]<>(3492525) AND [Id]<>(3492623) AND [Id]<>(1979420) AND [Id]<>(1979423) AND [Id]<>(1979416) AND [Id]<>(3492620) AND [Id]<>(3492616) AND [Id]<>(3492609) AND [Id]<>(3492608) AND [Id]<>(3492605) AND [Id]<>(3492604) AND [Id]<>(3492603) AND [Id]<>(3492599) AND [Id]<>(3492597) AND [Id]<>(3492586) AND [Id]<>(3492582) AND [Id]<>(3492578) AND [Id]<>(3492573) AND [Id]<>(3492571) AND [Id]<>(3492570) AND [Id]<>(3492569) AND [Id]<>(3492567) AND [Id]<>(3492565) AND [Id]<>(3492563) AND [Id]<>(3155002) AND [Id]<>(3492635) AND [Id]<>(3492636) AND [Id]<>(3492637) AND [Id]<>(3492627) AND [Id]<>(3492625) AND [Id]<>(3492624) AND [Id]<>(3492536) AND [Id]<>(3492524) AND [Id]<>(3492523) AND [Id]<>(3492533) AND [Id]<>(1979410) AND [Id]<>(3492621) AND [Id]<>(3492619) AND [Id]<>(3492607) AND [Id]<>(3492602) AND [Id]<>(3492601) AND [Id]<>(3492528) AND [Id]<>(3492594) AND [Id]<>(3492591) AND [Id]<>(3492585) AND [Id]<>(3492581) AND [Id]<>(3492584) AND [Id]<>(3154996) AND [Id]<>(3154991) AND [Id]<>(3155006) AND [Id]<>(3154990) AND [Id]<>(3154958) AND [Id]<>(3154957) AND [Id]<>(3155000) AND [Id]<>(3155008) AND [Id]<>(3154986) AND [Id]<>(3155001) AND [Id]<>(3155009) AND [Id]<>(3154955) AND [Id]<>(3154954) AND [Id]<>(3154989) AND [Id]<>(3154967) AND [Id]<>(3154974) AND [Id]<>(3154975) AND [Id]<>(3154972) AND [Id]<>(3154999) AND [Id]<>(3154976) AND [Id]<>(2951707) AND [Id]<>(2951706) AND [Id]<>(2951708) AND [Id]<>(2951710) AND [Id]<>(2951965) AND [Id]<>(2951967) AND [Id]<>(2952476) AND [Id]<>(2952473) AND [Id]<>(2951248) AND [Id]<>(2951575) AND [Id]<>(2951012) AND [Id]<>(2951011) AND [Id]<>(2951223) AND [Id]<>(2951235) AND [Id]<>(2951015) AND [Id]<>(2951241) AND [Id]<>(2951242) AND [Id]<>(2951244) AND [Id]<>(2951337) AND [Id]<>(2951336) AND [Id]<>(3492589) AND [Id]<>(3492588) AND [Id]<>(3492587) AND [Id]<>(3492580) AND [Id]<>(3492579) AND [Id]<>(3492576) AND [Id]<>(3492572) AND [Id]<>(3492568) AND [Id]<>(3492559) AND [Id]<>(3492558) AND [Id]<>(3154997) AND [Id]<>(3155005) AND [Id]<>(3155003) AND [Id]<>(3155012) AND [Id]<>(3155011) AND [Id]<>(3155010) AND [Id]<>(3154960) AND [Id]<>(3154979) AND [Id]<>(3154966) AND [Id]<>(3154981) AND [Id]<>(2951718) AND [Id]<>(2952481) AND [Id]<>(2952480) AND [Id]<>(2951970) AND [Id]<>(2951249) AND [Id]<>(2951574) AND [Id]<>(2951577) AND [Id]<>(2951009) AND [Id]<>(2951236) AND [Id]<>(2951243) AND [Id]<>(2951227) AND [Id]<>(3492612) AND [Id]<>(3492610) AND [Id]<>(3492606) AND [Id]<>(3492526) AND [Id]<>(3492598) AND [Id]<>(3492593) AND [Id]<>(3492592) AND [Id]<>(3492577) AND [Id]<>(3492575) AND [Id]<>(3492574) AND [Id]<>(3492566) AND [Id]<>(3492564) AND [Id]<>(3492560) AND [Id]<>(3492562) AND [Id]<>(3155007) AND [Id]<>(3154956) AND [Id]<>(3155004) AND [Id]<>(3155013) AND [Id]<>(3154964) AND [Id]<>(3154961) AND [Id]<>(3154988) AND [Id]<>(3154998) AND [Id]<>(3154978) AND [Id]<>(3154977) AND [Id]<>(3154980) AND [Id]<>(1728522) AND [Id]<>(1400808) AND [Id]<>(1449484) AND [Id]<>(1550623) AND [Id]<>(2952513) AND [Id]<>(2952482) AND [Id]<>(2951230) AND [Id]<>(2951563) AND [Id]<>(2951565) AND [Id]<>(2951331) AND [Id]<>(1550627) AND [Id]<>(3154987) AND [Id]<>(3154984) AND [Id]<>(3154970) AND [Id]<>(3154992) AND [Id]<>(3154993) AND [Id]<>(3115429) AND [Id]<>(3115430) AND [Id]<>(3192802) AND [Id]<>(2733679) AND [Id]<>(2951709) AND [Id]<>(2952514) AND [Id]<>(2952509) AND [Id]<>(2951966) AND [Id]<>(3192857) AND [Id]<>(2951247) AND [Id]<>(2951250) AND [Id]<>(2951252) AND [Id]<>(2951576) AND [Id]<>(2951579) AND [Id]<>(2951013) AND [Id]<>(2951010) AND [Id]<>(2951008) AND [Id]<>(2951253) AND [Id]<>(2951254) AND [Id]<>(2951233) AND [Id]<>(2951014) AND [Id]<>(1951881) AND [Id]<>(2951334) AND [Id]<>(2951560) AND [Id]<>(2951564) AND [Id]<>(2951327) AND [Id]<>(2951329) AND [Id]<>(1859552) AND [Id]<>(1713176) AND [Id]<>(2951228) AND [Id]<>(2951229) AND [Id]<>(2951232) AND [Id]<>(2951561) AND [Id]<>(2951562) AND [Id]<>(2403789) AND [Id]<>(2403788) AND [Id]<>(2403787) AND [Id]<>(2951963) AND [Id]<>(2951968) AND [Id]<>(2952475) AND [Id]<>(2952474) AND [Id]<>(2952472) AND [Id]<>(2952471) AND [Id]<>(2733678) AND [Id]<>(1550624) AND [Id]<>(1728521) AND [Id]<>(1815786) AND [Id]<>(2951251) AND [Id]<>(2951578) AND [Id]<>(2951224) AND [Id]<>(2951225) AND [Id]<>(2951226) AND [Id]<>(2951234) AND [Id]<>(3192801) AND [Id]<>(2951246) AND [Id]<>(2951245) AND [Id]<>(2951333) AND [Id]<>(2951338) AND [Id]<>(2951339) AND [Id]<>(3220143) AND [Id]<>(2132864) AND [Id]<>(1951878) AND [Id]<>(1951880) AND [Id]<>(3192858) AND [Id]<>(2951231) AND [Id]<>(2951328) AND [Id]<>(2951332) AND [Id]<>(2951330) AND [Id]<>(2167076))");

            entity.HasIndex(e => e.Serial, "NonClusteredIndex-20160919-113245");

            entity.HasIndex(e => e.PackagePk, "NonClusteredIndex-20190601-131054");

            entity.Property(e => e.DateInsert)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DatePackage)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DatePhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InsertUserPk).HasColumnName("InsertUser_pk");
            entity.Property(e => e.PackagePk).HasColumnName("Package_pk");
            entity.Property(e => e.PackageUserPk).HasColumnName("PackageUser_pk");
            entity.Property(e => e.PackagingStationDate)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PackagingStation_Date");
            entity.Property(e => e.PackagingStationId).HasColumnName("PackagingStationID");
            entity.Property(e => e.PackagingStationLevel2Date)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PackagingStationLevel2_Date");
            entity.Property(e => e.PackagingStationLevel2UserPk).HasColumnName("PackagingStationLevel2_UserPK");
            entity.Property(e => e.PackagingStationUserPk).HasColumnName("PackagingStation_UserPK");
            entity.Property(e => e.PcbserialNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("PCBSerialNumber");
            entity.Property(e => e.PcbserialNumberDate)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PCBSerialNumber_Date");
            entity.Property(e => e.PcbserialNumberUserPk).HasColumnName("PCBSerialNumber_UserPk");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumberUserPk).HasColumnName("PhoneNumberUser_pk");
            entity.Property(e => e.Serial)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MeterDevice>(entity =>
        {
            entity.HasKey(e => e.RegistrationId);

            entity.HasIndex(e => e.MeterSerial, "IX_MeterDevices");

            entity.HasIndex(e => e.MeterSerial, "IX_MeterDevices_Serial");

            entity.HasIndex(e => e.MeterIdFk, "NonClusteredIndex-MeterID-FK");

            entity.Property(e => e.FirmwareVersion).HasMaxLength(100);
            entity.Property(e => e.IgnoreCityConflict).HasDefaultValue(false);
            entity.Property(e => e.IsValid).HasDefaultValue(true);
            entity.Property(e => e.MeterCity).HasMaxLength(50);
            entity.Property(e => e.MeterId).HasMaxLength(200);
            entity.Property(e => e.MeterIdFk).HasColumnName("MeterID_FK");
            entity.Property(e => e.MeterSerial).HasMaxLength(100);
            entity.Property(e => e.OperationPassword).HasMaxLength(1024);
            entity.Property(e => e.OrderId).HasMaxLength(100);
            entity.Property(e => e.PreviousMeterSerial)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ProductionTime).HasColumnType("datetime");
            entity.Property(e => e.RegistrationTime).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<MeterDevicesHist>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MeterDevicesHist");

            entity.Property(e => e.FirmwareVersion).HasMaxLength(100);
            entity.Property(e => e.MeterCity).HasMaxLength(50);
            entity.Property(e => e.MeterId).HasMaxLength(200);
            entity.Property(e => e.MeterIdFk).HasColumnName("MeterID_FK");
            entity.Property(e => e.MeterSerial).HasMaxLength(100);
            entity.Property(e => e.OperationPassword).HasMaxLength(1024);
            entity.Property(e => e.OrderId).HasMaxLength(100);
            entity.Property(e => e.PreviousMeterSerial)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ProductionTime).HasColumnType("datetime");
            entity.Property(e => e.RegistrationTime).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<MeterRefer>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MeterRefer");

            entity.Property(e => e.Date).HasMaxLength(50);
            entity.Property(e => e.DatePackage).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PackagePk).HasColumnName("Package_pk");
            entity.Property(e => e.PackageUserPk).HasColumnName("PackageUser_pk");
            entity.Property(e => e.UserPk).HasColumnName("User_pk");
        });

        modelBuilder.Entity<MeterReferValue>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PackagePk).HasColumnName("Package_pk");
            entity.Property(e => e._000000)
                .HasMaxLength(50)
                .HasColumnName("00_00_00");
            entity.Property(e => e._000901)
                .HasMaxLength(50)
                .HasColumnName("00_09_01");
            entity.Property(e => e._000902)
                .HasMaxLength(50)
                .HasColumnName("00_09_02");
            entity.Property(e => e._010600)
                .HasMaxLength(50)
                .HasColumnName("01_06_00");
            entity.Property(e => e._010800)
                .HasMaxLength(50)
                .HasColumnName("01_08_00");
            entity.Property(e => e._010801)
                .HasMaxLength(50)
                .HasColumnName("01_08_01");
            entity.Property(e => e._010802)
                .HasMaxLength(50)
                .HasColumnName("01_08_02");
            entity.Property(e => e._010803)
                .HasMaxLength(50)
                .HasColumnName("01_08_03");
            entity.Property(e => e._010804)
                .HasMaxLength(50)
                .HasColumnName("01_08_04");
            entity.Property(e => e._020801)
                .HasMaxLength(50)
                .HasColumnName("02_08_01");
            entity.Property(e => e._0c0200)
                .HasMaxLength(50)
                .HasColumnName("0C_02_00");
            entity.Property(e => e._0c0201)
                .HasMaxLength(50)
                .HasColumnName("0C_02_01");
            entity.Property(e => e._0c0202)
                .HasMaxLength(50)
                .HasColumnName("0C_02_02");
            entity.Property(e => e._0c0600)
                .HasMaxLength(50)
                .HasColumnName("0C_06_00");
            entity.Property(e => e._0c0700)
                .HasMaxLength(50)
                .HasColumnName("0C_07_00");
            entity.Property(e => e._0c5108)
                .HasMaxLength(50)
                .HasColumnName("0C_51_08");
            entity.Property(e => e._0c5400)
                .HasMaxLength(50)
                .HasColumnName("0C_54_00");
            entity.Property(e => e._0c7100)
                .HasMaxLength(50)
                .HasColumnName("0C_71_00");
            entity.Property(e => e._0c7102)
                .HasMaxLength(50)
                .HasColumnName("0C_71_02");
            entity.Property(e => e._0c8100)
                .HasMaxLength(50)
                .HasColumnName("0C_81_00");
            entity.Property(e => e._0c8200)
                .HasMaxLength(50)
                .HasColumnName("0C_82_00");
            entity.Property(e => e._0c8300)
                .HasMaxLength(50)
                .HasColumnName("0C_83_00");
            entity.Property(e => e._0c8400)
                .HasMaxLength(50)
                .HasColumnName("0C_84_00");
            entity.Property(e => e._0f0f00)
                .HasMaxLength(50)
                .HasColumnName("0F_0F_00");
            entity.Property(e => e._310700)
                .HasMaxLength(50)
                .HasColumnName("31_07_00");
            entity.Property(e => e._320700)
                .HasMaxLength(50)
                .HasColumnName("32_07_00");
        });

        modelBuilder.Entity<MetersByCity>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("MetersByCity");

            entity.Property(e => e.DatePhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EndSerial)
                .HasMaxLength(17)
                .IsUnicode(false);
            entity.Property(e => e.OrderNumber).HasMaxLength(255);
            entity.Property(e => e.OrderRegNumber).HasMaxLength(255);
            entity.Property(e => e.PcbserialNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("PCBSerialNumber");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RsportTypeId).HasColumnName("RSPortTypeId");
            entity.Property(e => e.Serial)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StartSerial)
                .HasMaxLength(17)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MetersHist>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MetersHist");

            entity.Property(e => e.DateInsert)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DatePackage)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DatePhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InsertUserPk).HasColumnName("InsertUser_pk");
            entity.Property(e => e.PackagePk).HasColumnName("Package_pk");
            entity.Property(e => e.PackageUserPk).HasColumnName("PackageUser_pk");
            entity.Property(e => e.PackagingStationDate)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PackagingStation_Date");
            entity.Property(e => e.PackagingStationId).HasColumnName("PackagingStationID");
            entity.Property(e => e.PackagingStationLevel2Date)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PackagingStationLevel2_Date");
            entity.Property(e => e.PackagingStationLevel2UserPk).HasColumnName("PackagingStationLevel2_UserPK");
            entity.Property(e => e.PackagingStationUserPk).HasColumnName("PackagingStation_UserPK");
            entity.Property(e => e.PcbserialNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("PCBSerialNumber");
            entity.Property(e => e.PcbserialNumberDate)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PCBSerialNumber_Date");
            entity.Property(e => e.PcbserialNumberUserPk).HasColumnName("PCBSerialNumber_UserPk");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumberUserPk).HasColumnName("PhoneNumberUser_pk");
            entity.Property(e => e.Serial)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Mtfsoftversion>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MTFSOFTVersion");

            entity.Property(e => e.DateVersion)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("dateVersion");
            entity.Property(e => e.DescVersion)
                .HasMaxLength(500)
                .HasColumnName("descVersion");
            entity.Property(e => e.Idversion).HasColumnName("IDVersion");
            entity.Property(e => e.NameVersion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TypeVersion)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<OnlineStation>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Cartons)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.StationId).HasColumnName("StationID");
        });

        modelBuilder.Entity<OperationsLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("OperationsLog");

            entity.Property(e => e.OperationDetails).HasMaxLength(255);
            entity.Property(e => e.OperationTime).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.Operation).WithMany(p => p.OperationsLogs)
                .HasForeignKey(d => d.OperationId)
                .HasConstraintName("FK_OperationsLog_AppTypes");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber, "IX_OrderNumber");

            entity.HasIndex(e => e.RegDate, "IX_RegDate");

            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.EndSerial)
                .HasMaxLength(17)
                .IsUnicode(false);
            entity.Property(e => e.OperatorId).HasColumnName("OperatorID");
            entity.Property(e => e.OrderNumber).HasMaxLength(255);
            entity.Property(e => e.OrderRegNumber).HasMaxLength(255);
            entity.Property(e => e.RegDate).HasColumnType("datetime");
            entity.Property(e => e.RegisterTime).HasColumnType("datetime");
            entity.Property(e => e.StartSerial)
                .HasMaxLength(17)
                .IsUnicode(false);

            entity.HasOne(d => d.City).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_Orders_Cities");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.PackagingId).HasName("PK_Packaging1");

            entity.Property(e => e.CheckSum)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.EndSerial)
                .HasMaxLength(17)
                .IsUnicode(false);
            entity.Property(e => e.FrimwareVersion)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.OperatorId).HasColumnName("OperatorID");
            entity.Property(e => e.ProfileName).HasMaxLength(200);
            entity.Property(e => e.RegisterTime).HasColumnType("datetime");
            entity.Property(e => e.RsportTypeId).HasColumnName("RSPortTypeId");
            entity.Property(e => e.StartSerial)
                .HasMaxLength(17)
                .IsUnicode(false);

            entity.HasOne(d => d.CoverType).WithMany(p => p.Packages)
                .HasForeignKey(d => d.CoverTypeId)
                .HasConstraintName("FK_Packages_AppTypes1");

            entity.HasOne(d => d.Order).WithMany(p => p.Packages)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Packages_Orders1");
        });

        modelBuilder.Entity<PackagesLatest>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Packages_Latest");

            entity.Property(e => e.CityName).HasMaxLength(50);
            entity.Property(e => e.DateInsert)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrderNum)
                .HasMaxLength(255)
                .HasColumnName("orderNum");
        });

        modelBuilder.Entity<PackagesOld>(entity =>
        {
            entity.HasKey(e => e.PackagingId).HasName("PK_Packaging");

            entity.ToTable("Packages_old");

            entity.HasIndex(e => e.MeterSerialEnd, "IX_MeterSerialEnd");

            entity.HasIndex(e => e.MeterSerialStart, "IX_MeterSerialStart").IsUnique();

            entity.Property(e => e.MeterSerialEnd).HasMaxLength(50);
            entity.Property(e => e.MeterSerialStart).HasMaxLength(50);

            entity.HasOne(d => d.CoverType).WithMany(p => p.PackagesOldCoverTypes)
                .HasForeignKey(d => d.CoverTypeId)
                .HasConstraintName("FK_Packages_AppTypes");

            entity.HasOne(d => d.MeterBase).WithMany(p => p.PackagesOldMeterBases)
                .HasForeignKey(d => d.MeterBaseId)
                .HasConstraintName("FK_Packaging_AppTypes");

            entity.HasOne(d => d.MeterType).WithMany(p => p.PackagesOldMeterTypes)
                .HasForeignKey(d => d.MeterTypeId)
                .HasConstraintName("FK_Packaging_AppTypes1");

            entity.HasOne(d => d.Order).WithMany(p => p.PackagesOlds)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Packages_Orders");

            entity.HasOne(d => d.PackageType).WithMany(p => p.PackagesOldPackageTypes)
                .HasForeignKey(d => d.PackageTypeId)
                .HasConstraintName("FK_Packaging_AppTypes2");
        });

        modelBuilder.Entity<PackagingKasri>(entity =>
        {
            entity.ToTable("PackagingKasri");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.PackagingStationId).HasColumnName("PackagingStationID");
        });

        modelBuilder.Entity<PackgingStation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PackingStation");

            entity.ToTable("PackgingStation");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.StationName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PrintPasswordUser>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<ProfileLog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ProfileLog");

            entity.Property(e => e.CityId).HasColumnName("City_id");
            entity.Property(e => e.Date).HasMaxLength(50);
            entity.Property(e => e.ManufactoryPassword).HasMaxLength(50);
            entity.Property(e => e.MeterType).HasMaxLength(50);
            entity.Property(e => e.OperatorPassword).HasMaxLength(50);
            entity.Property(e => e.SettingPassword).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(50);
            entity.Property(e => e.UtilityPassword).HasMaxLength(50);
        });

        modelBuilder.Entity<ReturnAll>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Return_all");

            entity.Property(e => e.CityName).HasMaxLength(50);
            entity.Property(e => e.CityNameEn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReturnedNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SentDate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.State).HasMaxLength(50);
        });

        modelBuilder.Entity<ReturnOpen>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Return_Open");

            entity.Property(e => e.CityName).HasMaxLength(50);
            entity.Property(e => e.CityNameEn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReturnedNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SentDate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.State).HasMaxLength(50);
        });

        modelBuilder.Entity<ReturnedMajorProblem>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("ReturnedMajorProblem");

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.English)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Persian).HasMaxLength(50);
        });

        modelBuilder.Entity<ReturnedMeter>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CityId).HasColumnName("CityID");
            entity.Property(e => e.Customer).HasMaxLength(100);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.MajorProblemId).HasColumnName("MajorProblemID");
            entity.Property(e => e.MinorProblemId).HasColumnName("MinorProblemID");
            entity.Property(e => e.OperatorId).HasColumnName("OperatorID");
            entity.Property(e => e.ReceivedMeter)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ResponsibleId).HasColumnName("ResponsibleID");
            entity.Property(e => e.ReturnedNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SentBy).HasMaxLength(50);
            entity.Property(e => e.SentDate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SentMeter)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SimcardNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StatuesId).HasColumnName("StatuesID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<ReturnedMinorProblem>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("ReturnedMinorProblem");

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.English)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Persian).HasMaxLength(50);
        });

        modelBuilder.Entity<ReturnedResponsible>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("ReturnedResponsible");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ReturnedStatus>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("ReturnedStatus");

            entity.Property(e => e.State).HasMaxLength(50);
        });

        modelBuilder.Entity<Rm>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RM");

            entity.Property(e => e.CityName).HasMaxLength(50);
            entity.Property(e => e.CityNameEn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Customer).HasMaxLength(100);
            entity.Property(e => e.English)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Expr2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Expr3).HasMaxLength(50);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Persian).HasMaxLength(50);
            entity.Property(e => e.ReceivedMeter)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReturnedNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SentBy).HasMaxLength(50);
            entity.Property(e => e.SentDate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SentMeter)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SimcardNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.State).HasMaxLength(50);
        });

        modelBuilder.Entity<StatisticsReturnedMeter>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Statistics_ReturnedMeters");

            entity.Property(e => e.Rn)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("RN");
        });

        modelBuilder.Entity<TableTest>(entity =>
        {
            entity.ToTable("TableTest");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.C2).HasColumnName("c2");
            entity.Property(e => e.Name)
                .HasMaxLength(70)
                .HasColumnName("name_");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<UserManagement>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("UserManagement");

            entity.Property(e => e.ActiveDate).HasColumnType("datetime");
            entity.Property(e => e.DeactiveDate).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<VerifiedProfileByQc>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("VerifiedProfileByQC");

            entity.Property(e => e.CityId).HasColumnName("City_id");
            entity.Property(e => e.Date).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.MeterType)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Profile).HasColumnType("text");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
