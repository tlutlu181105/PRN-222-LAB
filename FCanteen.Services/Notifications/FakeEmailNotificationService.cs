using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Services.Notifications;

public class FakeEmailNotificationService : INotificationService
{
    public async Task SendAsync(string subject, string message)
    {
        await Task.Delay(200);   // giả lập độ trễ khi gửi mail

        Console.WriteLine("[EMAIL] ---------------------------------");
        Console.WriteLine("  From:    noreply@fcanteen.local");
        Console.WriteLine("  To:      kitchen@fcanteen.local");
        Console.WriteLine($"  Subject: {subject}");
        Console.WriteLine($"  Body:    {message}");
        Console.WriteLine("-----------------------------------------");
    }
}