namespace ZaloOAWebhook.Models
{
    public class Point
    {
        public string CD_CSTMR { get; set; }=string.Empty;
        public int SEQ { get; set; }
        public string CD_SHOP { get; set; } = string.Empty;
        public DateTime? POS_DATE { get; set; }
        public int? POS_NO { get; set; }
        public int? BILL_NO { get; set; }
        public double? TAXATION { get; set; }
        public double? TAXFREE { get; set; }
        public string REMARK { get; set; } = string.Empty;
        public double? P_CARRYOVER { get; set; }
        public double? P_SAVE { get; set; }
        public double? P_USE { get; set; }
        public double? P_NOW { get; set; }
        public string SAVE_CD_EMP { get; set; } = string.Empty;
        public DateTime? SAVE_DATE { get; set; }
        public string UPDATE_CD_EMP { get; set; } = string.Empty;
        public DateTime? UPDATE_DATE { get; set; }
    }
}
