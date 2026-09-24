Funcionalidade: Logout
  Para encerrar a sessão do usuário
  Como sistema
  Quero invalidar o refresh token no logout

Cenário: Logout com refresh token válido
  Dado que existe um refresh token válido
  Quando o logout é realizado
  Então o refresh token deve ser revogado

Cenário: Logout com refresh token inexistente
  Dado que não existe refresh token
  Quando o logout é realizado
  Então nenhuma revogação deve ocorrer
