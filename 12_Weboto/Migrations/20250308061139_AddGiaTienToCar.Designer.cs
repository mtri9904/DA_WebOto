﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using _12_Weboto.Data;

#nullable disable

namespace _12_Weboto.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250308061139_AddGiaTienToCar")]
    partial class AddGiaTienToCar
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("_12_Weboto.Models.Car", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ChieuCao")
                        .HasColumnType("int");

                    b.Property<int>("ChieuDai")
                        .HasColumnType("int");

                    b.Property<int>("ChieuRong")
                        .HasColumnType("int");

                    b.Property<int>("CoSoBanhXe")
                        .HasColumnType("int");

                    b.Property<string>("DongCo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DongXe")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("GiaTien")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("HangXe")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HopSo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KieuDang")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LopSau")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LopTruoc")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("MucTieuThuNgoaiDoThi")
                        .HasColumnType("real");

                    b.Property<float>("MucTieuThuTrongDoThi")
                        .HasColumnType("real");

                    b.Property<int>("NamSanXuat")
                        .HasColumnType("int");

                    b.Property<string>("NhienLieu")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhienBan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SoChoNgoi")
                        .HasColumnType("int");

                    b.Property<int>("SoKM")
                        .HasColumnType("int");

                    b.Property<string>("TenXe")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TrongLuongKhongTai")
                        .HasColumnType("int");

                    b.Property<string>("XuatXu")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Cars");
                });

            modelBuilder.Entity("_12_Weboto.Models.CarImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CarId")
                        .HasColumnType("int");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.ToTable("CarImages");
                });

            modelBuilder.Entity("_12_Weboto.Models.CarImage", b =>
                {
                    b.HasOne("_12_Weboto.Models.Car", "Car")
                        .WithMany("Images")
                        .HasForeignKey("CarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Car");
                });

            modelBuilder.Entity("_12_Weboto.Models.Car", b =>
                {
                    b.Navigation("Images");
                });
#pragma warning restore 612, 618
        }
    }
}
