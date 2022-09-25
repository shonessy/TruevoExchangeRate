﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TruevoExchangeRateAPI.Data;

#nullable disable

namespace TruevoExchangeRateAPI.Data.Migrations
{
    [DbContext(typeof(TruevoExchangeRateDbContext))]
    partial class TruevoExchangeRateDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TruevoExchangeRateAPI.Models.ExchangeRate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("BuyRate")
                        .HasColumnType("numeric");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("CurrencyExponent")
                        .HasColumnType("integer");

                    b.Property<int>("CurrencyNumber")
                        .HasColumnType("integer");

                    b.Property<decimal>("MidRate")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SellRate")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("ValidityDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyCode")
                        .IsUnique();

                    b.HasIndex("CurrencyNumber")
                        .IsUnique();

                    b.ToTable("ExchangeRates");
                });
#pragma warning restore 612, 618
        }
    }
}
