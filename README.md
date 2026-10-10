# TP Integrador — Sistema de Venta y Validación de Entradas para Eventos

**Materia:** Programación 1 — UCSE  
**Integrantes:** Uriel Callebaut, Faustino Trivelli, Marcos Weppler  
**Tecnologías:** C# / .NET 8 (con compatibilidad de roll-forward de runtime), ASP.NET Core Web API, NUnit (25 pruebas unitarias en verde), Persistencia JSON compartida en disco, HTML5, CSS3, JavaScript nativo (Vanilla JS SPA con Hash Routing), Leaflet con mosaicos CARTO Voyager y OpenStreetMap Nominatim.

---

## 📌 Descripción General

El sistema reemplaza la gestión tradicional de venta de entradas con planillas de cálculo y mensajería manual por una solución integral compuesta por **dos APIs REST independientes que comparten la misma información persistida en disco (sin base de datos central)** y una **aplicación web frontend**:

1. **API de Gestión (`ApiGestion` - Puerto 5001):**
   - Catálogo y administración de eventos y sus modalidades de entrada.
   - Venta de entradas asociadas al DNI del comprador, con cálculo automático de **descuento por volumen del 15% para compras de 5 o más entradas** de la misma modalidad.
   - Generación de **entradas individuales con códigos únicos alfanuméricos de 6 dígitos** (garantía de no repetición y unicidad global).
   - Reportes de recaudación y entradas vendidas por evento con desglose por modalidad y listado de compradores, deduciendo de forma consistente cualquier entrada cancelada.
   - Endpoint de promoción: **Cancelación de entradas individuales** (`DELETE /api/entradas/{codigo}`) restaurando el cupo de la modalidad.
   - Cancelación de modalidades y eventos completos por organizadores.
   - Control de permisos mediante roles (**Organizador** vs. **Comprador**) y excepciones propias de dominio.
   - Documentación interactiva con **Swagger UI**.

2. **API de Validación en Puerta (`ApiValidacion` - Puerto 5002):**
   - Utilizada en el acceso el día del evento.
   - Implementada bajo el **patrón Repositorio y Servicio (Unidad 5)** con entidades de dominio fuertemente tipadas (`Entrada`, `Compra`, `Evento`).
   - Recibe el código de una entrada y valida si puede ingresar, registrando inmediatamente su uso (`Usada = true`, fecha y hora) o rechazándola con causa y estado tipado explícito:
     - `Valida`: ingreso autorizado.
     - `YaFueUsada`: entrada utilizada previamente con detalle de fecha y hora.
     - `Cancelada`: entrada cancelada previamente por el comprador.
     - `NoExiste`: código no registrado en el sistema.
     - `EventoIncorrecto`: la entrada corresponde a otro evento.
     - `EventoCancelado`: el evento asociado ha sido suspendido/cancelado.
   - Sincronización inmediata contra los mismos archivos en disco.

3. **Frontend Web (`CarpetaFrontend`):**
   - Aplicación responsiva construida con HTML5, CSS3 (paleta Slate moderna sin excesos visuales) y JavaScript nativo.
   - **Arquitectura SPA con Hash Routing (`#...`):** URLs independientes por vista (`#catalogo`, `#evento={id}`, `#consulta-compra`, `#control-acceso`, `#gestion-eventos`, `#reporte-recaudacion`) con soporte de historial del navegador (`Atrás`/`Adelante`) y *deep linking*.
   - Selector dinámico de usuario activo basado en DNI y validación de roles.
   - Restricción de seguridad en cliente: **la vista de Control de Acceso (Puerta) está reservada exclusivamente a organizadores**.
   - Aislamiento de privacidad: los compradores sólo visualizan sus propias entradas y compras adquiridas (`GET /api/compras?dni=...`).
   - Mapa interactivo con **Leaflet + CARTO Voyager**, compatible con redes universitarias e institucionales (sin bloqueos de CORS/403).

---

## 📂 Estructura del Repositorio

