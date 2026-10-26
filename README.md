# TP Integrador — Sistema de Venta y Validación de Entradas para Eventos

- **Materia:** Programación 1 — UCSE
- **Equipo:** Escultores
- **Integrantes:** Uriel Callebaut, Faustino Trivelli, Marcos Weppler (en condición de promoción)
- **Tecnologías:** C# / .NET 8, ASP.NET Core Web API, NUnit (83 pruebas unitarias en verde: 66 en Gestión y 17 en Validación), Persistencia JSON compartida en disco, HTML5, CSS3, JavaScript nativo (Vanilla JS multipágina), Google Maps (Embed Iframe) y geocodificación mediante Nominatim (OpenStreetMap).

---

## Descripción General

El sistema sustituye la gestión manual en planillas de cálculo y grupos de mensajería por una solución integral compuesta por **dos APIs REST independientes que comparten la misma información persistida en disco (sin base de datos central)** y una **aplicación web frontend**:

1. **API de Gestión (`ApiGestion` - Puerto 5001):**
   - Catálogo y administración de eventos y sus modalidades de entrada.
   - Venta de entradas asociadas al DNI del comprador, con cálculo automático de **descuento por volumen del 15% para compras de 5 o más entradas** de la misma modalidad.
   - Generación de **entradas individuales con códigos únicos alfanuméricos de 6 dígitos** (garantía de cero colisiones).
   - Reportes de recaudación y entradas vendidas por evento con desglose por modalidad y compradores.
   - *(Promoción)* **Cancelación de entradas individuales** (`DELETE /api/entradas/{codigo}`) restaurando el cupo de la modalidad y recalculando el total vigente.
   - Cancelación de modalidades y eventos completos por usuarios con rol Organizador.
   - Endpoint optimizado para control de acceso en puerta (`GET /api/eventos?paraControlAcceso=true`).
   - Documentación interactiva con **Swagger UI**.

2. **API de Validación en Puerta (`ApiValidacion` - Puerto 5002):**
   - Biblioteca de clases estándar (`Microsoft.NET.Sdk`) desacoplada con dominio, repositorios y servicios propios.
   - Recibe el código de una entrada y el ID del evento actual; valida atómicamente si puede ingresar registrando su uso (`Usada = true`, fecha y hora) o rechazándola con causa explícita: inexistente, ya usada, corresponde a otro evento o evento cancelado.
   - Sincronización inmediata sobre los archivos JSON compartidos en disco.
   - Documentación interactiva con **Swagger UI**.

3. **Frontend Web (`CarpetaFrontend`):**
   - Construido íntegramente con HTML5, CSS3 y JavaScript nativo sin dependencias de frameworks externos.
   - **Arquitectura multipágina:** Seis pantallas HTML independientes comunicadas mediante parámetros de URL (`?id=`):
     - `index.html`: Catálogo de eventos disponibles con filtros por rol y estado.
     - `comprar-entrada.html`: Detalle del evento, mapa de Google Maps, selección de modalidad y panel de compra exclusivo para compradores.
     - `control-acceso.html`: Pantalla de puerta con selector obligatorio de eventos del día, semáforo visual grande (Verde para autorizado, Rojo para rechazado y Ámbar para fallo de comunicación de red) e historial de la sesión.
     - `consultar-compra.html`: Búsqueda de compra, detalle de entradas, estados (Disponible, Usada con fecha/hora, Cancelada), desglose de Total Vigente y monto reintegrado por cancelaciones, y cancelación de entradas.
     - `gestion-eventos.html`: Panel para que organizadores creen eventos con geocodificación mediante Nominatim, agreguen modalidades y cancelen modalidades existentes.
     - `reporte-recaudacion.html`: Métricas de recaudación general y tabla detallada por evento con desglose por modalidad y compradores.
   - Selector dinámico de usuario con persistencia en `localStorage`, header `X-Dni` en las peticiones y protección estricta de rutas por rol (**Organizador** vs. **Comprador**).
   - **Google Maps:** Integración mediante iframe oficial sin requerir API key ni exponer credenciales (`maps.google.com/maps?q=lat,lng&output=embed`). Si el evento no posee coordenadas exactas registradas, el sistema muestra un panel indicando "Ubicación geográfica no disponible" y permite buscar el nombre del lugar directamente en Google Maps.

---

## Estructura del Repositorio

