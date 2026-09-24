using FCG.Application.Identidade.DTOs;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Repositories;
using Moq;

namespace FCG.BDDTests.Identidade.CriarUsuario
{
	public sealed class CriarUsuarioContext
	{
		public CriarUsuarioRequest? Request { get; set; }
		public Guid? UsuarioId { get; set; }
		public Exception? Excecao { get; set; }
		public Usuario? UsuarioPersistido { get; set; }
	}
}
