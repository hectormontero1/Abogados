﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Models;

public partial class Log
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DateTime { get; set; }

    [StringLength(50)]
    public string Level { get; set; } = null!;

    public string Message { get; set; } = null!;

    [StringLength(255)]
    public string Logger { get; set; } = null!;

    [StringLength(255)]
    public string? Thread { get; set; }

    public string? Exception { get; set; }

    public string? StackTrace { get; set; }
}