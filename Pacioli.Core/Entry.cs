using System.Security.Cryptography.X509Certificates;

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

        public Entry(Guid accountID, long pence, string currency, Direction direction)
        {
            Id = Guid.NewGuid();
            AccountId = accountID;
            Pence = pence;
            Currency = currency;
            Direction = direction;
        }
    }
}