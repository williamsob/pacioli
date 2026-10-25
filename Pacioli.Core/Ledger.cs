using System.ComponentModel.DataAnnotations;

namespace Pacioli.Core
{
    public class Ledger
    {
        private readonly IJournalStore _store;

        public Ledger(IJournalStore store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));
            _store = store;
        }

        public Account CreateAccount(string externalId, string name, string currency)
        {
            var account = new Account(externalId, name, currency);
            _store.AddAccount(account);
            return account;
        }

        public long GetBalance(string externalId)
        {
            var account = _store.GetAccountByExternalId(externalId);

            if (account == null)
                throw new InvalidOperationException("No account with label '{externalId}' was found.");

            long balance = 0;
            foreach (var entry in _store.GetEntries(account.Id))
            {
                balance += entry.SignedPence;
            }
            return balance;
        }
        public Transaction Transfer(string fromExternalId, string toExternalId, long pence)
        {
            if (pence <= 0)
                throw new ArgumentOutOfRangeException(nameof(pence), "Amount must be positive.");
            if (string.IsNullOrWhiteSpace(fromExternalId) || string.IsNullOrWhiteSpace(toExternalId))
                throw new ArgumentException("Both account labels are required.");
            if (fromExternalId == toExternalId)
                throw new ArgumentException("Sender and receiver must be different accounts.");

            var from = _store.GetAccountByExternalId(fromExternalId)
                ?? throw new InvalidOperationException($"No account with label '{fromExternalId}'.");
            var to = _store.GetAccountByExternalId(toExternalId)
                ?? throw new InvalidOperationException($"No account with label '{toExternalId}'.");

            if (from.Currency != to.Currency)
                throw new InvalidOperationException("Accounts must share a currency.");

            var entries = new List<Entry>
            {
            new Entry(from.Id, pence, from.Currency, Direction.Credit),
            new Entry(to.Id,   pence, to.Currency,   Direction.Debit)
            };

            var transaction = new Transaction(entries, DateTimeOffset.UtcNow);
            _store.Append(transaction);
            return transaction;
        }
    }
}
