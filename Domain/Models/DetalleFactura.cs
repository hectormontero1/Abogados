﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Models;

[Table("DetalleFactura")]
public partial class DetalleFactura
{
    [Key]
    public int IdDetalle { get; set; }

    public int? IdFactura { get; set; }

    public int? IdTiempo { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? Subtotal { get; set; }

    [ForeignKey("IdFactura")]
    [InverseProperty("DetalleFacturas")]
    public virtual Factura? IdFacturaNavigation { get; set; }

    [ForeignKey("IdTiempo")]
    [InverseProperty("DetalleFacturas")]
    public virtual TiempoFacturable? IdTiempoNavigation { get; set; }
}