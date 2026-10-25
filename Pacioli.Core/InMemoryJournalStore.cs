namespace Pacioli.Core
{
    public class InMemoryJournalStore : IJournalStore
    {
        private readonly object _sync = new();
        private readonly Dictionary<Guid, Account> _accountsById = new();
        private readonly Dictionary<string, Account> _accountsByExternalId = new();
        private readonly List<Transaction> _transactions = new();

        public void AddAccount(Account account)
        {
            lock (_sync)
            {
                if (_accountsByExternalId.ContainsKey(account.ExternalId))
                    throw new InvalidOperationException("An account with that label already exists.");

                _accountsById.Add(account.Id, account);
                _accountsByExternalId.Add(account.ExternalId, account);
            }
        }

        public Account? GetAccount(Guid id)
        {
            lock (_sync)
            {
                _accountsById.TryGetValue(id, out var account);
                return account;
            }
        }

        public Account? GetAccountByExternalId(string externalId)
        {
            lock (_sync)
            {
                _accountsByExternalId.TryGetValue(externalId, out var account);
                return account;
            }
        }

        public void Append(Transaction transaction)
        {
            lock (_sync)
            {
                _transactions.Add(transaction);
            }
        }

        public IEnumerable<Entry> GetEntries(Guid accountId)
        {
            lock (_sync)
            {
                var result = new List<Entry>();

                foreach (Transaction transaction in _transactions)
                {
                    foreach (Entry entry in transaction.Entries)
                    {
                        if (entry.AccountId == accountId)
                            result.Add(entry);
                    }
                }

                return result;
            }
        }
    }
}
