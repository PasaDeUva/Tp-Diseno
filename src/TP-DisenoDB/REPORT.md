# Informe Técnico - Sistema de Registro de Pagos de Tarjetas

Este proyecto implementa un sistema de gestión de pagos de tarjetas con dos opciones de persistencia: **Relacional (MySQL con EF Core)** y **No Relacional (MongoDB)**.

## Arquitectura y Decisiones de Mapeo

### 1. Utilización de Relaciones Bidireccionales
Se han utilizado relaciones bidireccionales en el modelo de Entity Framework (MySQL) para facilitar la navegación entre entidades (ej: `Bank.Cards` y `Card.Bank`). Esto permite realizar consultas más naturales y legibles, aunque requiere cuidado para evitar ciclos infinitos en la serialización JSON (resuelto mediante el uso de DTOs o configuraciones de serialización).

### 2. Carga de Objetos bajo Demanda (Lazy Loading)
Se ha configurado el uso de **Lazy Loading Proxies** en Entity Framework. Esto permite que las colecciones relacionadas (como las cuotas de una compra) se carguen automáticamente cuando se accede a ellas, simplificando la lógica de negocio pero debiendo ser monitoreado para evitar el problema de consultas N+1.

### 3. Operaciones en Cascada
Se han definido operaciones en cascada para mantener la integridad referencial. Por ejemplo, al eliminar un banco, sus promociones asociadas pueden ser eliminadas (según la regla de negocio). En MongoDB, dado que las promociones pueden estar embebidas o referenciadas, se ha optado por un manejo programático para asegurar la consistencia.

### 4. Embeber Objetos o Utilización de Referencias
- **MySQL:** Se utiliza el modelo relacional puro con claves foráneas.
- **MongoDB:**
  - Se han utilizado **referencias** para entidades principales como `Bank`, `CardHolder` y `Card`.
  - Se ha evaluado **embeber** las `Quotas` dentro de `MonthlyPayments` o la descripción de los items en el `Payment` para optimizar las consultas de reportes mensuales, reduciendo la necesidad de "Lookups" complejos.

### 5. Uso de Transacciones
Para asegurar la atomicidad en operaciones complejas (como la creación de una compra que genera múltiples cuotas), se ha contemplado el uso de transacciones:
- **EF Core:** `DbContext.Database.BeginTransactionAsync()`.
- **MongoDB:** `IClientSession.StartTransaction()`.

---

## Instrucciones para Levantar la Aplicación

### Configuración de Base de Datos
La aplicación permite cambiar entre MySQL y MongoDB mediante el archivo `appsettings.json`.

1. **MySQL:**
   - Asegúrese de tener un servidor MySQL corriendo.
   - Configure la cadena de conexión en `ConnectionStrings:MySql`.
   - Ejecute las migraciones de EF Core: `dotnet ef database update`.

2. **MongoDB:**
   - Asegúrese de tener una instancia de MongoDB.
   - Configure `ConnectionStrings:Mongo` y `ConnectionStrings:MongoDbName`.

3. **Selección de Persistencia:**
   - Cambie la clave `"Persistence"` en `appsettings.json` a `"MySql"` o `"Mongo"`.

### Pruebas de la API
La API expone los endpoints requeridos a través de Swagger. Al ejecutar el proyecto (`dotnet run`), acceda a `https://localhost:7055/swagger/index.html` para probar las funcionalidades:
- Agregar promociones.
- Editar fechas de vencimiento de pagos.
- Generar reportes mensuales de pagos.
- Consultas de estadísticas (Top locales, bancos, clientes por banco).

### Uso de Docker
Para levantar todos los servicios (App, MySQL y MongoDB) de forma automática:
1. Asegúrese de tener Docker Desktop instalado y corriendo.
2. Ejecute `docker-compose up --build -d` en la raíz del proyecto.
3. La API estará disponible en `http://localhost:8080/swagger`.
4. Para cambiar la base de datos que usa la app en docker, modifique la variable de entorno `Persistence` en el archivo `docker-compose.yml`.

---

**Fecha de Entrega:** 1/04/2026
**Autores:** Acosta Jorge Santiago, Valoni Mercedes.
