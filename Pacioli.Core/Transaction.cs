namespace Pacioli.Core
{

public class Transaction
{
    public Guid id { get; }
    public IReadOnlyList<Entry> entries { get; }
    public DateTimeOffset date { get; }

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


        this.id = Guid.NewGuid();
        this.entries = list.AsReadOnly();
        this.date = date;
    }
}

}
