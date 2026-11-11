Stack law: /rancho-modular-dotnet:law

## Reference

- Aggregate: src/Modules/Catalog/Catalog.Domain/Products/
- Command: src/Modules/Catalog/Catalog.Application/CreateProduct/
- Command with an outcome: src/Modules/Catalog/Catalog.Application/RenameProduct/
- Query: src/Modules/Catalog/Catalog.Application/GetProduct/ with its handler in src/Modules/Catalog/Catalog.Infrastructure/Queries/GetProduct/
- Endpoint: src/Api/Endpoints/Catalog/
- Module registration: src/Modules/Catalog/Catalog.Infrastructure/CatalogModule.cs
- Host test: tests/Api.Tests/PipelineTests.cs

## Verification

```bash
dotnet build Starter.slnx
```

```bash
dotnet test --solution Starter.slnx
```

A failing build or test is the law speaking: fix the code, not the check. The project reference table is `build/ProjectKinds.targets`; every project declares its `<ProjectKind>`.
