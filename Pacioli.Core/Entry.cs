namespace Pacioli.Core
{

    public class Entry
    {
        public Guid Id { get; }
        public Guid AccountId { get; }
        public long Pence { get; }
        public string Currency { get; }
        public Direction Direction { get; }
        public long SignedPence => Direction == Direction.Debit ? Pence : -Pence;

        public Entry(Guid accountId, long pence, string currency, Direction direction)
        {
            if (pence <= 0)
                throw new ArgumentOutOfRangeException(nameof(pence), "Amount must be positive");
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be null or whitespace", nameof(currency));
            if (!Enum.IsDefined(direction))
                throw new ArgumentException("Direction must be Debit or Credit", nameof(direction));

            Id = Guid.NewGuid();
            AccountId = accountId;
            Pence = pence;
            Currency = currency.ToUpperInvariant();
            Direction = direction;
        }
    }
}