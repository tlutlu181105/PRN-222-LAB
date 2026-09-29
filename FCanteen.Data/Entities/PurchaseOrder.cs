namespace FCanteen.Data.Entities;

public class PurchaseOrder
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Pending";

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}