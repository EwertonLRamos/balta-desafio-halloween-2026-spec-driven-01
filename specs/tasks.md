# Tarefas

| ID | Tarefa | Origem | Depende de | Concluída quando |
|---|---|---|---|---|
| T-01 | Criar solução e projeto da API | Constituição; Plano — Arquitetura e Decisões | | A solução contém o projeto da API em .NET 10 com Minimal API e compila sem erros ou warnings |
| T-02 | Criar projeto de testes e configurar dependências | Constituição; Plano — Decisões | T-01 | O projeto xUnit referencia a API, contém as dependências justificadas no plano e executa um teste inicial com sucesso |
| T-03 | Configurar persistência SQLite e Entity Framework Core | RF-02; Plano — Decisões | T-01 | A aplicação configura o provedor SQLite, usa `passwords.db` como caminho padrão configurável e compila |
| T-04 | Criar modelo de senha e migração inicial | RF-02; RN-04; Plano — Modelo de dados e Banco e esquema | T-03 | O modelo persiste GUID como chave primária e senha como texto, e a migração cria o esquema correspondente |
| T-05 | Implementar gerador de senhas e testes unitários | RN-01, RN-02, RN-03; CA-01; Plano — Aleatoriedade e tamanho | T-02 | Testes unitários verificam alfabeto permitido, mínimo de 16 caracteres e presença de ao menos um caractere especial |
| T-06 | Implementar persistência de senhas | RF-02; RN-04; Plano — Arquitetura e Modelo de dados | T-04 | A camada de persistência grava e consulta senhas por GUID sem permitir substituir uma associação existente |
| T-07 | Implementar serviço de aplicação para gerar e consultar senhas | RF-01 a RF-04; RN-04, RN-05; CA-02, CA-03 | T-05, T-06 | Testes unitários verificam geração e armazenamento, GUIDs distintos, consulta exata e resultado de consulta sem associação |
| T-08 | Implementar endpoints e contratos HTTP | RF-03 a RF-06; CA-01, CA-04, CA-05; Plano — Contratos e Erros HTTP | T-07 | `POST /api/passwords` e `GET /api/passwords/{id}` respeitam os contratos, retornam 400 com ProblemDetails para GUID malformado, 404 para GUID válido sem associação e 500 com ProblemDetails genérico para falhas inesperadas |
| T-09 | Configurar inicialização do banco e testes de integração | RF-02; CA-01 a CA-06; Plano — Banco e esquema e Integração de testes | T-02, T-03, T-08 | A aplicação aplica migrações ao iniciar e testes de integração com SQLite isolado cobrem o caminho feliz de ambos os endpoints, GUID malformado, GUID sem associação e falha de persistência |
| T-10 | Validar a entrega completa | Constituição — Qualidade; Definição de pronto do desafio | T-09 | A solução compila sem erros ou warnings, todos os testes passam e a aplicação inicia sem erros |
