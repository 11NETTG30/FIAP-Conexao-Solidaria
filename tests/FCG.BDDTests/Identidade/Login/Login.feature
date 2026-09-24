Funcionalidade: Login de usuário
  Para permitir autenticação no sistema
  Como usuário
  Quero realizar login com e-mail e senha válidos

Cenário: Login com credenciais válidas
  Dado que existe um usuário cadastrado para login com e-mail "gabriel@email.com" e senha válida
  Quando o login é realizado com e-mail "gabriel@email.com" e senha "123456"
  Então deve ser retornado um access token
  E deve ser gerado um refresh token

Cenário: Login com senha inválida
  Dado que existe um usuário cadastrado para login com e-mail "gabriel@email.com" e senha inválida
  Quando o login é realizado com e-mail "gabriel@email.com" e senha "123456"
  Então deve ocorrer um erro de autenticação

Cenário: Login com e-mail inexistente
  Dado que não existe usuário cadastrado para login com o e-mail "gabriel@email.com"
  Quando o login é realizado com e-mail "gabriel@email.com" e senha "123456"
  Então deve ocorrer um erro de autenticação
