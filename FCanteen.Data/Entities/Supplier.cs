using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Data.Entities;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Address { get; set; } = "";
}