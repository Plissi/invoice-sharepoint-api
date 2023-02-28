namespace DechargeAPI.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("FactureADecharger")]
    public class FactureADecharger
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int id;
        public String customerCode;
        public String customerName;
        public DateTime date;
        public int amount;
        public int invoiceCount;
        public String status;

        public FactureADecharger(int id, string customerCode, string customerName, DateTime date, int amount, int invoiceCount, string status)
        {
            this.id = id;
            this.customerCode = customerCode;
            this.customerName = customerName;
            this.date = date;
            this.amount = amount;
            this.invoiceCount = invoiceCount;
            this.status = status;
        }
    }
}