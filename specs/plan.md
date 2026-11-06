# Plano técnico

## Contexto
Implementar a especificação em `specs/specV2.md`: uma API didática com uma operação de geração e armazenamento de senha e outra de consulta por GUID. A solução será pequena e executável localmente, sem pretensão de uso em produção.

O plano segue `specs/constitution.md`. A senha é armazenada em texto puro conforme o escopo didático definido na especificação; portanto, não deve ser usada com senhas reais. A geração usará fonte aleatória segura disponível na plataforma, sem que isso implique criptografia ou armazenamento seguro.

## Arquitetura
Manter a aplicação em um projeto de API e um projeto de testes, com separação por responsabilidade dentro da API:

- **HTTP:** Minimal API declara as duas rotas, valida o formato textual do GUID recebido, chama serviços de aplicação e converte os resultados para códigos HTTP e contratos de resposta. Não acessa `DbContext` nem banco diretamente.
- **Aplicação:** um serviço de aplicação coordena geração, persistência e consulta. Depende de uma abstração de persistência e de um gerador de senhas; não contém tipos ou lógica específicos de HTTP.
- **Geração de senha:** componente responsável por gerar senhas que cumpram RN-01 a RN-03. Usa o gerador criptográfico aleatório fornecido pelo .NET, sem dependência adicional.
- **Persistência:** implementação da abstração de persistência com EF Core e SQLite. O `DbContext` e as entidades de persistência permanecem nesta responsabilidade.
- **Composição:** o ponto de inicialização registra as implementações e configurações por injeção de dependência.
- **Testes:** projeto xUnit separado. Testes unitários cobrem cada regra de negócio: alfabeto, tamanho e caractere especial do gerador; criação de associação sem sobrescrita; e consulta que devolve exatamente o valor associado. Testes de integração verificam o caminho feliz dos dois endpoints, além de GUID malformado, GUID sem associação e falha de persistência.

## Decisões
- **.NET 10, C# e Minimal API:** adotados conforme a stack recomendada no desafio; atendem à API pequena de duas operações sem adicionar uma camada de framework de controladores.
- **SQLite com EF Core:** SQLite persiste as senhas entre execuções sem exigir um servidor externo, e EF Core fornece o mapeamento e as operações de dados. O caminho da base será configurável, com `passwords.db` como padrão local.
- **xUnit:** adotado conforme a recomendação do desafio para testes unitários e de integração.
- **Dependências adicionais:** usar `Microsoft.EntityFrameworkCore.Sqlite` para persistir dados em SQLite; `Microsoft.EntityFrameworkCore.Design` somente no projeto necessário para gerar migrações; `Microsoft.AspNetCore.Mvc.Testing` para hospedar a API nos testes de integração; e xUnit, seu runner e `Microsoft.NET.Test.Sdk` para executar a suíte de testes. Não adicionar bibliotecas de geração de senha, validação, mapeamento ou acesso a dados sem justificativa registrada neste plano.
- **Aleatoriedade e tamanho:** gerar exatamente 16 caracteres por senha, atendendo ao mínimo de RN-02 sem criar uma opção de tamanho que não existe na especificação. Selecionar caracteres usando `RandomNumberGenerator.GetInt32`, disponível no .NET, evitando viés de módulo e dependência de terceiros. Gerar os 16 caracteres a partir do alfabeto de RN-01; se o resultado não tiver caractere especial conforme RN-03, substituir uma posição escolhida aleatoriamente por um sinal de pontuação permitido. Não se exigirá presença de letras maiúsculas, minúsculas ou algarismos, pois isso não consta na especificação.
- **Rotas:** `POST /api/passwords` gera uma senha sem parâmetros; `GET /api/passwords/{id}` consulta uma senha. O POST retorna HTTP 200 conforme RF-03, em vez de escolher outro código de sucesso.
- **GUID:** a geração serializa o GUID em formato `D` (hifens, 32 dígitos hexadecimais em minúsculas). A consulta valida o texto no formato `D`, aceitando hexadecimais em maiúsculas ou minúsculas. A rota não restringe o parâmetro a GUID para que valores malformados possam receber HTTP 400 com ProblemDetails em vez de serem tratados como rota inexistente.
- **Falha de persistência:** não retornar sucesso quando a gravação falhar. A falha inesperada deve resultar em HTTP 500 com ProblemDetails genérico, sem detalhes internos; não será convertida em 404 nem em resposta de sucesso.
- **Banco e esquema:** usar migrações do EF Core para criar e evoluir o esquema; aplicar as migrações ao iniciar a aplicação. Não usar banco em memória na aplicação, pois isso não atenderia à persistência requerida.
- **Erros HTTP:** configurar tratamento global de exceções para retornar HTTP 500 com ProblemDetails genérico em falhas inesperadas, sem expor detalhes internos. As respostas 400 e 404 previstas na especificação serão mapeadas explicitamente.
- **Integração de testes:** executar a API em testes com uma base SQLite isolada por teste ou conjunto de testes, para validar comportamento com o mesmo provedor de persistência usado pela aplicação sem afetar o arquivo local.

