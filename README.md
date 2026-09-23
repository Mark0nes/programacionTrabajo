# TP Integrador — Sistema de Venta y Validación de Entradas para Eventos

**Materia:** Programación 1 — UCSE  
**Tecnologías:** C# / .NET 8, ASP.NET Core Web API, NUnit, Persistencia JSON compartida en disco, HTML5, CSS3, JavaScript nativo (Vanilla JS), Leaflet / OpenStreetMap.

---

## 📌 Descripción General

El sistema reemplaza la gestión tradicional de venta de entradas con Excel y WhatsApp por una solución moderna compuesta por **dos APIs REST independientes que comparten la misma información persistida en disco (sin base de datos central)** y una **aplicación web frontend**:

1. **API de Gestión (`ApiGestion` - Puerto 5001):**
   - Catálogo y administración de eventos y sus modalidades de entrada.
   - Venta de entradas asociadas al DNI del comprador, con cálculo automático de **descuento por volumen del 15% para compras de 5 o más entradas** de la misma modalidad.
   - Generación de **entradas individuales con códigos únicos alfanuméricos de 6 dígitos** (sin colisiones).
   - Reportes de recaudación y entradas vendidas por evento.
   - Endpoint de promoción: **Cancelación de entradas individuales** (`DELETE /api/entradas/{codigo}`) restaurando el cupo de la modalidad.
   - Documentación interactiva con **Swagger UI**.

2. **API de Validación en Puerta (`ApiValidacion` - Puerto 5002):**
   - Utilizada en el acceso el día del evento.
   - Recibe el código de una entrada y valida si puede ingresar, registrando inmediatamente su uso (`Usada = true`, fecha y hora) o rechazándola con causa explícita (inexistente, ya usada, corresponde a otro evento, evento cancelado).
   - Sincronización inmediata contra los mismos archivos en disco.

3. **Frontend Web (`CarpetaFrontend`):**
   - Aplicación responsiva construida con HTML, CSS y JavaScript nativo (sin frameworks).
   - Selector dinámico de usuario activo basado en DNI y validación de roles (**Organizador** vs. **Comprador**).
   - Catálogo de eventos, vista de detalle con **mapa interactivo geográfico (Leaflet)**.
   - Pantalla de **Control de Acceso (Puerta)** con semáforo visual grande (Verde / Rojo).
   - Consulta de compras y gestión de estado de cada entrada individual.
   - Panel de organizador para crear eventos, agregar modalidades, cancelar eventos y ver reporte de recaudación.

---

## 📂 Estructura del Repositorio

```text
programacionTrabajo/
├── ApiGestion/                 # Solución 1: API de Gestión y Ventas
│   ├── Api/                    # Web API ASP.NET Core (.NET 8, Swagger en :5001)
│   ├── Clases/                 # Lógica de dominio, repositorios, generador de códigos
│   ├── Testing/                # Pruebas unitarias NUnit de ApiGestion (9 tests en verde)
│   └── Solucion.sln            # Solución Visual Studio de Gestión
│
├── ApiValidacion/              # Solución 2: API de Validación en Puerta
│   ├── Api/                    # Web API ASP.NET Core (.NET 8, Swagger en :5002)
│   ├── Clases/                 # Lógica del validador de entradas atómico
│   ├── Testing/                # Pruebas unitarias NUnit de ApiValidacion (7 tests en verde)
│   └── Solucion.sln            # Solución Visual Studio de Validación
│
├── CarpetaFrontend/            # Frontend Web (Vanilla HTML/CSS/JS)
│   ├── index.html              # Pantallas del sistema (Catálogo, Puerta, Compras, Admin)
│   ├── styles.css              # Estilos modernos y responsivos
│   └── app.js                  # Lógica cliente, consumo de ambas APIs y mapas Leaflet
│
├── data/                       # Almacenamiento compartido en disco (rutas relativas)
│   ├── usuarios.json           # 10 usuarios precargados (2 Organizadores, 8 Compradores)
│   ├── eventos.json            # Eventos, coordenadas y modalidades
│   └── compras.json            # Compras y entradas individuales generadas
│
└── README.md                   # Documentación del proyecto
```

