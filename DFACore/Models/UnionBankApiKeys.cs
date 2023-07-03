using System.ComponentModel.DataAnnotations;

namespace DFACore.Models
{
    public class UnionBankApiKeys
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientId { get; set; }

        [Required]
        [StringLength(70)]
        public string ClientSecret { get; set; }

        [Required]
        [StringLength(50)]
        public string PartnerId { get; set; }

        public Enums.UnionBankApiKeysEnviroment Environment { get; set; }
    }
}
