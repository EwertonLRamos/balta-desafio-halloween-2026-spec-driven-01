# Constituição do projeto

## Stack
- A API será implementada em C# sobre .NET 10, usando Minimal API.
- A persistência será feita em SQLite com Entity Framework Core.
- Os testes automatizados serão implementados com xUnit.
- Uma dependência de produção ou de teste só poderá ser adicionada quando sua necessidade e justificativa estiverem registradas em `specs/plan.md`.

## Arquitetura
- Os endpoints HTTP não acessarão o banco de dados nem o `DbContext` diretamente; operações de aplicação serão delegadas a serviços de aplicação.
- Regras de negócio ficarão fora dos endpoints e de outros handlers HTTP.
- A camada HTTP será responsável por receber e validar a requisição, chamar o serviço de aplicação e traduzir o resultado para a resposta HTTP.
- A persistência será acessada por meio dos componentes definidos no plano técnico, sem transferir regras de negócio para essa camada.

## Qualidade
- Toda regra de negócio terá pelo menos um teste unitário que verifique seu comportamento.
- Todo endpoint terá pelo menos um teste de integração para o caminho feliz.
- Erros de validação serão respondidos com status HTTP 400 e corpo no formato ProblemDetails.
- A definição de pronto exige que a solução compile sem erros ou warnings, que todos os testes passem e que a aplicação inicie sem erros.

## Convenções
- Nomes de tipos, métodos e membros seguirão as convenções idiomáticas de C#; nomes de arquivos e tipos serão consistentes.
- Contratos HTTP e modelos de persistência não serão usados como substitutos dos modelos internos da aplicação quando isso acoplar indevidamente as camadas.
- Alterações de comportamento serão refletidas nos artefatos SDD relacionados antes da implementação: especificação para requisitos, plano para decisões técnicas e tarefas para o trabalho executável.

## Governança
- Esta constituição se aplica a toda funcionalidade e alteração do projeto.
- Uma decisão que desvie destes princípios deverá registrar a justificativa e o impacto em `specs/plan.md`.
- O projeto é exclusivamente didático e não deverá ser usado em produção; não representa uma solução de armazenamento seguro de senhas.
