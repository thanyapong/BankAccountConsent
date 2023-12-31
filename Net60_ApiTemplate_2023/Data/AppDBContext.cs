﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TTB.BankAccountConsent.Models;

namespace TTB.BankAccountConsent.Data
{
    public partial class AppDBContext : DbContext
    {
        public AppDBContext()
        {
        }

        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BatchSource> BatchSources { get; set; }
        public virtual DbSet<TTBActionType> TTBActionTypes { get; set; }
        public virtual DbSet<TTBBatchDetail> TTBBatchDetails { get; set; }
        public virtual DbSet<TTBBatchHeader> TTBBatchHeaders { get; set; }
        public virtual DbSet<TTBBatchTrailer> TTBBatchTrailers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Thai_100_CI_AI");

            modelBuilder.Entity<TTBBatchDetail>(entity =>
            {
                entity.Property(e => e.TTBBatchDetailId).ValueGeneratedNever();
            });

            modelBuilder.Entity<TTBBatchHeader>(entity =>
            {
                entity.Property(e => e.TTBBatchHeaderId).ValueGeneratedNever();
            });

            modelBuilder.Entity<TTBBatchTrailer>(entity =>
            {
                entity.Property(e => e.TTBBatchTrailerId).ValueGeneratedNever();
            });

            OnModelCreatingGeneratedProcedures(modelBuilder);
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}