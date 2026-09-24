using FCG.Domain.Shared.UoW;

namespace FCG.BDDTests.Support
{
	public sealed class FakeUnitOfWork : IUnitOfWork
	{
		public Task<bool> Commit() => Task.FromResult(true);
	}
}
