using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = "";        
    public string Unit { get; set; } = "";          
    public decimal StockQuantity { get; set; }        
    public decimal WarningThreshold { get; set; }
    public decimal UnitCost { get; set; }   // đơn giá nhập, dùng để tính giá vốn món ăn
}
