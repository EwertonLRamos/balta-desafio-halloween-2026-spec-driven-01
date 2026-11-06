# Gerador de Senhas Fortes

## Problema
Pessoas que precisam de uma senha para um uso didático precisam gerar uma senha que atenda a requisitos mínimos e recuperá-la mais tarde. Sem um identificador associado à senha, não há uma forma simples de consultá-la posteriormente.

## Objetivo
Oferecer uma API com duas operações: gerar e armazenar uma senha conforme os requisitos deste documento, e recuperar essa senha usando o GUID associado.

## Usuários
- Consumidores da API que precisam gerar uma senha e guardar o identificador para consultá-la depois.
- Desenvolvedores e estudantes que usam o projeto para aprender sobre desenvolvimento orientado a especificações.

## Histórias
- Como consumidor da API, quero gerar uma senha que cumpra os requisitos deste documento e receber seu GUID, para poder consultá-la posteriormente.
- Como consumidor da API, quero consultar uma senha informando o GUID recebido na geração, para recuperar a senha gerada anteriormente.
- Como consumidor da API, quero receber respostas diferentes para um GUID malformado e para um GUID válido sem senha associada, para entender por que a consulta não foi concluída.

## Requisitos funcionais
- **RF-01 — Gerar senha:** a primeira operação não recebe parâmetros do consumidor e gera uma senha que cumpra todas as regras de negócio deste documento.
- **RF-02 — Persistir senha:** a senha gerada deve ser armazenada e associada a um GUID.
- **RF-03 — Responder à geração:** somente após o armazenamento bem-sucedido, a operação deve retornar HTTP 200 e incluir na resposta tanto a senha gerada quanto o GUID associado. O GUID deve usar o formato textual `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`, com dígitos hexadecimais em minúsculas.
- **RF-04 — Consultar senha:** a segunda operação recebe um GUID no formato textual definido em RF-03, aceitando letras hexadecimais maiúsculas ou minúsculas. Se ele estiver associado a uma senha armazenada, a operação retorna HTTP 200 e essa senha.
- **RF-05 — Tratar GUID malformado:** se o valor recebido não corresponder ao formato textual definido em RF-03, ignorando apenas a diferença entre letras hexadecimais maiúsculas e minúsculas, a operação de consulta retorna HTTP 400 com corpo ProblemDetails.
- **RF-06 — Tratar GUID sem associação:** se o valor recebido for um GUID válido, mas não estiver associado a uma senha armazenada, a operação de consulta retorna HTTP 404.

## Regras de negócio
- **RN-01 — Alfabeto:** a senha gerada deve conter somente caracteres ASCII imprimíveis entre `!` (U+0021) e `~` (U+007E), inclusive. Esse conjunto contém letras, algarismos e sinais de pontuação, mas não contém espaços nem outros caracteres de espaço em branco.
- **RN-02 — Tamanho:** a senha gerada deve conter pelo menos 16 caracteres.
- **RN-03 — Caractere especial:** toda senha gerada deve conter pelo menos um caractere permitido por RN-01 que não seja letra (`A-Z` ou `a-z`) nem algarismo (`0-9`); esse caractere é considerado especial nesta especificação.
- **RN-04 — Associação única:** cada geração bem-sucedida deve criar uma associação entre a senha e um GUID que não esteja associado a outra senha armazenada. Uma nova geração não pode substituir uma associação existente.
- **RN-05 — Consulta exata:** consultar um GUID associado deve retornar a senha armazenada nessa associação, sem gerar ou alterar senhas.

## Casos de borda
- Um valor que não possa ser interpretado como GUID deve resultar em HTTP 400 com ProblemDetails.
- Um GUID válido sem associação deve resultar em HTTP 404; a API não deve retornar uma senha vazia nem gerar outra senha para essa consulta.
- Se a senha não puder ser armazenada, a operação de geração não deve retornar sucesso nem informar um GUID como se a senha estivesse disponível para consulta.
- Uma nova geração deve criar uma associação própria, mesmo que o conteúdo da senha gerada seja igual ao de outra senha.

## Fora de escopo
- Autenticação, autorização, limitação de requisições e outros mecanismos de proteção para produção.
- Criptografia ou hash das senhas armazenadas.
- Alterar, excluir, expirar ou listar senhas armazenadas.
- Permitir que o consumidor escolha o tamanho ou a composição da senha.
- Disponibilizar interface gráfica ou outros canais além da API.
- Uso em produção. Este projeto é exclusivamente didático e não deve ser usado para armazenar senhas reais.

## Critérios de aceite
- **CA-01 — Geração:** ao solicitar uma geração, a resposta de sucesso contém uma senha e seu GUID. A senha tem pelo menos 16 caracteres, contém somente caracteres permitidos por RN-01, contém ao menos um caractere especial conforme RN-03 e não contém espaços em branco.
- **CA-02 — Persistência e consulta:** depois de uma geração bem-sucedida, consultar o GUID retornado recupera exatamente a senha informada na resposta da geração.
- **CA-03 — Associações independentes:** duas gerações bem-sucedidas criam GUIDs distintos; consultar cada GUID retorna a senha da respectiva geração, ainda que as senhas tenham o mesmo conteúdo.
- **CA-04 — GUID malformado:** consultar com um valor que não possa ser interpretado como GUID retorna HTTP 400 com corpo ProblemDetails.
- **CA-05 — GUID válido sem associação:** consultar com um GUID válido sem senha associada retorna HTTP 404 e não gera uma senha.
- **CA-06 — Falha ao armazenar:** se a senha não for armazenada, a operação de geração não retorna uma resposta de sucesso nem um GUID utilizável para consulta.
