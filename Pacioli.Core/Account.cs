namespace Pacioli.Core
{
    public class Account
    {
        public Guid Id { get; }
        public string ExternalId { get; }
        public string Name { get; }
        public string Currency { get; }

        public Account(string externalId, string name, string currency)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Account name cannot be null or whitespace.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new ArgumentException("External ID cannot be null or whitespace.", nameof(externalId));
            }
            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new ArgumentException("Currency cannot be null or whitespace.", nameof(currency));
            }

            Id = Guid.NewGuid();
            ExternalId = externalId;
            Name = name;
            Currency = currency.ToUpperInvariant();
        }
    }
}