## Modelo de dados
Uma entidade de senha armazenada com:

| Campo | Tipo lógico | Restrições e finalidade |
| --- | --- | --- |
| `Id` | GUID | Chave primária e identificador usado na consulta. |
| `Value` | Texto | Senha gerada, armazenada sem hash ou criptografia conforme o escopo didático. |

Cada geração cria uma nova entidade. A chave primária impede que um GUID já associado seja substituído por outra senha. Não incluir data de criação, expiração ou outros campos, pois não são necessários aos requisitos atuais.

## Contratos
### Gerar senha

- **Requisição:** `POST /api/passwords`, sem corpo e sem parâmetros.
- **Sucesso:** HTTP 200, JSON `{"id":"xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx","password":"..."}`. `id` contém o GUID em formato `D`, em minúsculas; `password` contém exatamente a senha armazenada.
- **Falha de persistência ou falha interna inesperada:** HTTP 500 com ProblemDetails genérico.

### Consultar senha

- **Requisição:** `GET /api/passwords/{id}`, em que `id` deve conter um GUID no formato `D`; letras hexadecimais maiúsculas e minúsculas são aceitas.
- **Sucesso:** HTTP 200, JSON `{"password":"..."}` com a senha associada exatamente ao GUID consultado.
- **GUID malformado:** HTTP 400 com corpo ProblemDetails.
- **GUID válido sem associação:** HTTP 404.

Os nomes dos campos JSON são estáveis e em camelCase. As respostas de sucesso não incluem campos além dos definidos acima.

## Riscos
- **Armazenamento em texto puro:** quem tiver acesso ao arquivo SQLite poderá ler as senhas. É uma limitação intencional do exercício; documentar que a aplicação não é para produção e não armazenar credenciais reais.
- **Arquivo SQLite e migrações:** falha no caminho ou na aplicação das migrações impede a inicialização ou gravação. Manter a conexão configurável e cobrir persistência e consulta em testes de integração.
- **Aleatoriedade e regras:** alterações no alfabeto ou no algoritmo podem violar RN-01 a RN-03. Cobrir os limites e a composição com testes unitários, sem testar valores aleatórios exatos. Testar também, no serviço de aplicação, que cada geração cria uma associação sem substituir outras e que a consulta retorna o valor exato associado.
- **Falhas internas:** indisponibilidade ou erro de persistência não pode produzir resposta de sucesso enganosa. Mapear falhas inesperadas para HTTP 500 genérico e preservar detalhes apenas nos registros internos da aplicação.
- **Escopo de segurança limitado:** não há autenticação, autorização, criptografia, hash, limitação de requisições nem garantias operacionais de produção, conforme o fora de escopo da especificação.