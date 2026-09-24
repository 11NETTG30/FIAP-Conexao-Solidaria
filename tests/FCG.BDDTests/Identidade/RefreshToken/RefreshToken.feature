Funcionalidade: Renovação de token
  Para manter a sessão do usuário ativa com segurança
  Como sistema
  Quero renovar o access token usando um refresh token válido

Cenário: Renovar token com refresh token válido
  Dado que existe um refresh token válido para renovação
  E o usuário associado ao refresh token existe
  Quando a renovação do token é realizada
  Então deve ser retornado um novo access token
  E deve ser gerado um novo refresh token

Cenário: Renovar token com refresh token inexistente
  Dado que refresh token é inexistente
  Quando a renovação do token é realizada
  Então deve ocorrer um erro de autenticação ao renovar o token

Cenário: Renovar token com refresh token revogado
  Dado que existe um refresh token revogado
  Quando a renovação do token é realizada
  Então deve ocorrer um erro de autenticação ao renovar o token
