using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Services.Notifications;

public class ConsoleNotificationService : INotificationService
{
    public Task SendAsync(string subject, string message)
    {
        Console.WriteLine($"[CONSOLE] {subject}: {message}");
        return Task.CompletedTask;
    }
}