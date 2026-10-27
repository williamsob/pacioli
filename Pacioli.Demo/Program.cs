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
Console.WriteLine($"key={transfer.idempotencyKey}  hash={transfer.requestHash}");
PrintTransfer("Caravaggio pays DaVinci", transfer2);
Console.WriteLine($"key={transfer2.idempotencyKey}  hash={transfer2.requestHash}");

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