```text
├── ApiGestion/                 # Solución 1: API de Gestión y Ventas
│   ├── Api/                    # Web API ASP.NET Core (.NET 8, Swagger en :5001)
│   │   ├── Controllers/        # Controladores REST (Eventos, Compras, Entradas, Usuarios, Reportes)
│   │   ├── DTOs/               # DTOs de transferencia para API (Unidad 6)
│   │   ├── Api.http            # Solicitudes HTTP de prueba para VS / VS Code
│   │   └── Program.cs          # Inyección de dependencias y middlewares
│   ├── Clases/                 # Capa de Dominio y Datos (Unidad 5)
│   │   ├── Dominio/            # Entidades encapsuladas (Evento, Modalidad, Compra, Entrada, Usuario)
│   │   ├── Excepciones/        # Excepciones propias del dominio (AccesoDenegado, ReglaNegocio, etc.)
│   │   ├── Data/               # Repositorios JSON y DataPathHelper robusto
│   │   └── Servicios/          # Lógica de negocio (EventoService, CompraService, UsuarioService)
│   ├── Testing/                # Pruebas unitarias NUnit de ApiGestion (18 tests en verde)
│   └── Solucion.sln            # Solución Visual Studio de Gestión
│
├── ApiValidacion/              # Solución 2: API de Validación en Puerta
│   ├── Api/                    # Web API ASP.NET Core (.NET 8, Swagger en :5002)
│   │   ├── Controllers/        # Controlador de validación en puerta
│   │   ├── Api.http            # Solicitudes HTTP de validación
│   │   └── Program.cs          # Configuración DI con repositorios tipados
│   ├── Clases/                 # Capa de Lógica y Persistencia Tipada (Unidad 5)
│   │   ├── Dominio/            # Entidades tipadas (Entrada, Compra, Evento)
│   │   ├── Data/               # ValidacionCompraRepository, ValidacionEventoRepository y DataPathHelper
│   │   ├── EstadoValidacion.cs # Enum de estados (incluye Cancelada, YaFueUsada, Valida, etc.)
│   │   └── ValidadorEntradaService.cs # Servicio de validación atómica
│   ├── Testing/                # Pruebas unitarias NUnit de ApiValidacion (7 tests en verde)
│   └── Solucion.sln            # Solución Visual Studio de Validación
│
├── CarpetaFrontend/            # Frontend Web (Vanilla HTML/CSS/JS SPA)
│   ├── index.html              # Estructura de vistas del sistema
│   ├── styles.css              # Diseño Slate minimalista, accesible y responsivo
│   └── app.js                  # Lógica de cliente, consumo de APIs y Leaflet con CARTO Voyager
│
├── data/                       # Almacenamiento compartido en disco (rutas relativas)
│   ├── usuarios.json           # 10 usuarios del anexo (2 Organizadores, 8 Compradores)
│   ├── eventos.json            # Eventos, coordenadas y modalidades
│   └── compras.json            # Compras y entradas individuales generadas
│
└── README.md                   # Documentación técnica completa
```

---

## 🚀 Cómo Ejecutar el Proyecto

### Requisitos Previos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado (compatible también con .NET 9 y 10 vía roll-forward).
- Navegador web moderno (Chrome, Edge, Firefox).

---

### Paso 1: Ejecutar las Pruebas Unitarias (NUnit)

Para verificar que toda la lógica de negocio, las reglas de promoción y los casos de borde están superados (25/25 en verde):

```bash
# Tests de ApiGestion (18 pruebas unitarias: descuento, roles, unicidad, cupos, cancelación y reporte)
dotnet test ApiGestion/Solucion.sln

# Tests de ApiValidacion (7 pruebas unitarias: válida, usada, cancelada, inexistente, otro evento, etc.)
dotnet test ApiValidacion/Solucion.sln
```

---

### Paso 2: Iniciar las APIs

Abra dos terminales en la raíz del proyecto:

