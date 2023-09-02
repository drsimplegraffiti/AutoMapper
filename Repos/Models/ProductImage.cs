using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Techie.Repos.Models;

[Table("ProductImage")]
public partial class ProductImage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("productcode")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Productcode { get; set; }

    [Column("productimage", TypeName = "image")]
    public byte[]? Productimage { get; set; }
}
