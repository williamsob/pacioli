namespace Pacioli.Core
{
    public class Account
    {
        public Guid id { get; }
        public string external_id { get; }
        public string name { get; }
        public string currency { get; }

        public Account(string external_id, string name, string currency)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Account name cannot be null or whitespace.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(external_id))
            {
                throw new ArgumentException("External ID cannot be null or whitespace.", nameof(external_id));
            }
            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new ArgumentException("Currency cannot be null or whitespace.", nameof(currency));
            }

            id = Guid.NewGuid();
            this.external_id = external_id;
            this.name = name;
            this.currency = currency;
        }
    }
}
