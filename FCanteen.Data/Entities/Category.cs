using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Data.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";   

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}