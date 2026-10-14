# TP Integrador — Sistema de Venta y Validación de Entradas para Eventos

**Repositorio:** `2026-prog1-tpfinal-Escultores`
**Materia:** Programación 1 — UCSE
**Equipo:** Escultores — Uriel Callebaut, Faustino Trivelli, Marcos Weppler
**Video de la demo:** _(pegar acá el enlace; el video NO se sube al repositorio, los binarios pesados no se versionan en Git)_

Dos APIs REST (ASP.NET Core, .NET 8) que **comparten los mismos archivos JSON** (sin base de datos) y un frontend en HTML/CSS/JavaScript sin frameworks.

| Componente | Carpeta | Puerto | Para qué sirve |
|---|---|---|---|
| API de Gestión | `ApiGestion/` | **5001** | Eventos, modalidades, compras, entradas, reporte. Swagger en `/swagger`. |
| API de Validación | `ApiValidacion/` | **5002** | Validar entradas en la puerta el día del evento. |
| Frontend | `CarpetaFrontend/` | — | Pantallas para organizador, comprador y control de acceso. |
| Datos compartidos | `data/` | — | `usuarios.json`, `eventos.json`, `compras.json`. |

Los puertos salen de `Api/Properties/launchSettings.json` de cada API y coinciden con las constantes `API_GESTION` / `API_VALIDACION` de `CarpetaFrontend/app.js`.

## Cómo levantarlo

Requisitos: .NET 8 SDK y un navegador moderno.

```bash
# 0) Clonar y entrar a la raíz del repo (todas las rutas de abajo son relativas a ella)
git clone <url-del-repo>/2026-prog1-tpfinal-Escultores.git
cd 2026-prog1-tpfinal-Escultores

# 1) Tests (una vez por solución, desde la raíz del repo)
dotnet test ApiGestion/Solucion.sln
dotnet test ApiValidacion/Solucion.sln

# 2) API de Gestión (terminal 1)
cd ApiGestion/Api
dotnet run            # http://localhost:5001/swagger

# 3) API de Validación (terminal 2)
cd ApiValidacion/Api
dotnet run            # http://localhost:5002/swagger

# 4) Frontend: abrir CarpetaFrontend/index.html (o servirlo con Live Server)
```

**Desde qué carpeta se lanzan:** las rutas son relativas y se resuelven con `DataPathHelper`, que sube por los directorios padres hasta encontrar la carpeta `data/` de la raíz del repo. Por eso funciona lanzando desde la raíz, desde `ApiGestion/` o desde `ApiGestion/Api/`. Si no la encuentra, **falla con un mensaje claro** en vez de crear otra carpeta en silencio (si cada API usara su propia carpeta `data`, dejarían de compartir datos).

## Estructura

```text
2026-prog1-tpfinal-Escultores/
├── ApiGestion/
│   ├── Api/        Controllers/, DTOs/ (Request, Response y GestionMapper), Program.cs
│   ├── Clases/     Entidades, servicios (EventoService, CompraService, UsuarioService),
│   │               Excepciones/, Data/ (repositorios JSON y DataPathHelper)
│   └── Testing/    Tests NUnit de la lógica de Gestión
├── ApiValidacion/
│   ├── Api/        Controllers/ValidacionesController, DTOs/, Program.cs
│   ├── Clases/     Dominio/ (Compra, Entrada, Evento), Data/ (repositorios), ValidadorEntradaService
│   └── Testing/    Tests NUnit de la lógica de Validación
├── CarpetaFrontend/  index.html, páginas por pantalla, app.js, styles.css
└── data/             usuarios.json, eventos.json, compras.json
```

Dependencias: `Api → Clases` y `Testing → Clases`. Los controllers y los DTOs viven en `Api`; `Clases` es una biblioteca de clases común en las dos soluciones (no es un proyecto Web) y no conoce HTTP.

## Identificación del usuario y roles

No hay login. Quien opera se indica con el **DNI en el header `X-Dni`** (alternativa para pruebas desde el navegador: `?dni=`). Es el **único criterio en todos los endpoints**; ningún endpoint lo toma del body. El servicio busca el DNI en `usuarios.json` y compara el rol; si falta, no existe o el rol no corresponde → **403** con el motivo en el body. El frontend, además, oculta las acciones que el rol no puede hacer.

## Endpoints

Errores: siempre `{ "error": "motivo" }` (400 regla de negocio o formato, 403 acceso, 404 no existe).

### API de Gestión (`http://localhost:5001`)

| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| GET | `/api/usuarios` | — | Usuarios precargados (un solo campo `dni`). |
| GET | `/api/eventos` | — | Catálogo: solo eventos **disponibles** (no cancelados y futuros). Con `?incluirNoDisponibles=true` también cancelados y pasados. |
| GET | `/api/eventos/{id}` | — | Detalle con modalidades (existe para cualquier evento). |
| POST | `/api/eventos` | Organizador | Crea un evento (201). |
| PUT | `/api/eventos/{id}` | Organizador | Edita un evento no cancelado. |
| PUT | `/api/eventos/{id}/cancelar` | Organizador | Cancela el evento. |
| POST | `/api/eventos/{id}/modalidades` | Organizador | Agrega una modalidad (201). |
| PUT | `/api/eventos/{id}/modalidades/{idModalidad}/cancelar` | Organizador | Cancela una modalidad. |
| POST | `/api/compras` | Comprador | Compra y genera las entradas individuales (201). El body lleva `idEvento`, `idModalidad`, `cantidad`. |
| GET | `/api/compras/{id}` | — | Detalle y estado de cada entrada. No exige rol (como pide el enunciado); si se envía `X-Dni`, un comprador solo ve las suyas y el organizador cualquiera. |
| GET | `/api/compras` | Cualquier usuario cargado | Compras de quien opera; el organizador recibe todas. |
| DELETE | `/api/entradas/{codigo}` | Comprador dueño | (Promoción) Cancela una entrada no usada y devuelve el cupo. |
| GET | `/api/reportes/recaudacion` | Organizador | Entradas vendidas y dinero por evento, modalidad y comprador. |

### API de Validación (`http://localhost:5002`)

`POST /api/validaciones` con `{ "codigo": "ABC123", "idEvento": "<guid>" }`. **El evento es obligatorio.**

| Resultado | HTTP | `estado` |
|---|---|---|
| Ingreso autorizado (queda usada, con fecha y hora) | 200 | `Valida` |
| El código no existe | 404 | `NoExiste` |
| La entrada es de otro evento (o no se envió evento) | 400 | `EventoIncorrecto` |
| La entrada fue cancelada por el comprador | 400 | `Cancelada` |
| El evento fue cancelado | 400 | `EventoCancelado` |
| La entrada ya ingresó | 400 | `YaFueUsada` |
| Falta el código o el evento | 400 | `DatosInvalidos` |

## Decisiones de diseño (para el coloquio)

**Dónde vive cada regla.** El descuento por volumen (5 o más entradas de la misma modalidad = 15%) lo calcula `Modalidad.CalcularPrecio`, junto al dato del precio, y se prueba sola. El cupo lo maneja `Modalidad.RegistrarVenta/RestaurarCupo`. Los permisos por rol y las reglas de compra están en los servicios, no en los controllers: el controller solo llama al servicio y traduce las excepciones del dominio a 400/403/404 (`BaseApiController.Ejecutar`).

**Entrada individual.** Cada `Entrada` guarda su código, el evento y la modalidad a los que pertenece, si fue usada (con fecha), si fue cancelada y el **precio que realmente pagó**. Por eso se valida sola, sin la compra.

**Códigos.** 6 caracteres `A-Z0-9` con `RandomNumberGenerator`; el servicio carga en un `HashSet` los códigos de **todas** las compras y el generador reintenta hasta encontrar uno libre, así que no se repiten entre compras ni dentro de la misma.

**Encapsulamiento.** El estado de negocio (`CupoDisponible`, `Usada`, `Cancelada`, `Total`) tiene `private set` y solo cambia por métodos que validan. Cada propiedad con `private set` lleva `[JsonProperty]`: sin ese atributo Newtonsoft no puede asignarla al leer el JSON y la entidad se reconstruye vacía (hay tests que lo verifican).

**Excepciones propias.** `AccesoDenegadoException`, `ReglaNegocioException` (y su subtipo `CupoInsuficienteException`) y `EntidadNoEncontradaException`, todas derivadas de `DominioException`, que hereda de `Exception`. No se usan excepciones del framework con otro significado.

**Reloj inyectable.** Los servicios reciben `Func<DateTime>` (por defecto `DateTime.Now`) para que los tests no dependan de la hora real.

### Cancelar una entrada de una compra con descuento
Se compran 5 entradas de $1.000 con descuento: total $4.250, **$850 cada una** (la última absorbe cualquier diferencia de redondeo, así la suma de las entradas es exactamente lo cobrado). Si se cancela una:
- La entrada cancelada se reintegra por lo que pagó ($850) y vuelve su lugar al cupo.
- Las otras 4 **conservan el precio que ya abonaron**: la compra se hizo bajo las condiciones vigentes al pagar; recalcularlas sin descuento le cobraría de más al comprador sin su consentimiento.
- `Compra.Total` pasa a ser la suma de las entradas no canceladas: $3.400.
- **Reporte de recaudación:** cuenta solo entradas no canceladas y suma su precio efectivo, de modo que cantidad y dinero bajan juntos (4 entradas, $3.400). La cantidad y el dinero del evento salen de la misma lista de compradores, por lo que no pueden discrepar.
- Una entrada **ya validada en la puerta no se puede cancelar**, ni tampoco las de un evento que ya comenzó.

