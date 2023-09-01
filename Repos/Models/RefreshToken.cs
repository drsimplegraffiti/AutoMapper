using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Techie.Repos.Models;

[Table("RefreshToken")]
public partial class RefreshToken
{
    [Key]
    [Column("userid")]
    [StringLength(50)]
    [Unicode(false)]
    public string Userid { get; set; } = null!;

    [Column("tokenid")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Tokenid { get; set; }

    [Column("refreshtoken")]
    [Unicode(false)]
    public string? Refreshtoken { get; set; }
}
