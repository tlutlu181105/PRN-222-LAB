using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class MenuItem
{
    public int Id { get; set; }
    public string Code { get; set; } = "";         
    public string Name { get; set; } = "";          
    public decimal Price { get; set; }               
    public string Unit { get; set; } = "";            
    public bool IsAvailable { get; set; } = true;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }


    public ICollection<TicketLine> TicketLines { get; set; } = new List<TicketLine>();
    public ICollection<MenuItemIngredient> MenuItemIngredients { get; set; } = new List<MenuItemIngredient>();  
}