using System.Security.Cryptography;
using System.Text;
using System.Globalization;

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

        public Account? GetAccount(string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("Account label is required.", nameof(externalId));

            return _store.GetAccountByExternalId(externalId);
        }
        public Account? GetAccountById(Guid id) => _store.GetAccount(id);

        public long GetBalance(string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("Account label is required.", nameof(externalId));
            
            var account = _store.GetAccountByExternalId(externalId)
                ?? throw new InvalidOperationException($"No account with label '{externalId} was found");

            long balance = 0;
            foreach (var entry in _store.GetEntries(account.Id))
            {
                balance += entry.SignedPence;
            }
            return balance;
        }
        public Transaction Transfer(string fromExternalId, string toExternalId, long pence, string idempotencyKey)
        {
            if (pence <= 0)
                throw new ArgumentOutOfRangeException(nameof(pence), "Amount must be positive.");
            if (string.IsNullOrWhiteSpace(fromExternalId) || string.IsNullOrWhiteSpace(toExternalId))
                throw new ArgumentException("Both account labels are required.");
            if (fromExternalId == toExternalId)
                throw new ArgumentException("Sender and receiver must be different accounts.");
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new ArgumentException("Idempotency key needed");
            var from = _store.GetAccountByExternalId(fromExternalId)
                ?? throw new InvalidOperationException($"No account with label '{fromExternalId}'.");
            var to = _store.GetAccountByExternalId(toExternalId)
                ?? throw new InvalidOperationException($"No account with label '{toExternalId}'.");

            if (from.Currency != to.Currency)
                throw new InvalidOperationException("Accounts must share a currency.");

            var requestHash = HashRequest("transfer", fromExternalId, toExternalId, pence.ToString(CultureInfo.InvariantCulture));
            var entries = new List<Entry>
            {
            new Entry(from.Id, pence, from.Currency, Direction.Credit),
            new Entry(to.Id,   pence, to.Currency,   Direction.Debit)
            };

            var transaction = new Transaction(entries, DateTimeOffset.UtcNow, idempotencyKey, requestHash);
            return _store.Append(transaction);
        }
            public Transaction Reverse(Guid transactionId, string idempotencyKey)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new ArgumentException("Idempotency key needed");

            var original = _store.GetTransaction(transactionId)
                ?? throw new InvalidOperationException("No transaction with this ID has been found");

            if (original.ReverseTransactionId.HasValue)
                throw new InvalidOperationException("A reversal can't itself be reversed. Make a new transfer instead.");

            var mirrorEntries = new List<Entry>();
            foreach (var entry in original.Entries)
            {
                mirrorEntries.Add(new Entry(entry.AccountId, entry.Pence, entry.Currency, Opposite(entry.Direction)));
            }

            var requestHash = HashRequest("reverse", transactionId.ToString());

            var reversal = new Transaction(
                mirrorEntries,
                DateTimeOffset.UtcNow,
                idempotencyKey,
                requestHash,
                reverseId: transactionId);

            return _store.Append(reversal);
        }

        private static Direction Opposite(Direction direction) =>
            direction == Direction.Debit ? Direction.Credit : Direction.Debit;

        private static string HashRequest(params string[] parts)
        {
            var text = string.Concat(parts.Select(part => $"{part.Length}:{part};"));
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        }


    }
}
