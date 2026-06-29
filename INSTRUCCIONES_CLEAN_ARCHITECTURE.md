# Instrucciones de Arquitectura Clean Architecture

Este proyecto usara un unico proyecto API, organizado internamente siguiendo principios de Clean Architecture, CQRS, features y vertical slice architecture.

El objetivo es mantener una separacion clara entre reglas de negocio, casos de uso y detalles externos como base de datos, servicios externos, archivos, autenticacion o integraciones.

## Estructura General

La API debe organizarse en tres areas principales:

```text
Api/
  Domain/
  Application/
  Infrastructure/
```

Aunque exista un solo proyecto fisico, las dependencias logicas deben respetar esta direccion:

```text
Api -> Application -> Domain
Api -> Infrastructure -> Application / Domain
```

El dominio no debe depender de Application, Infrastructure ni de frameworks externos.

## Domain

La carpeta `Domain` contiene el modelo de negocio puro.

Debe incluir:

- Entidades.
- Objetos de valor.
- Enumeraciones de negocio.
- Reglas de dominio.
- Eventos de dominio, si son necesarios.
- Excepciones propias del dominio.

Ejemplo de estructura:

```text
Domain/
  Entities/
  ValueObjects/
  Enums/
  Events/
  Exceptions/
```

### Entidades

Las entidades representan conceptos principales del negocio con identidad propia.

Reglas:

- Deben proteger sus invariantes.
- No deben exponer setters publicos innecesarios.
- Deben tener metodos de comportamiento, no solo propiedades.
- No deben depender de Entity Framework, controladores, DTOs ni servicios externos.

Ejemplo:

```csharp
public sealed class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Money Price { get; private set; }

    private Product() { }

    public Product(string name, Money price)
    {
        Id = Guid.NewGuid();
        ChangeName(name);
        ChangePrice(price);
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del producto es requerido.");

        Name = name;
    }

    public void ChangePrice(Money price)
    {
        Price = price ?? throw new DomainException("El precio es requerido.");
    }
}
```

### Objetos de Valor

Los objetos de valor representan conceptos sin identidad propia.

Reglas:

- Deben ser inmutables.
- Deben validar sus propias reglas.
- Dos objetos de valor son iguales si sus valores son iguales.
- Son ideales para conceptos como dinero, email, direccion, rango de fechas, cantidades o codigos.

Ejemplo:

```csharp
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("El monto no puede ser negativo.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("La moneda es requerida.");

        Amount = amount;
        Currency = currency;
    }
}
```

## Application

La carpeta `Application` contiene los casos de uso del sistema.

Se usara CQRS basado en features y vertical slices.

Cada feature debe agrupar sus comandos, consultas, handlers, DTOs, validadores y contratos especificos.

Ejemplo de estructura:

```text
Application/
  Abstractions/
    Data/
    Messaging/
    Services/
  Features/
    Products/
      CreateProduct/
        CreateProductCommand.cs
        CreateProductCommandHandler.cs
        CreateProductRequest.cs
        CreateProductResponse.cs
        CreateProductValidator.cs
      GetProductById/
        GetProductByIdQuery.cs
        GetProductByIdQueryHandler.cs
        GetProductByIdResponse.cs
      UpdateProduct/
      DeleteProduct/
```

### CQRS

CQRS separa operaciones de escritura y lectura.

- Command: representa una accion que modifica estado.
- Query: representa una consulta que no modifica estado.
- Handler: ejecuta el caso de uso.

Reglas:

- Un command o query debe representar una intencion clara.
- Los handlers deben contener la orquestacion del caso de uso.
- Las reglas de negocio deben vivir en el dominio, no en el handler.
- Los handlers deben depender de abstracciones, no de implementaciones concretas.
- Cada feature debe ser independiente y facil de ubicar.

Ejemplo de command:

```csharp
public sealed record CreateProductCommand(
    string Name,
    decimal Price,
    string Currency
);
```

Ejemplo de handler:

```csharp
public sealed class CreateProductCommandHandler
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var price = new Money(command.Price, command.Currency);
        var product = new Product(command.Name, price);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
```

### Vertical Slice