#### Terminal 1 — API de Gestión (Puerto por defecto: 5001):
```bash
cd ApiGestion/Api
dotnet run
```
> **Swagger UI:** [http://localhost:5001/swagger](http://localhost:5001/swagger)

#### Terminal 2 — API de Validación (Puerto por defecto: 5002):
```bash
cd ApiValidacion/Api
dotnet run
```
> **Swagger UI:** [http://localhost:5002/swagger](http://localhost:5002/swagger)

*Nota sobre puertos:* Si se ejecutan en puertos alternativos mediante el parámetro `--urls` (por ejemplo `http://localhost:5130` y `http://localhost:5131`), el frontend `CarpetaFrontend/app.js` permite ajustar las constantes `API_GESTION` y `API_VALIDACION` en sus primeras líneas.

---

### Paso 3: Abrir el Frontend

Abra directamente el archivo `CarpetaFrontend/index.html` en su navegador preferido (haciendo doble clic en el archivo o sirviéndolo con una extensión como Live Server).

---

## 👥 Usuarios Precargados (`data/usuarios.json`)

El sistema incluye los 10 usuarios requeridos en el anexo de la cátedra, identificados por su DNI (campo único `dni`):

| DNI | Nombre | Rol | Acciones Permitidas |
|---|---|---|---|
| **30111222** | Lucía Fernández | **Organizador** | Crear/editar/cancelar eventos y modalidades, ver recaudación, puerta. |
| **29888777** | Martín Aguirre | **Organizador** | Crear/editar/cancelar eventos y modalidades, ver recaudación, puerta. |
| **40123456** | Sofía Gómez | **Comprador** | Comprar entradas, ver únicamente sus compras, cancelar entradas no usadas. |
| **38456789** | Nicolás Pereyra | **Comprador** | Comprar entradas, ver únicamente sus compras, cancelar entradas no usadas. |
| **41234567** | Valentina Ríos | **Comprador** | Comprar entradas, ver únicamente sus compras, cancelar entradas no usadas. |
| **37654321** | Tomás Ibáñez | **Comprador** | Comprar entradas, ver únicamente sus compras, cancelar entradas no usadas. |
| **42345678** | Camila Suárez | **Comprador** | Comprar entradas, ver únicamente sus compras, cancelar entradas no usadas. |
| **39876543** | Agustín Molina | **Comprador** | Comprar entradas, ver únicamente sus compras, cancelar entradas no usadas. |
| **43456789** | Julieta Torres | **Comprador** | Comprar entradas, ver únicamente sus compras, cancelar entradas no usadas. |
| **36765432** | Bruno Acosta | **Comprador** | Comprar entradas, ver únicamente sus compras, cancelar entradas no usadas. |

---

## 📋 Endpoints de las APIs REST

### API de Gestión (`http://localhost:5001`)

| Método | Endpoint | Rol Requerido | Descripción |
|---|---|---|---|
| `GET` | `/api/usuarios` | — | Lista todos los usuarios con DNI unificado. |
| `GET` | `/api/usuarios/{dni}` | — | Obtiene los datos del usuario correspondiente al DNI. |
| `GET` | `/api/eventos` | — | Lista los eventos del sistema con sus modalidades asociadas. |
| `GET` | `/api/eventos/{id}` | — | Detalle de un evento y sus modalidades. |
| `POST` | `/api/eventos` | **Organizador** (`X-Dni`) | Crea un evento con fecha, lugar, descripción y geolocalización. |
| `PUT` | `/api/eventos/{id}` | **Organizador** (`X-Dni`) | Edita los datos de un evento no cancelado. |
| `PUT` | `/api/eventos/{id}/cancelar` | **Organizador** (`X-Dni`) | Cancela un evento completo (bloquea ventas de todas sus modalidades). |
| `POST` | `/api/eventos/{id}/modalidades` | **Organizador** (`X-Dni`) | Agrega una modalidad de entrada con precio, cupo y beneficios. |
| `PUT` | `/api/eventos/{id}/modalidades/{idModalidad}/cancelar` | **Organizador** (`X-Dni`) | Cancela una modalidad específica (bloquea ventas para dicha modalidad). |
| `POST` | `/api/compras` | **Comprador** (`X-Dni` o body) | Registra la compra, descuenta cupo y genera entradas individuales. |
| `GET` | `/api/compras/{id}` | — | Detalle de una compra específica. |
| `GET` | `/api/compras?dni={dni}` | — | Lista las compras pertenecientes a un DNI específico. |
| `GET` | `/api/compras` *(sin DNI)* | **Organizador** (`X-Dni`) | Devuelve el total de compras del sistema (protegido con 403 Forbidden). |
| `DELETE` | `/api/entradas/{codigo}` | **Comprador** (`X-Dni`) | *(Promoción)* Cancela una entrada no usada de su propiedad y restituye el cupo. |
| `GET` | `/api/reportes/recaudacion` | **Organizador** (`X-Dni`) | Reporte de recaudación y entradas vendidas deduciendo entradas canceladas. |

### API de Validación (`http://localhost:5002`)

| Método | Endpoint | Descripción |
|---|---|---|
| `POST` | `/api/validaciones` | Valida el código de entrada. Soporta validación general (`{ "codigo": "..." }`) o para terminal asignada a evento (`{ "codigo": "...", "idEvento": "..." }`). Marca como usada o rechaza con estado (`Valida`, `YaFueUsada`, `Cancelada`, `NoExiste`, `EventoIncorrecto`, `EventoCancelado`). |

---

## 🧠 Decisiones de Diseño, Arquitectura y Análisis para el Coloquio

A continuación se fundamentan las decisiones requeridas para la defensa oral y la retención de la promoción:

### 1. Política de Cancelación de Entradas con Descuento por Volumen
- **El caso:** Un comprador adquiere 5 entradas de \$1.000. Al comprar $\ge 5$, se aplica un 15% de descuento, abonando un total de \$4.250 (\$850 por entrada). Posteriormente, el comprador cancela 1 entrada.
- **Decisión implementada:**
  - Al realizarse la compra, cada entrada almacena de forma inmutable su precio unitario efectivo (`PrecioUnitario = 850`).
  - Al cancelar 1 entrada, se restaura 1 lugar de cupo a la modalidad (`CupoDisponible++`) y la entrada queda marcada como `Cancelada = true`.
  - La compra recalcula su total sumando exclusivamente las entradas activas: $4 \times 850 = \$3.400$.
  - **Justificación:** La transacción inicial fue celebrada bajo las condiciones de venta mayorista vigentes al momento del pago. Aplicar un recargo retroactivo a las 4 entradas restantes por la cancelación de una de ellas implicaría cobrarle un adicional al comprador sin su consentimiento. Si un negocio quisiera penalizar esta situación, la regla comercial alternativa requeriría cancelar la compra completa y obligar a una recompra al precio estándar de 4 unidades sin descuento.
- **Impacto en el Reporte de Recaudación:**
  - El reporte de recaudación (`EventoService.ObtenerReporteRecaudacion`) suma estrictamente las entradas que no están canceladas (`!e.Cancelada`).
  - Tras la cancelación, el reporte refleja exactamente **4 entradas vendidas** y **\$3.400 de recaudación**, eliminando cualquier inconsistencia entre cantidad y monto.

### 2. Validación de Entradas Canceladas en Puerta
- Cuando el operador de puerta intenta escanear una entrada cancelada (por ejemplo, `G4BXEZ`), la API de Validación responde:
  - Código HTTP: `400 Bad Request`.
  - Estado devuelto: `Cancelada` (dentro del enum `EstadoValidacion`).
  - Mensaje explicativo: *"La entrada 'G4BXEZ' fue cancelada previamente por el comprador."*
- Esto garantiza correspondencia semántica exacta entre el estado tipado y la justificación del rechazo.

### 3. Validación por Evento en Puerta
- La API de Validación exige enviar el `idEvento` del evento que se está controlando. Si falta o es `Guid.Empty`, rechaza la solicitud:
  - El identificador del evento es obligatorio. Si la entrada es válida pero corresponde a otro evento, se rechaza como `EventoIncorrecto`.
  - El frontend también exige seleccionar el evento que se está controlando antes de enviar el código.

### 4. Concurrencia y Persistencia Compartida en Archivos JSON
- Ambas APIs comparten el acceso a `data/compras.json` y `data/eventos.json`.
- En el código fuente, las operaciones de lectura y guardado están protegidas mediante un semáforo de bloqueo en memoria (`lock (Bloqueo)`).
- **Riesgo entre procesos:** Dado que `ApiGestion` y `ApiValidacion` son dos procesos independientes en el sistema operativo, los bloqueos en memoria solo resuelven la concurrencia entre hilos de un mismo proceso. Si una compra en Gestión y una validación en Puerta se escriben en el mismo milisegundo exacto sobre el disco, existe un riesgo de colisión de archivos (*race condition*).
- **Solución en entornos productivos:** En un sistema comercial, la concurrencia se delega a un motor de base de datos relacional (PostgreSQL, SQL Server) con transacciones ACID y aislamiento, o mediante un bloqueo exclusivo a nivel del sistema operativo (`FileStream` con `FileShare.None`).

### 5. Resolución Robusta de Rutas (`DataPathHelper`)
- Para evitar que la carga de archivos dependa rígidamente del directorio de trabajo actual desde el que se lanza cada API, se implementó la clase `DataPathHelper`.
- Esta utilidad inspecciona la jerarquía de carpetas hacia arriba (recorriendo los directorios padres tanto desde `Directory.GetCurrentDirectory()` como desde `AppContext.BaseDirectory`), localizando la carpeta `data/` de la raíz del proyecto tanto al ejecutar desde la raíz, desde la carpeta de la solución o desde la subcarpeta `Api/`.

### 6. Calidad POO, Encapsulamiento y Excepciones de Dominio
- **Encapsulamiento estricto:** Todas las propiedades de estado crítico (`Usada`, `Cancelada`, `CupoDisponible`, `Total`) tienen setter privado (`private set`). La modificación de estado solo es posible a través de métodos de negocio con validaciones (`MarcarComoUsada()`, `CancelarEntrada()`, `RegistrarVenta()`, `RestaurarCupo()`, `RecalcularTotal()`).
- **Compatibilidad con serialización:** Se incorporaron los atributos `[JsonProperty]` y `[JsonInclude]` para permitir la reconstrucción fiel de entidades tanto en Newtonsoft.Json como en System.Text.Json sin abrir setters públicos.
- **Excepciones de Dominio:** Se definieron excepciones propias:
  - `AccesoDenegadoException`: lanzada ante violaciones de roles o intentos de acceder a recursos ajenos (reemplaza excepciones de E/S del framework).
  - `ReglaNegocioException`: lanzada ante operaciones no permitidas (comprar en eventos cancelados, eventos pasados, cancelar entradas ya usadas).
  - `CupoInsuficienteException`: subtipo específico cuando se agota el cupo de una modalidad.
  - `EntidadNoEncontradaException`: lanzada cuando un recurso no existe en los repositorios.

---

## 🗺️ Mapa Interactivo: Leaflet + CARTO Voyager

- En reemplazo de librerías comerciales que exigen llaves de facturación o que sufren bloqueos institucionales de CORS/403 en redes universitarias (como los servidores estándar de OpenStreetMap), el mapa del frontend utiliza **Leaflet 1.9.4** con la capa de mosaicos vectoriales rasterizados **CARTO Voyager**.
- Permite:
  - Navegación fluida por mapas de calles sin límites de uso ni marcas de agua.
  - Selector interactivo con marcador arrastrable y geocodificación de direcciones vía OpenStreetMap Nominatim para organizadores.
  - Enlaces directos a navegación externa.

---

## 🧪 Pruebas Unitarias Automatizadas (NUnit)

### Solución `ApiGestion/Solucion.sln` — 18 tests definidos:
1. `DescuentoPorVolumen_CompraDeCincoOMasEntradas_AplicaDescuentoDel15PorCiento`
2. `RealizarCompra_SinCupoDisponible_LanzaExcepcion`
3. `RealizarCompra_EventoCancelado_LanzaExcepcion`
4. `RealizarCompra_EventoConFechaPasada_LanzaExcepcion`
5. `RealizarCompra_GeneraEntradasIndividualesConCodigosUnicosDe6Caracteres`
6. `ValidacionRoles_OrganizadorYComprador_AplicaRestriccionesCorrectas`
7. `CancelarEntrada_CompradorCancelaEntradaNoUsada_RestauraCupoYMarcaCancelada`
8. `CancelarEntrada_EntradaYaUsada_LanzaExcepcion`
9. `ReporteRecaudacion_CalculaEntradasVendidasYTotalesCorrectamente`
10. `CancelarModalidad_OrganizadorPuedeCancelarModalidad_QuedaMarcadaComoCancelada`
11. `RealizarCompra_ModalidadCancelada_LanzaExcepcion`
12. `CancelarEvento_OrganizadorPuedeCancelarEvento_QuedaMarcadoComoCancelado`
13. `RealizarCompra_DniInexistente_LanzaAccesoDenegadoException` *(Borde Sección 5)*
14. `RealizarCompra_OrganizadorIntentaComprar_LanzaAccesoDenegadoException` *(Borde Sección 5)*
15. `CancelarEntrada_CompradorCancelaEntradaDeOtroComprador_LanzaAccesoDenegadoException` *(Borde Sección 5)*
16. `RealizarCompra_CompraExactamenteCuatroEntradas_NoAplicaDescuento` *(Borde Sección 5)*
17. `RealizarCompra_UnicidadDeCodigosEntreComprasDistintas_TodosLosCodigosSonUnicos` *(Borde Sección 5)*
18. `ReporteRecaudacion_DespuesDeCancelarEntrada_DescuentaMontoDeLaEntradaCancelada` *(Borde Sección 5 y corrección G16)*

### Solución `ApiValidacion/Solucion.sln` — 8 tests definidos:
1. `ValidarEntrada_EntradaValida_AutorizaIngresoYMarcaComoUsada`
2. `ValidarEntrada_EntradaYaUsada_RechazaConEstadoYaFueUsada`
3. `ValidarEntrada_EntradaInexistente_RechazaConEstadoNoExiste`
4. `ValidarEntrada_EntradaDeOtroEvento_RechazaConEstadoEventoIncorrecto`
5. `ValidarEntrada_EntradaCanceladaPorComprador_RechazaIngreso` *(Verifica Estado: Cancelada)*
6. `ValidarEntrada_EventoCancelado_RechazaIngreso`
7. `ValidarEntrada_DobleValidacionConsecutiva_SegundaFallaPorYaUsada`