namespace Pacioli.Core
{

public class Transaction
{
    public Guid id { get; }
    public Entry[] entries { get; }
    public DateTime date { get; }

    public Transaction(Guid id, Entry[] entries, DateTime date)
    {
        this.id = id;
        this.entries = entries;
        this.date = date;
    }
}

}
