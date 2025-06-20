﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Models;

public partial class Factura
{
    [Key]
    public int IdFactura { get; set; }

    [StringLength(50)]
    public string? NumeroFactura { get; set; }

    public DateOnly? FechaEmision { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? MontoTotal { get; set; }

    [StringLength(50)]
    public string? Estado { get; set; }

    public int? IdCliente { get; set; }

    [InverseProperty("IdFacturaNavigation")]
    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();

    [ForeignKey("IdCliente")]
    [InverseProperty("Facturas")]
    public virtual Cliente? IdClienteNavigation { get; set; }
}