---

## 🚀 Cómo Ejecutar el Proyecto

### Requisitos Previos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.
- Navegador web moderno (Chrome, Edge, Firefox).

---

### Paso 1: Ejecutar las Pruebas Unitarias (NUnit)

Para verificar que toda la lógica de negocio y las reglas están en verde:

```bash
# Tests de ApiGestion (9 pruebas unitarias)
dotnet test programacionTrabajo/ApiGestion/Solucion.sln

# Tests de ApiValidacion (7 pruebas unitarias)
dotnet test programacionTrabajo/ApiValidacion/Solucion.sln
```

---

### Paso 2: Iniciar las APIs

Abra dos terminales separadas en la raíz del proyecto:

#### Terminal 1 — API de Gestión (Puerto 5001):
```bash
cd programacionTrabajo/ApiGestion/Api
dotnet run
```
> Swagger UI disponible en: **http://localhost:5001/swagger**

#### Terminal 2 — API de Validación (Puerto 5002):
```bash
cd programacionTrabajo/ApiValidacion/Api
dotnet run
```
> Swagger UI disponible en: **http://localhost:5002/swagger**

---

### Paso 3: Abrir el Frontend

Abra directamente el archivo `programacionTrabajo/CarpetaFrontend/index.html` en su navegador (haciendo doble clic o con una extensión como Live Server).

---

## 👥 Usuarios Precargados (`usuarios.json`)

El sistema incluye los 10 usuarios requeridos en el anexo, identificados por su DNI (sin contraseñas):

| DNI | Nombre | Rol | Acciones Permitidas |
|---|---|---|---|
| **30111222** | Lucía Fernández | **Organizador** | Crear/editar/cancelar eventos, agregar modalidades, ver recaudación. |
| **29888777** | Martín Aguirre | **Organizador** | Crear/editar/cancelar eventos, agregar modalidades, ver recaudación. |
| **40123456** | Sofía Gómez | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **38456789** | Nicolás Pereyra | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **41234567** | Valentina Ríos | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **37654321** | Tomás Ibáñez | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **42345678** | Camila Suárez | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **39876543** | Agustín Molina | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **43456789** | Julieta Torres | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **36765432** | Bruno Acosta | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |

---

## 📋 Endpoints de las APIs REST

### API de Gestión (`http://localhost:5001`)

| Método | Endpoint | Rol Requerido | Descripción |
|---|---|---|---|
| `GET` | `/api/usuarios` | — | Lista todos los usuarios precargados. |
| `GET` | `/api/eventos` | — | Lista los eventos disponibles con sus modalidades. |
| `GET` | `/api/eventos/{id}` | — | Detalle de un evento y sus modalidades. |
| `POST` | `/api/eventos` | **Organizador** (`X-Dni`) | Crea un nuevo evento. |
| `PUT` | `/api/eventos/{id}` | **Organizador** (`X-Dni`) | Edita los datos de un evento. |
| `PUT` | `/api/eventos/{id}/cancelar` | **Organizador** (`X-Dni`) | Cancela un evento (bloquea ventas futuras). |
| `POST` | `/api/eventos/{id}/modalidades` | **Organizador** (`X-Dni`) | Agrega una modalidad con precio, cupo y beneficios. |
| `POST` | `/api/compras` | **Comprador** (`X-Dni`) | Registra compra, descuenta cupo y genera entradas individuales. |
| `GET` | `/api/compras/{id}` | — | Consulta detalle de compra y estado de cada entrada. |
| `GET` | `/api/compras?dni={dni}` | — | Lista compras asociadas a un DNI. |
| `GET` | `/api/reportes/recaudacion` | **Organizador** (`X-Dni`) | Reporte de recaudación y entradas vendidas por evento y modalidad. |
| `DELETE` | `/api/entradas/{codigo}` | **Comprador** (`X-Dni`) | *(Promoción)* Cancela una entrada no usada y devuelve el cupo. |

