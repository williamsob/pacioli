namespace Pacioli.Core
{
    public class InMemoryJournalStore : IJournalStore
    {
        private readonly object _sync = new();
        private readonly Dictionary<Guid, Account> _accountsById = new();
        private readonly Dictionary<string, Account> _accountsByExternalId = new();
        private readonly List<Transaction> _transactions = new();
        private readonly Dictionary<string, Transaction> _transactionsByKey = new();

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

        public Transaction Append(Transaction transaction)
        { 
            lock (_sync)
            {
                if (_transactionsByKey.TryGetValue(transaction.idempotencyKey, out var existing))
                {
                    if (existing.requestHash != transaction.requestHash)
                        throw new InvalidOperationException("Idempotency key was already used with a different request.");
                    return existing;   // retry: return the original, write nothing


                }

                _transactions.Add(transaction);
                _transactionsByKey.Add(transaction.idempotencyKey, transaction);
                return transaction;
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