### Catálogo de eventos
`GET /api/eventos` devuelve solo lo que se puede comprar. Justificación: una persona no puede comprar un evento cancelado o pasado, así que no tiene sentido ofrecérselo. El detalle (`/api/eventos/{id}`) sí existe para cualquier evento, y el reporte del organizador incluye todos.

### Persistencia compartida y concurrencia
Cada operación **lee el archivo completo, modifica y guarda completo**, sin copia en memoria, por eso una compra hecha en Gestión se valida al instante en Validación. La API de Validación tiene sus propias clases de dominio (`Compra`, `Entrada`, `Evento`) y repositorios; sus clases modelan **todas** las propiedades que escribe Gestión, porque al reescribir el archivo una propiedad no modelada se borraría (un test lo comprueba).

**Riesgo:** los `lock` protegen solo entre hilos de un mismo proceso, y Gestión y Validación son dos procesos. Si una venta y una validación leen-modifican-escriben `compras.json` al mismo tiempo, gana la última escritura y se puede perder el cambio de la otra (por ejemplo, una validación o una venta). Se reduce acortando la ventana lectura→escritura (así está hecho), y se resolvería con acceso exclusivo al archivo (`FileStream` con `FileShare.None` y reintento, o un `Mutex` con nombre) o con una base de datos con transacciones.

### Mapa (promoción)
El frontend muestra la ubicación con un **mapa de Google Maps incrustado (`iframe` con `maps.google.com/maps?q=lat,lng&output=embed`)**, sin librería de mapas ni API key, más un botón "Ver en Google Maps" que abre la ubicación en una pestaña nueva. Para buscar una dirección al crear un evento usa **Nominatim (OpenStreetMap)** y con el resultado actualiza el mapa. El evento guarda `Latitud` y `Longitud` además del `Lugar`: están en el dominio (`Evento`), se persisten en `eventos.json`, viajan en los DTOs y el frontend los usa para centrar el mapa. Si el evento no tiene coordenadas, se muestra una ubicación por defecto (Buenos Aires).

## Tests

Se ejecutan con `dotnet test` en cada solución: **64 tests en Gestión y 17 en Validación**. Nombres `Metodo_Escenario_Resultado`, un comportamiento por test, archivos temporales propios y reloj fijo. Se dejó un test por regla de negocio y se sacaron los que repetían lo mismo desde otro ángulo.

- **Gestión:**
  - Descuento: 4 entradas (sin descuento) y 5 (con 15%), directo en `Modalidad` y a través del servicio.
  - Cupo: compra que supera el cupo, cupo agotado, y el cupo nunca supera el máximo al restaurar.
  - Rechazos de compra: evento cancelado, evento pasado, evento inexistente y modalidad cancelada.
  - Roles: DNI inexistente, organizador intentando comprar, comprador intentando crear o cancelar eventos o agregar modalidades, y comprador intentando ver el reporte.
  - Códigos: 6 caracteres alfanuméricos, sin repetirse dentro de una compra ni entre compras distintas.
  - Cancelación de entradas: propia, de otro comprador, usada en la puerta, ya cancelada, de un evento que ya comenzó, y total de la compra con descuento.
  - Reporte de recaudación: con compra de 5 con descuento, **después de cancelar una entrada** (bajan la cantidad y el dinero), cancelando todas, y totales generales de varios eventos.
  - Catálogo: no muestra eventos cancelados ni pasados.
  - Formato del JSON compartido con Validación y lectura de usuarios.
- **Validación:** entrada válida (queda usada), segunda validación rechazada, cada entrada de una compra por separado, código inexistente, código vacío, entrada de otro evento, sin evento, entrada cancelada (estado `Cancelada`), evento cancelado, y lectura/reescritura del formato real de Gestión sin perder campos ni la persistencia al reiniciar.

## Usuarios precargados (`data/usuarios.json`)

2 organizadores (30111222 Lucía Fernández, 29888777 Martín Aguirre) y 8 compradores (40123456, 38456789, 41234567, 37654321, 42345678, 39876543, 43456789, 36765432), según el anexo del enunciado.

## Nota para la demo
`data/eventos.json` incluye "Noche de Jazz & Blues" con fecha ya pasada (sirve para mostrar el rechazo de compra) y eventos de prueba ("Test", "Devil God Contest"). Antes de grabar el video conviene usar un evento futuro (por ejemplo "Festival Primavera Sound 2026", 15/10/2026, o crear uno nuevo).
