# Certificados extras para o build da imagem

Qualquer `.crt` colocado nesta pasta é instalado como CA confiável durante o
`docker build` (ver estágio `build` do `Dockerfile` na raiz) antes do
`dotnet restore`. A pasta fica vazia por padrão — nesse caso a etapa é um
no-op e o build funciona normalmente.

Isso existe para builds atrás de um proxy corporativo/sandbox que reassina
TLS (ex.: o ambiente de execução remota do Claude Code, onde o tráfego HTTPS
de dentro dos containers passa por um proxy de egress e o `dotnet restore`
falha com `NU1301`/`UntrustedRoot` sem confiar nesse CA). O próprio
`e2e/smoke-test.sh` detecta esse cenário e copia o CA bundle do proxy pra cá
antes do build, removendo o arquivo ao final — nenhuma ação manual é
necessária.

Nunca commite um certificado real aqui (`*.crt` está no `.gitignore`).
