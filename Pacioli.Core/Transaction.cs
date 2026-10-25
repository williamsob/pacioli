namespace Pacioli.Core
{

public class Transaction
{
    public Guid Id { get; }
    public IReadOnlyList<Entry> Entries { get; }
    public DateTimeOffset Date { get; }

    public Transaction(IEnumerable<Entry> entries, DateTimeOffset date)
    {
            var list = entries.ToList();
            if (list.Count < 2)
            {
                throw new ArgumentException("A transaction needs two entries at least.");
            }
            if (list.Select(e => e.Currency).Distinct().Count() != 1)
            {
                throw new ArgumentException("All entries must share one currency");
            }

            long debits = list.Where(e => e.Direction == Direction.Debit).Sum(e => e.Pence);
            long credits = list.Where(e => e.Direction == Direction.Credit).Sum(e => e.Pence);

            if (debits != credits)
            {
                throw new InvalidOperationException("This transaction isn't balanced");
            }


        this.Id = Guid.NewGuid();
        this.Entries = list.AsReadOnly();
        this.Date = date;
    }
}

}
