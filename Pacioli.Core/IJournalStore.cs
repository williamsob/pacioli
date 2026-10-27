namespace Pacioli.Core
{

	public interface IJournalStore
	{
		void AddAccount(Account account);
		Account? GetAccount(Guid id);
		Account? GetAccountByExternalId(string externalId);
		Transaction Append(Transaction transaction);
		IEnumerable<Entry> GetEntries(Guid accountId);
	}
}
