namespace Pacioli.Core
{
    public class Account
    {
        public Guid id { get; }
        public string name { get; }
        public string currency { get; }

        public Account(Guid id, string name, string currency)
        {
            this.id = Guid.NewGuid();
            this.name = name;
            this.currency = currency;
        }
    }
}
