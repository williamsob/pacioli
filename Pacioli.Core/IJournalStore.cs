namespace Pacioli.Core
{

	public interface IJournalStore
	{
		void AddAccount(Account account);
		Account? GetAccount(Guid id);
		Account? GetAccountByExternalId(string externalId);
		void Append(Transaction transaction);
		IEnumerable<Entry> GetEntries(Guid accountId);
	}
}
