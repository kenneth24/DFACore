using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFACore.Models
{
    public class Payment
    {
        public long Id { get; set; }

        public DateTime TransactionDate { get; set; }
        [StringLength(50)]
        public string TransactionReferenceId { get; set; }

        [StringLength(50)]
        public string TransactionId { get; set; }

        [Column(TypeName = "decimal(9,2)")]
        public decimal Amount { get; set; }

        public Enums.PaymentStatus Status { get; set; }

        public Guid UserId { get; set; }
    }
}
