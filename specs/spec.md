# Gerador de Senhas Fortes

## Problema
Pessoas que precisam de uma senha forte para um uso temporário ou didático precisam gerar uma senha que atenda a requisitos mínimos e recuperá-la mais tarde. Sem um identificador associado à senha, não há uma forma simples de consultá-la posteriormente.

## Objetivo
Oferecer uma API que gere senhas com os requisitos definidos neste documento, armazene cada senha gerada e permita recuperá-la por meio de um identificador único.

## Usuários
- Consumidores da API que precisam gerar uma senha e guardar o identificador para consultá-la depois.
- Desenvolvedores e estudantes que usam o projeto para aprender sobre desenvolvimento orientado a especificações.

## Histórias
- Como consumidor da API, quero gerar uma senha forte e receber seu identificador, para poder consultá-la posteriormente.
- Como consumidor da API, quero consultar uma senha informando seu identificador, para recuperar a senha gerada anteriormente.
- Como consumidor da API, quero receber um erro claro quando informar um identificador inválido ou inexistente, para entender por que a consulta não foi concluída.

## Requisitos funcionais
- **RF-01 — Gerar senha:** a API deve oferecer uma operação para gerar uma senha que cumpra todas as regras de negócio deste documento.
- **RF-02 — Persistir senha:** após gerar uma senha, a API deve armazená-la e associá-la a um identificador único do tipo GUID.
- **RF-03 — Retornar identificador:** após gerar e armazenar uma senha com sucesso, a resposta deve disponibilizar o GUID associado e a senha gerada.
- **RF-04 — Consultar senha:** a API deve oferecer uma operação que receba um GUID e retorne a senha associada quando ela existir.
- **RF-05 — Tratar GUID inválido:** se o valor informado não puder ser interpretado como GUID, a API deve responder com erro de validação HTTP 400 no formato ProblemDetails.
- **RF-06 — Tratar senha inexistente:** se o GUID for válido, mas não estiver associado a uma senha armazenada, a API deve responder com HTTP 404.

## Regras de negócio
- **RN-01:** toda senha gerada deve conter pelo menos 16 caracteres.
- **RN-02:** toda senha gerada deve conter pelo menos um caractere especial.
- **RN-03:** senhas geradas não podem conter espaços em branco.
- **RN-04:** cada senha gerada com sucesso deve ser armazenada e associada a um GUID único.
- **RN-05:** uma consulta por GUID deve retornar a senha previamente armazenada e associada exatamente àquele GUID.

## Casos de borda
- Um GUID bem-formado sem senha associada deve resultar em HTTP 404, não em uma senha vazia ou gerada novamente.
- Um valor que não seja um GUID válido deve resultar em HTTP 400 com ProblemDetails.
- A operação de geração não deve retornar sucesso nem um GUID se a senha não puder ser armazenada.
- Cada nova geração deve receber um identificador próprio, sem substituir a senha de outra geração.

## Fora de escopo
- Autenticação, autorização, limitação de requisições e outros mecanismos de proteção para produção.
- Criptografia ou hash das senhas armazenadas.
- Alterar, excluir, expirar ou listar senhas armazenadas.
- Permitir que o consumidor escolha o tamanho ou a composição da senha.
- Disponibilizar interface gráfica ou outros canais além da API.
- Uso em produção. Este projeto é exclusivamente didático e não deve ser usado para armazenar senhas reais.

## Critérios de aceite
- **CA-01 — Geração:** ao solicitar uma senha, a API retorna sucesso, a senha retornada tem pelo menos 16 caracteres, contém ao menos um caractere especial e não contém espaços em branco; a resposta inclui um GUID associado.
- **CA-02 — Persistência e consulta:** após uma geração bem-sucedida, consultar o GUID retornado recupera a mesma senha gerada.
- **CA-03 — Identificadores distintos:** duas gerações bem-sucedidas distintas retornam GUIDs distintos e cada GUID recupera sua própria senha.
- **CA-04 — GUID inválido:** consultar com um valor que não seja GUID retorna HTTP 400 com corpo ProblemDetails.
- **CA-05 — GUID inexistente:** consultar com um GUID válido sem senha associada retorna HTTP 404.
