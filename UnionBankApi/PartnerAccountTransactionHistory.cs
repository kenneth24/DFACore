using System.Collections.Generic;

namespace UnionBankApi
{
    public class PartnerAccountTransactionHistory
    {
        public IEnumerable<PartnerAccountTransaction> Records { get; set; }

        public string TotalRecords { get; set; }

        public string LastRunningBalance { get; set; }
    }
}
