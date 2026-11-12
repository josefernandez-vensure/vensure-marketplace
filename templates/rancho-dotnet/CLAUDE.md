Stack law: /rancho-modular-dotnet:law

## Reference

- Aggregate: src/Modules/Catalog/Catalog.Domain/Products/
- Command: src/Modules/Catalog/Catalog.Application/CreateProduct/
- Command with an outcome: src/Modules/Catalog/Catalog.Application/RenameProduct/
- Command with an idempotency key: src/Modules/Catalog/Catalog.Application/ReceiveStock/
- Query: src/Modules/Catalog/Catalog.Application/GetProduct/ with its handler in src/Modules/Catalog/Catalog.Infrastructure/Queries/GetProduct/
- Paged query: src/Modules/Catalog/Catalog.Application/ListProducts/ with its handler and keyless read type in src/Modules/Catalog/Catalog.Infrastructure/Queries/ListProducts/
- Endpoint: src/Api/Endpoints/Catalog/
- Data class: src/Modules/Catalog/Catalog.Domain/Products/ProductName.cs and its twin src/Modules/Catalog/Catalog.Contracts/ProductName.cs; a member projected from it in src/Modules/Catalog/Catalog.Application/GetProduct/GetProduct.cs
- Logging method: src/Platform/Platform.Application/Behaviors/DispatchTelemetry.cs
- OpenAPI document: openapi/v1.json, generated through src/Api/OpenApi/ContractDocument.cs
- Module context and registration: src/Modules/Catalog/Catalog.Infrastructure/Persistence/CatalogDbContext.cs and src/Modules/Catalog/Catalog.Infrastructure/CatalogModule.cs
- Host test: tests/Api.Tests/PipelineTests.cs
- Integration test: tests/Catalog.IntegrationTests/IdempotencyTests.cs

## Verification

```bash
dotnet build Starter.slnx
```

```bash
dotnet test --solution Starter.slnx
```

The integration tests need Docker running. A failing build or test is the law speaking: fix the code, not the check. The project reference table is `build/ProjectKinds.targets`; every project declares its `<ProjectKind>`.
