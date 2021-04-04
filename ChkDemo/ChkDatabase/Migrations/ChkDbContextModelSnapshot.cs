﻿// <auto-generated />
using System;
using ChkDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChkDatabase.Migrations
{
    [DbContext(typeof(ChkDbContext))]
    partial class ChkDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.4")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ChkDatabase.Entites.ChkMerchant", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("APIKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BankAcc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BankSort")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Merchants");
                });

            modelBuilder.Entity("ChkDatabase.Entites.ChkTransaction", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,4)");

                    b.Property<Guid>("BankTransactionID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CardCVV")
                        .HasColumnType("int");

                    b.Property<int>("CardExpMonth")
                        .HasColumnType("int");

                    b.Property<int>("CardExpYear")
                        .HasColumnType("int");

                    b.Property<string>("CardNameOnCard")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CardNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CurrencyID")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateTimeStateLastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("StateID")
                        .HasColumnType("int");

                    b.Property<string>("StateMessage")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("MerchantID");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("ChkDatabase.Entites.ChkTransaction", b =>
                {
                    b.HasOne("ChkDatabase.Entites.ChkMerchant", "Merchant")
                        .WithMany()
                        .HasForeignKey("MerchantID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Merchant");
                });
#pragma warning restore 612, 618
        }
    }
}