Cada caso de uso debe estar agrupado por funcionalidad, no por tipo tecnico.

Preferir:

```text
Features/
  Products/
    CreateProduct/
    GetProductById/
    UpdateProduct/
```

Evitar:

```text
Commands/
Queries/
Handlers/
Validators/
Dtos/
```

La razon es que una feature debe poder entenderse y modificarse desde una sola ubicacion.

### Abstracciones

Las interfaces que Application necesita para hablar con recursos externos deben vivir en `Application/Abstractions`.

Ejemplos:

```text
Application/
  Abstractions/
    Data/
      IApplicationDbContext.cs
    Services/
      IEmailSender.cs
      ICurrentUserService.cs
    Storage/
      IFileStorage.cs
```

Application define lo que necesita. Infrastructure implementa como se hace.

## Infrastructure

La carpeta `Infrastructure` contiene implementaciones concretas de responsabilidades externas.

Debe incluir:

- Acceso a datos.
- Entity Framework Core.
- Repositorios, si se usan.
- Configuraciones de persistencia.
- Servicios externos.
- Envio de correos.
- Consumo de APIs externas.
- Almacenamiento de archivos.
- Proveedores de fecha, usuario actual, tokens o autenticacion externa.

Ejemplo de estructura:

```text
Infrastructure/
  Data/
    ApplicationDbContext.cs
    Configurations/
    Migrations/
  ExternalServices/
    Email/
    Payments/
    Notifications/
  DependencyInjection.cs
```

### Data

La comunicacion con base de datos debe vivir en `Infrastructure/Data`.

Reglas:

- `ApplicationDbContext` debe implementar la abstraccion definida en Application.
- Las configuraciones de Entity Framework deben estar separadas por entidad.
- Las migraciones pertenecen a Infrastructure.
- Los detalles de persistencia no deben contaminar el dominio.

Ejemplo:

```csharp
public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

### APIs y servicios externos

Toda comunicacion con servicios externos debe vivir en Infrastructure.

Ejemplos:

- Clientes HTTP.
- SDKs de terceros.
- Servicios de correo.
- Servicios de pago.
- Almacenamiento en nube.
- Sistemas de mensajeria.

Application solo debe conocer interfaces, no implementaciones.

## Api

La raiz del proyecto API contiene la entrada de la aplicacion.

Debe incluir:

- Controllers o Minimal APIs.
- Configuracion de servicios.
- Middleware.
- Autenticacion y autorizacion.
- Validacion de requests.
- Registro de dependencias.
- Configuracion de Swagger/OpenAPI.

Ejemplo:

```text
Api/
  Controllers/
  Endpoints/
  Middleware/
  Program.cs
  appsettings.json
```

Los endpoints deben delegar el trabajo a Application. No deben contener reglas de negocio.

Ejemplo:

```csharp
app.MapPost("/products", async (
    CreateProductRequest request,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var command = new CreateProductCommand(
        request.Name,
        request.Price,
        request.Currency);

    var productId = await sender.Send(command, cancellationToken);

    return Results.Created($"/products/{productId}", new { id = productId });
});
```

## Registro de Dependencias

Separar el registro de servicios por area:

```text
Application/
  DependencyInjection.cs

Infrastructure/
  DependencyInjection.cs
```

Ejemplo en `Program.cs`:

```csharp
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
```

## Reglas de Dependencia

Reglas obligatorias:

- `Domain` no depende de ninguna otra capa.
- `Application` depende de `Domain`.
- `Infrastructure` puede depender de `Application` y `Domain`.
- `Api` puede depender de todas las capas internas porque es el punto de composicion.
- Los controladores o endpoints no deben acceder directamente a Entity Framework.
- Las reglas de negocio no deben estar en controllers, endpoints ni infraestructura.
- Los DTOs de entrada y salida no deben reemplazar entidades de dominio.
- Las integraciones externas deben estar detras de interfaces.

## Convenciones por Feature

Cada feature debe tener una carpeta propia.

Ejemplo:

```text
Features/
  Orders/
    CreateOrder/
      CreateOrderCommand.cs
      CreateOrderCommandHandler.cs
      CreateOrderRequest.cs
      CreateOrderResponse.cs
      CreateOrderValidator.cs
