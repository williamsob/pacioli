using Pacioli.Core;

InMemoryJournalStore journal = new();
Ledger ledger = new(journal);
var key = Guid.NewGuid().ToString();
var key2 = Guid.NewGuid().ToString();

var bank = ledger.CreateAccount("bancabresciana", "Banca Bresciana", "GBP");
var davinci = ledger.CreateAccount("davinci-current","Da Vinci", "GBP");
var caravaggio = ledger.CreateAccount("caravaggio-business", "Caravaggio", "GBP");

Transaction transfer = ledger.Transfer("bancabresciana", "caravaggio-business", 10000, key);
Transaction transfer2 = ledger.Transfer("caravaggio-business", "davinci-current", 2000, key2);

Console.OutputEncoding = System.Text.Encoding.UTF8;   // so £ prints correctly

Console.WriteLine("=== Transfers ===");
PrintTransfer("Bank pays Caravaggio", transfer);
PrintTransfer("Caravaggio pays DaVinci", transfer2);


Console.WriteLine();
Console.WriteLine("=== Closing balances ==="); //hardcoded for demo purposes
long bankBal = ledger.GetBalance("bancabresciana");
long cavBal = ledger.GetBalance("caravaggio-business");
long davBal = ledger.GetBalance("davinci-current");

Console.WriteLine($"{bank.Name,-18} {Gbp(bankBal),12}");
Console.WriteLine($"{caravaggio.Name,-18} {Gbp(cavBal),12}");
Console.WriteLine($"{davinci.Name,-18} {Gbp(davBal),12}");

long total = bankBal + cavBal + davBal;
Console.WriteLine(new string('-', 31));
Console.WriteLine($"{"Total",-18} {Gbp(total),12}");
Console.WriteLine(total == 0 ? "Books balance." : "WARNING: books do NOT balance.");

Console.WriteLine();
Console.WriteLine("=== Reversal ===");

// helper used below (hardcoded for demo purposes: assumes these three accounts)
void PrintBalances(string title)
{
    long b = ledger.GetBalance("bancabresciana");
    long c = ledger.GetBalance("caravaggio-business");
    long d = ledger.GetBalance("davinci-current");
    Console.WriteLine($"{title}: bank {Gbp(b)} | Caravaggio {Gbp(c)} | DaVinci {Gbp(d)} | total {Gbp(b + c + d)}");
}

PrintBalances("Before reversal");

// 1. reverse the second transfer (Caravaggio -> DaVinci)
var reverseKey = Guid.NewGuid().ToString();
Transaction reversal = ledger.Reverse(transfer2.Id, reverseKey);
PrintTransfer("Reversal of Caravaggio pays DaVinci", reversal);
PrintBalances("After reversal ");
// expected: Caravaggio back to £100.00 and DaVinci back to £0.00

// 2. retry the same reversal with the same key: should return the original, change nothing
Transaction retry = ledger.Reverse(transfer2.Id, reverseKey);
Console.WriteLine();
Console.WriteLine($"Retry returns the same transaction: {retry.Id == reversal.Id}");
PrintBalances("After retry    ");

// 3. reverse the same transfer again with a NEW key: should be rejected
try
{
    ledger.Reverse(transfer2.Id, Guid.NewGuid().ToString());
    Console.WriteLine("BUG: second reversal was not rejected");
}
catch (Exception ex)
{
    Console.WriteLine($"Second reversal rejected: {ex.Message}");
}

// 4. reverse the reversal: should be rejected
try
{
    ledger.Reverse(reversal.Id, Guid.NewGuid().ToString());
    Console.WriteLine("BUG: reversing a reversal was not rejected");
}
catch (Exception ex)
{
    Console.WriteLine($"Reversing a reversal rejected: {ex.Message}");
}

// 5. reverse a transaction that doesn't exist: should be rejected
try
{
    ledger.Reverse(Guid.NewGuid(), Guid.NewGuid().ToString());
    Console.WriteLine("BUG: unknown transaction was not rejected");
}
catch (Exception ex)
{
    Console.WriteLine($"Unknown transaction rejected: {ex.Message}");
}

PrintBalances("Final   ");

// ---- helpers ----

static string Gbp(long pence)
{
    var sign = pence < 0 ? "-" : "";
    return $"{sign}£{Math.Abs(pence) / 100m:N2}";
}

string AccountName(Guid id) => ledger.GetAccountById(id)?.Name ?? "unknown";

void PrintTransfer(string description, Transaction tx)
{
    Console.WriteLine();
    Console.WriteLine($"{description}  (tx {tx.Id.ToString()[..8]})");
    foreach (var entry in tx.Entries)
    {
        Console.WriteLine($"  {entry.Direction,-7} {AccountName(entry.AccountId),-18} {Gbp(entry.Pence),10}");
    }
}
