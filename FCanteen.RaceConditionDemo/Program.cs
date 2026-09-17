using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FCanteen.Data;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("FCanteenConnection");

FCanteenContext CreateContext()
{
    var optionsBuilder = new DbContextOptionsBuilder<FCanteenContext>();
    optionsBuilder.UseSqlServer(connectionString);
    return new FCanteenContext(optionsBuilder.Options);
}

Console.WriteLine("=== YC5: Race Condition khi trừ nguyên liệu ===");
Console.WriteLine();

// Lấy tồn kho ban đầu của 1 nguyên liệu để làm thí nghiệm
int ingredientId;
decimal initialStock;
using (var db = CreateContext())
{
    var ingredient = await db.Ingredients.FirstAsync(i => i.Name == "Thịt gà");
    ingredientId = ingredient.Id;
    initialStock = ingredient.StockQuantity;
}

Console.WriteLine($"Tồn kho ban đầu của 'Thịt gà': {initialStock} kg");
Console.WriteLine();

const int NumberOfOrders = 1000;      // mô phỏng 1000 phiếu order cùng trừ nguyên liệu này
const decimal AmountPerOrder = 0.1m;  // mỗi phiếu trừ 0.1 kg
decimal expectedFinalStock = initialStock - (NumberOfOrders * AmountPerOrder);

Console.WriteLine($"Mô phỏng {NumberOfOrders} phiếu order, mỗi phiếu trừ {AmountPerOrder} kg");
Console.WriteLine($"Tồn kho ĐÚNG phải là: {expectedFinalStock} kg");
Console.WriteLine();
// bắt đầu chạy từng bản // 
Console.WriteLine("---- PHIÊN BẢN 1: KHÔNG đồng bộ (race condition) ----");

decimal stockWithoutSync = initialStock;

var tasksWithoutSync = new List<Task>();
for (int i = 0; i < NumberOfOrders; i++)
{
    tasksWithoutSync.Add(Task.Run(() =>
    {
        // Mô phỏng đúng 3 bước ngầm của "stock -= amount": đọc, tính, ghi
        decimal current = stockWithoutSync;   // Bước 1: đọc
        decimal updated = current - AmountPerOrder; // Bước 2: tính
        Thread.Sleep(0); // nhường CPU 1 nhịp, làm tăng khả năng 2 luồng đọc cùng lúc (mô phỏng rõ race condition hơn)
        stockWithoutSync = updated;             // Bước 3: ghi — CÓ THỂ ghi đè mất kết quả của luồng khác
    }));
}

await Task.WhenAll(tasksWithoutSync);

Console.WriteLine($"Tồn kho SAU khi chạy (không đồng bộ): {stockWithoutSync} kg");
Console.WriteLine($"Chênh lệch so với giá trị đúng: {stockWithoutSync - expectedFinalStock} kg");
Console.WriteLine(stockWithoutSync != expectedFinalStock
    ? "=> SAI — đã xảy ra race condition, mất dữ liệu do nhiều luồng ghi đè lẫn nhau!"
    : "=> Lần chạy này ngẫu nhiên không lộ race condition, thử chạy lại.");
Console.WriteLine();

//===============================//
Console.WriteLine("---- PHIÊN BẢN 2: CÓ đồng bộ bằng lock ----");

decimal stockWithLock = initialStock;
var lockObject = new object();

var tasksWithLock = new List<Task>();
for (int i = 0; i < NumberOfOrders; i++)
{
    tasksWithLock.Add(Task.Run(() =>
    {
        lock (lockObject)
        {
            decimal current = stockWithLock;
            decimal updated = current - AmountPerOrder;
            Thread.Sleep(0);
            stockWithLock = updated;
        }
    }));
}

await Task.WhenAll(tasksWithLock);

Console.WriteLine($"Tồn kho SAU khi chạy (có lock): {stockWithLock} kg");
Console.WriteLine($"Chênh lệch so với giá trị đúng: {stockWithLock - expectedFinalStock} kg");
Console.WriteLine(stockWithLock == expectedFinalStock
    ? "=> ĐÚNG — lock đã ngăn được race condition."
    : "=> Vẫn sai, kiểm tra lại code.");
Console.WriteLine();
//===============================//

Console.WriteLine("---- PHIÊN BẢN 3: CÓ đồng bộ bằng Interlocked ----");

long stockInCentiGrams = (long)(initialStock * 1000); // Interlocked chỉ hỗ trợ long/int, nên đổi kg sang gram*10 để giữ số lẻ
long amountPerOrderInCentiGrams = (long)(AmountPerOrder * 1000);

var tasksWithInterlocked = new List<Task>();
for (int i = 0; i < NumberOfOrders; i++)
{
    tasksWithInterlocked.Add(Task.Run(() =>
    {
        Interlocked.Add(ref stockInCentiGrams, -amountPerOrderInCentiGrams);
    }));
}

await Task.WhenAll(tasksWithInterlocked);

decimal stockWithInterlocked = stockInCentiGrams / 1000m;
Console.WriteLine($"Tồn kho SAU khi chạy (Interlocked): {stockWithInterlocked} kg");
Console.WriteLine($"Chênh lệch so với giá trị đúng: {stockWithInterlocked - expectedFinalStock} kg");
Console.WriteLine(stockWithInterlocked == expectedFinalStock
    ? "=> ĐÚNG — Interlocked đã ngăn được race condition."
    : "=> Vẫn sai, kiểm tra lại code.");

//===============================//