```

Convenciones:

- Commands terminan en `Command`.
- Queries terminan en `Query`.
- Handlers terminan en `Handler`.
- Requests representan entrada desde API.
- Responses representan salida hacia API.
- Validators validan datos de entrada antes de ejecutar el caso de uso.
- Las entidades de dominio no deben exponerse directamente como respuesta HTTP.

## Flujo Recomendado

Para crear una nueva funcionalidad:

1. Crear o actualizar entidades y objetos de valor en `Domain`.
2. Crear la feature en `Application/Features`.
3. Crear command o query.
4. Crear handler.
5. Crear validator si aplica.
6. Agregar o actualizar abstracciones si el caso de uso necesita recursos externos.
7. Implementar detalles externos en `Infrastructure`.
8. Exponer el endpoint en `Api`.
9. Agregar pruebas del dominio y del caso de uso.

## Principio Principal

La aplicacion debe depender de reglas de negocio estables, no de detalles externos.

El dominio expresa que hace el sistema.
Application expresa los casos de uso.
Infrastructure expresa como se conectan recursos externos.
Api expresa como los usuarios o clientes interactuan con el sistema.

## Soluciones Implementadas

### Entidad Usuario

Se agrego la entidad `Usuario` dentro de `Api/Domain/Entities`.

Campos definidos:

- `Id`
- `Nombre`
- `Apellido`
- `Email`

Reglas iniciales:

- `Id` se genera automaticamente al crear un usuario.
- `Nombre`, `Apellido` y `Email` son requeridos.
- `Nombre` y `Apellido` se guardan sin espacios al inicio o final.
- `Email` se guarda sin espacios y en minusculas.
- La entidad expone metodos de comportamiento para cambiar sus datos.

Ubicacion:

```text
Api/
  Domain/
    Entities/
      Usuario.cs
```

Codigo base:

```csharp
public sealed class Usuario
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public string Email { get; private set; }

    public Usuario(string nombre, string apellido, string email)
    {
        Id = Guid.NewGuid();
        CambiarNombre(nombre);
        CambiarApellido(apellido);
        CambiarEmail(email);
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del usuario es requerido.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void CambiarApellido(string apellido)
    {
        if (string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El apellido del usuario es requerido.", nameof(apellido));

        Apellido = apellido.Trim();
    }

    public void CambiarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es requerido.", nameof(email));

        Email = email.Trim().ToLowerInvariant();
    }
}
```

### Ajuste: Common para entidades y objetos de valor

Se agrego la carpeta `Api/Domain/Common` para centralizar clases base del dominio.

Archivos agregados:

- `Entity.cs`
- `ValueObject.cs`

Estructura actualizada:

```text
Api/
  Domain/
    Common/
      Entity.cs
      ValueObject.cs
    Entities/
      Usuario.cs
    ValueObjects/
```

Regla aplicada:

- Las entidades deben heredar de `Entity` cuando tengan identidad propia.
- Los objetos de valor deben heredar de `ValueObject` cuando necesiten igualdad por valores.
- `Usuario` ahora hereda de `Entity` y ya no declara manualmente la propiedad `Id`.

Ejemplo:

```csharp
public sealed class Usuario : Entity
{
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public string Email { get; private set; }

    public Usuario(string nombre, string apellido, string email)
        : base(Guid.NewGuid())
    {
        CambiarNombre(nombre);
        CambiarApellido(apellido);
        CambiarEmail(email);
    }
}
```

### Ajuste: Objetos de valor con Vogen

Los objetos de valor no se implementaran con una clase base manual `ValueObject`.

Decision actual:

- Los objetos de valor se representaran usando la libreria Vogen.
- La carpeta `Domain/Common` no debe contener una clase base `ValueObject` manual.
- `Entity` debe mantenerse solo para campos reutilizables de entidades, como `Id`.
- Las validaciones propias de cada objeto de valor deben declararse en el tipo generado con Vogen.

Estructura esperada:

```text
Api/
  Domain/
    Common/
      Entity.cs
    Entities/
      Usuario.cs
    ValueObjects/
      Email.cs
