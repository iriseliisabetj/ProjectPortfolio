﻿// <auto-generated />
using System;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DAL.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250102175423_Changes")]
    partial class Changes
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("Domain.Configuration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BoardSizeHeight")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BoardSizeWidth")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConfigName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("GridSizeHeight")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GridSizeWidth")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("MovePieceAfterNMoves")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PiecesPerPlayer")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WinCondition")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Configurations");
                });

            modelBuilder.Entity("Domain.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ConfigId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("GameStateJson")
                        .IsRequired()
                        .HasMaxLength(10240)
                        .HasColumnType("TEXT");

                    b.Property<int>("GameType")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlayerOPass")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("PlayerXPass")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ConfigId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("Domain.Game", b =>
                {
                    b.HasOne("Domain.Configuration", "Config")
                        .WithMany("Games")
                        .HasForeignKey("ConfigId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Config");
                });

            modelBuilder.Entity("Domain.Configuration", b =>
                {
                    b.Navigation("Games");
                });
#pragma warning restore 612, 618
        }
    }
}
