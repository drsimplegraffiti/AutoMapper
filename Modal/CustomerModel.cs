using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Techie.Modal
{
	public class CustomerModel
	{
        [StringLength(50)]
        [Unicode(false)]
        public string Code { get; set; } = null!;

        [StringLength(50)]
        [Unicode(false)]
        public string Name { get; set; } = null!;

        [StringLength(50)]
        [Unicode(false)]
        public string Email { get; set; } = null!;

        [StringLength(50)]
        [Unicode(false)]
        public string PhoneNumber { get; set; } = null!;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Creditlimit { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedDate { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string TaxCode { get; set; } = null!;

        [JsonIgnore]
        public string? StatusName { get; set; } 
    }
}