### API de Validación (`http://localhost:5002`)

| Método | Endpoint | Descripción |
|---|---|---|
| `POST` | `/api/validaciones` | Recibe `{ "codigo": "...", "idEvento": "..." }`. Valida y marca como usada la entrada o rechaza con motivo exacto. |

---

## 🎯 Reglas de Negocio Implementadas y Verificadas

1. **Cupo Máximo:** No se puede comprar si la cantidad solicitada supera el `cupoDisponible`.
2. **Eventos Cancelados o Pasados:** Se rechaza cualquier intento de compra para eventos cancelados o cuya fecha ya pasó.
3. **Descuento por Volumen:** Si se compran **5 o más entradas** de la misma modalidad en una sola operación, se aplica un **15% de descuento** sobre el total.
4. **Validación Individual:** Cada entrada posee un **código alfanumérico único de 6 caracteres** generado mediante algoritmo criptoseguro que garantiza 0 repeticiones. Cada entrada se valida por separado en la puerta.
5. **Uso Único en Puerta:** Una vez que una entrada fue validada para ingresar, se registra su fecha de uso y no puede volver a ser utilizada (`YaFueUsada`).
6. **Validación de Evento:** Una entrada solo es válida para el evento con el que fue comprada. Si se intenta validar en otro evento, es rechazada (`EventoIncorrecto`).
7. **Control de Acceso por Roles:** Los endpoints administrativos (`POST /api/eventos`, `PUT /cancelar`, `/modalidades`, `/reportes/recaudacion`) exigen DNI de un usuario con rol `Organizador` (vía Header `X-Dni`). Si se envía el DNI de un `Comprador`, el servidor responde `403 Forbidden`.

---

## 🌟 Sección de Promoción

- **Mapa Interactivo con Leaflet y OpenStreetMap:** En la vista de detalle de cada evento, el frontend renderiza un mapa centrado en las coordenadas geográficas (`latitud` y `longitud`) del evento con un marcador interactivo y popup con la dirección y nombre.
- **Cancelación de Entrada Individual:** Un comprador puede cancelar una entrada individual comprada siempre que **no haya sido validada en puerta** y **la fecha del evento no haya pasado**. Al cancelarse, la entrada queda deshabilitada y el cupo disponible de la modalidad se restablece automáticamente.

---

## 🎬 Guión para Video Demostrativo (1 a 2 minutos)

1. **Inicio y Alta de Evento:**
   - Seleccionar a *Lucía Fernández (Organizador)* en el encabezado.
   - Ir a la pestaña **⚙️ Gestión de Eventos** y crear un nuevo evento (ej: *"Recital Acústico"* con fecha futura y lugar).
   - Agregar una modalidad (ej: *"Platea General"*, Precio: $10.000, Cupo: 20).
2. **Venta de Entradas:**
   - Cambiar de usuario a *Sofía Gómez (Comprador)*.
   - Ir a **📅 Catálogo de Eventos**, abrir el evento creado, observar el mapa interactivo y seleccionar 5 entradas para evidenciar el descuento del 15%.
   - Confirmar la compra y mostrar los códigos de 6 dígitos generados.
3. **Validación Exitosa en Puerta:**
   - Ir a la pestaña **🚪 Control de Acceso (Puerta)** (conectada a `ApiValidacion` en el puerto 5002).
   - Ingresar el primer código de entrada y presionar *Validar Ingreso* -> Se muestra el cartel gigante **VERDE: INGRESO AUTORIZADO**.
4. **Intento de Doble Validación (Rechazo):**
   - Presionar nuevamente *Validar Ingreso* con el mismo código -> Se muestra el cartel gigante **ROJO: ACCESO DENEGADO (La entrada ya fue utilizada)**.
5. **Consulta y Cancelación (Promoción):**
   - Ir a **🔎 Consultar Compra**, buscar la compra y mostrar que una entrada figura como *Usada* y otra como *Disponible*.
   - Cancelar una de las entradas disponibles y verificar que el cupo se restablece en el catálogo.