```

Ejemplo conceptual con Vogen:

```csharp
using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("El email es requerido.");

        if (!value.Contains('@'))
            return Validation.Invalid("El email no tiene un formato valido.");

        return Validation.Ok;
    }
}
```

Regla:

- Si un campo tiene reglas propias y significado de negocio, debe modelarse como value object con Vogen.
- Si un campo es solo un dato simple sin reglas relevantes, puede mantenerse como tipo primitivo.
- No crear objetos de valor manuales heredando de una clase base propia.

### Implementacion real de Vogen para Email

Se agrego el paquete NuGet `Vogen` al proyecto `Api/Api.csproj` usando:

```bash
dotnet add Api/Api.csproj package Vogen
```

Se creo el objeto de valor `Email` en:

```text
Api/Domain/ValueObjects/Email.cs
```

`Usuario.Email` dejo de ser `string` y ahora usa el value object `Email`.

Regla actual:

- La entidad recibe un `Email` ya validado.
- La validacion del formato basico y normalizacion del email vive en `Email` usando Vogen.
- `Usuario` mantiene reglas propias de entidad como nombre y apellido requeridos.

Uso esperado:

```csharp
var email = Email.From("USUARIO@correo.com");
var usuario = new Usuario("Juan", "Perez", email);
```

### Implementacion: endpoint de usuarios, EF Core y seed con Bogus

Se agrego infraestructura de datos usando Entity Framework Core y SQL Server, aprovechando el recurso `bd` definido en Aspire AppHost.

Paquetes agregados al proyecto `Api`:

```bash
dotnet add Api/Api.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add Api/Api.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add Api/Api.csproj package Bogus
```

Estructura agregada:

```text
Api/
  Application/
    Abstractions/
      Data/
        IUsuarioRepository.cs
    Features/
      Usuarios/
        CreateUsuario/
          CreateUsuario.cs
        GetUsuarioById/
          GetUsuarioById.cs
        GetUsuarios/
          GetUsuarios.cs
    DependencyInjection.cs
  Infrastructure/
    Data/
      ApplicationDbContext.cs
      Configurations/
        UsuarioConfiguration.cs
      Repositories/
        UsuarioRepository.cs
      Seed/
        DatabaseSeeder.cs
    DependencyInjection.cs
  Controllers/
    UsuariosController.cs
```

Endpoints agregados:

```text
GET  /api/usuarios
GET  /api/usuarios/{id}
POST /api/usuarios
```

Reglas aplicadas:

- La API delega en handlers de Application.
- Application depende de `IUsuarioRepository`, no de Entity Framework directamente.
- Infrastructure implementa `UsuarioRepository` con `ApplicationDbContext`.
- `Usuario.Email` se persiste como string usando conversion de EF Core hacia el value object `Email` de Vogen.
- `DatabaseSeeder` usa Bogus para crear 20 usuarios iniciales cuando la tabla esta vacia.
- El seed se ejecuta al iniciar la API.

### Ajuste: vertical slice en un solo archivo por feature

El proyecto usa controladores para exponer HTTP, pero cada caso de uso debe mantenerse como vertical slice.

Regla actual:

- Cada feature debe tener un solo archivo principal.
- En ese archivo deben vivir query o command, DTOs, mapeos, handler y validaciones.
- Los controladores solo traducen HTTP hacia la feature correspondiente.
- No separar una misma feature en carpetas tecnicas como `Requests`, `Responses`, `Handlers`, `Validators` o `Mappings`.

Estructura esperada:

```text
Application/
  Features/
    Usuarios/
      CreateUsuario/
        CreateUsuario.cs
      GetUsuarioById/
        GetUsuarioById.cs
      GetUsuarios/
        GetUsuarios.cs
```

Contenido esperado por archivo:

```text
CreateUsuario.cs
  CreateUsuarioRequest
  CreateUsuarioCommand
  CreateUsuarioResponse
  CreateUsuarioMapper
  CreateUsuarioValidator
  CreateUsuarioCommandHandler
```

Los controladores se mantienen en `Api/Controllers` porque este proyecto expone la API con controllers.
