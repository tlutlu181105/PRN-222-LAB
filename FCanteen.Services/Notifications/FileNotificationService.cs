using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Services.Notifications;

public class FileNotificationService : INotificationService
{
    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "notifications.log");

    public async Task SendAsync(string subject, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {subject} | {message}{Environment.NewLine}";
        await File.AppendAllTextAsync(FilePath, line);
    }
}