```text
.
├── ApiGestion/                 # Solución 1: API de Gestión y Ventas
│   ├── Api/                    # Web API ASP.NET Core (.NET 8, Swagger en :5001)
│   ├── Clases/                 # Lógica de dominio, repositorios, servicios
│   ├── Testing/                # Pruebas unitarias NUnit de ApiGestion (66 tests en verde)
│   └── Solucion.sln            # Solución Visual Studio de Gestión
│
├── ApiValidacion/              # Solución 2: API de Validación en Puerta
│   ├── Api/                    # Web API ASP.NET Core (.NET 8, Swagger en :5002)
│   ├── Clases/                 # Biblioteca de clases .NET estándar (dominio y repositorios)
│   ├── Testing/                # Pruebas unitarias NUnit de ApiValidacion (17 tests en verde)
│   └── Solucion.sln            # Solución Visual Studio de Validación
│
├── CarpetaFrontend/            # Frontend Web (HTML5/CSS3/Vanilla JS)
│   ├── index.html              # Catálogo de eventos
│   ├── comprar-entrada.html    # Detalle de evento, mapa y compra
│   ├── control-acceso.html     # Validación de entradas en puerta
│   ├── consultar-compra.html   # Consulta de compra y cancelación de entradas
│   ├── gestion-eventos.html    # Alta de eventos y modalidades (Organizador)
│   ├── reporte-recaudacion.html# Reporte de ingresos y compradores (Organizador)
│   ├── styles.css              # Hoja de estilos responsiva
│   └── app.js                  # Lógica cliente y conexión con APIs
│
├── data/                       # Almacenamiento compartido en disco
│   ├── usuarios.json           # 10 usuarios precargados (2 Organizadores, 8 Compradores)
│   ├── eventos.json            # Eventos, coordenadas y modalidades
│   └── compras.json            # Compras y entradas individuales generadas
│
└── README.md                   # Documentación técnica del proyecto
```

---

## Cómo Ejecutar el Proyecto

### Requisitos Previos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.
- Navegador web moderno (Chrome, Edge, Firefox).

---

### Paso 1: Ejecutar las Pruebas Unitarias (83 tests en verde)

```bash
dotnet test ApiGestion/Solucion.sln
dotnet test ApiValidacion/Solucion.sln
```

---

### Paso 2: Iniciar las APIs

Abra dos consolas separadas en la raíz del repositorio:

#### Terminal 1 — API de Gestión (Puerto 5001):
```bash
cd ApiGestion/Api
dotnet run
```
> Swagger UI: **http://localhost:5001/swagger**

#### Terminal 2 — API de Validación (Puerto 5002):
```bash
cd ApiValidacion/Api
dotnet run
```
> Swagger UI: **http://localhost:5002/swagger**

---

### Paso 3: Abrir el Frontend

Existen dos alternativas sencillas para visualizar la interfaz:

1. **A través de la propia API de Gestión (Recomendado):**
   Con la API de Gestión ejecutándose, acceda directamente en el navegador a:
   **http://localhost:5001**

2. **A través de un servidor local (ej. Live Server de VS Code):**
   Abra la carpeta del proyecto en VS Code y ejecute Live Server sobre `CarpetaFrontend/index.html` (o abra el archivo en el navegador).

---

## Guía para la Demo del Coloquio

Para replicar el circuito completo y verificar todas las funcionalidades:

1. **Iniciar como Comprador:**
   - Seleccionar en el desplegable superior a **Sofía Gómez (Comprador)**.
   - En el catálogo, ingresar al evento `"Conferencia de Tecnología e IA 2026"`.
   - Seleccionar una modalidad (ej. "Pase Estudiante"), indicar 5 entradas para verificar el **15% de descuento** en tiempo real y hacer clic en **Comprar Entradas**.
   - Guardar los códigos generados en el modal de confirmación.

2. **Validar en Puerta (Control de Acceso):**
   - Cambiar de usuario a **Lucía Fernández (Organizador)**.
   - Ir a la pestaña **Control de Acceso**.
   - Seleccionar el evento correspondiente en el desplegable.
   - Ingresar uno de los códigos comprados y validar: se observará la tarjeta en **Verde (Ingreso Permitido)**.
   - Reingresar el mismo código: se observará la tarjeta en **Rojo (Entrada Rechazada)** con el motivo *"La entrada ya fue utilizada"*.

