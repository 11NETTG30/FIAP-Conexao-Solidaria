Funcionalidade: Criar usuário
  Para permitir o cadastro de novos usuários
  Como sistema
  Quero criar um usuário quando os dados forem válidos

Cenário: Criar usuário com dados válidos
  Dado que não existe usuário cadastrado com o e-mail "gabriel@email.com"
  Quando o usuário é criado com nome "Gabriel", e-mail "gabriel@email.com" e senha "@Bc123456789"
  Então o usuário deve ser persistido
  E deve ser retornado um identificador válido

Cenário: Criar usuário com e-mail já existente
  Dado que já existe usuário cadastrado com o e-mail "gabriel@email.com"
  Quando o usuário é criado com nome "Gabriel", e-mail "gabriel@email.com" e senha "@Bc123456789"
  Então deve ocorrer um erro de conflito
