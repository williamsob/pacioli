namespace Pacioli.Core
{

    public class Entry
    {
        protected Guid id { get; }
        public string accountID { get; }
        public long pence { get; }
        public long currency { get; }
        public Direction direction { get; }

        public Entry(Guid id, string accountID, long pence, long currency, Direction direction)
        {
            this.id = Guid.NewGuid();
            this.accountID = accountID;
            this.pence = pence;
            this.currency = currency;
            this.direction = direction;
        }
    }
}