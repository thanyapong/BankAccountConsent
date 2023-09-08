﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTB.BankAccountConsent.Models
{
    [Table("BatchSource")]
    public partial class BatchSource
    {
        [Key]
        public int BatchSourceId { get; set; }
        [StringLength(100)]
        public string FileName { get; set; }
        public string LocalPath { get; set; }
        public Guid? BatchHeaderId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedByUserId { get; set; }
    }
}