3. **Consultar Compra y Cancelar Entrada:**
   - Volver a seleccionar a **Sofía Gómez (Comprador)** e ir a **Consultar Compra**.
   - Ingresar el ID de la compra (o utilizar los botones de compras rápidas).
   - Verificar que la entrada validada figura en estado `"Usada"` con su fecha y hora.
   - En una entrada que continúe en estado `"Disponible"`, hacer clic en **Cancelar Entrada**.
   - Confirmar: la entrada pasará a `"Cancelada"`, el cupo del evento se restablecerá y el resumen mostrará el **Total Vigente** junto con el monto **Reintegrado por cancelaciones**.

4. **Gestión y Reporte (Organizador):**
   - Cambiar a **Lucía Fernández (Organizador)**.
   - En **Gestión de Eventos**, dar de alta un nuevo evento ingresando su nombre y dirección, pulsar **Buscar en Mapa** para geolocalizarlo en el mapa interactivo y guardarlo.
   - Agregar una modalidad al evento creado.
   - En **Reporte de Recaudación**, comprobar las métricas generales de recaudación, las entradas vigentes vendidas y el listado de compradores.

---

## Usuarios Precargados (`usuarios.json`)

| DNI | Nombre | Rol | Acciones Permitidas |
|---|---|---|---|
| **30111222** | Lucía Fernández | **Organizador** | Crear/editar/cancelar eventos y modalidades, ver reporte de recaudación, puerta. |
| **29888777** | Martín Aguirre | **Organizador** | Crear/editar/cancelar eventos y modalidades, ver reporte de recaudación, puerta. |
| **40123456** | Sofía Gómez | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **38456789** | Nicolás Pereyra | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **41234567** | Valentina Ríos | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **37654321** | Tomás Ibáñez | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **42345678** | Camila Suárez | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **39876543** | Agustín Molina | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **43456789** | Julieta Torres | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |
| **36765432** | Bruno Acosta | **Comprador** | Comprar entradas, ver sus compras, cancelar entradas no usadas. |

---

## Endpoints de las APIs REST

### API de Gestión (`http://localhost:5001`)

| Método | Endpoint | Rol Requerido | Descripción |
|---|---|---|---|
| `GET` | `/api/usuarios` | — | Lista todos los usuarios precargados. |
| `GET` | `/api/eventos` | — | Lista eventos disponibles (`?paraControlAcceso=true` para puerta, `?incluirNoDisponibles=true` para organizador). |
| `GET` | `/api/eventos/{id}` | — | Detalle de un evento y sus modalidades. |
| `POST` | `/api/eventos` | **Organizador** (`X-Dni`) | Crea un nuevo evento con coordenadas y dirección. |
| `PUT` | `/api/eventos/{id}` | **Organizador** (`X-Dni`) | Modifica los datos de un evento. |
| `PUT` | `/api/eventos/{id}/cancelar` | **Organizador** (`X-Dni`) | Cancela un evento completo. |
| `POST` | `/api/eventos/{id}/modalidades` | **Organizador** (`X-Dni`) | Agrega una modalidad con precio, cupo y beneficios. |
| `PUT` | `/api/eventos/{id}/modalidades/{idModalidad}/cancelar` | **Organizador** (`X-Dni`) | Cancela una modalidad específica. |
| `POST` | `/api/compras` | **Comprador** (`X-Dni`) | Registra compra, descuenta cupo y genera entradas individuales. |
| `GET` | `/api/compras/{id}` | — | Consulta detalle de compra y estado de cada entrada. |
| `GET` | `/api/compras?dni={dni}` | — | Lista compras asociadas al DNI indicado. |
| `GET` | `/api/reportes/recaudacion` | **Organizador** (`X-Dni`) | Reporte de recaudación y entradas vendidas por evento. |
| `DELETE` | `/api/entradas/{codigo}` | **Comprador** (`X-Dni`) | *(Promoción)* Cancela una entrada no usada y devuelve el cupo. |

### API de Validación (`http://localhost:5002`)

| Método | Endpoint | Descripción |
|---|---|---|
| `POST` | `/api/validaciones` | Recibe `{ "codigo": "...", "idEvento": "..." }`. Valida y marca como usada la entrada o rechaza con motivo exacto. |

---

## Link al Video Demo
- **Enlace:** [https://drive.google.com/file/d/1-2FuJ2cG8K09oveOL-iZXee2Gz38XIQQ/view?usp=drive_link](https://drive.google.com/file/d/1-2FuJ2cG8K09oveOL-iZXee2Gz38XIQQ/view?usp=drive_link)
