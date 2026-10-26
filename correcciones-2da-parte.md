# Correcciones 2da entrega TP Final (Frontend) - Escultores

**Integrantes:** Uriel Callebaut, Faustino Trivelli, Marcos Weppler (en condición de promoción)
**Fecha de corrección:** 24/10/2026
**Qué se evaluó:** el estado del repositorio en la rama `main`. El último commit dentro del plazo (16/10/2026 23:59) es `51b878a` (16/10 13:40, Uriel). Después del vencimiento hay un commit:

- `2cc6db7`, 17/10 15:27, Marcos: "Agregado link al video demo (Disculpas por la tardanza)". Agrega el link al video en el README y modifica `data/compras.json` y `data/eventos.json` con los datos que quedaron de grabar la demo. El video también se grabó el 17/10.

Ese commit se acepta para el coloquio, pero queda registrado como entrega fuera de término. Lo tengo en cuenta junto con lo que se les dijo en la 1ra entrega sobre la promoción (ver la sección Promoción).

---

## Estado general

La entrega está casi completa respecto de las secciones 8 y 11. Las dos soluciones compilan sin errores ni advertencias, y los tests pasan todos: 64 en Gestión y 17 en Validación. Las dos APIs levantan siguiendo el README, en los puertos 5001 y 5002, y el frontend consume las dos. Las cinco pantallas existen y el circuito completo funciona: elegir usuario, ver el catálogo, comprar varias entradas con descuento, validar una entrada en la puerta, ver el rechazo al revalidarla y verla como usada en la consulta de la compra. Hay también pantallas de gestión de eventos y de reporte, que no eran obligatorias.

Con respecto a la 1ra entrega, el backend mejoró mucho. Corrigieron el reporte de recaudación, el estado de la entrada cancelada, la capa de Validación (ahora tiene dominio, repositorios y el controller en el proyecto de la API), los DTOs de respuesta, el encapsulamiento, las excepciones propias y los tests que faltaban.

Lo que falta para el coloquio:

- En el detalle del evento, un organizador ve el panel de compra completo con el botón "Comprar Entradas".
- La pantalla de control de acceso deja de servir desde el momento en que empieza el evento: el evento ya no aparece en la lista y no se puede validar ninguna entrada.
- En el mapa, si al crear el evento no se usa "Buscar en Mapa", el evento queda ubicado en el Obelisco, esté donde esté.
- Quedan pendientes dos observaciones de la 1ra entrega: el README describe cosas que no existen y el proyecto de lógica de Validación sigue declarado como proyecto Web.

---

## Checklist de entregables

| Entregable | Estado | Comentario | ¿Para el coloquio? |
|---|---|---|---|
| Pantalla 1: elegir usuario | ✅ | Desplegable con los 10 usuarios y su rol. Se guarda en `localStorage` y se mantiene entre páginas. El menú cambia según el rol. | — |
| Pantalla 2: catálogo | ✅ | Muestra los eventos disponibles con sus modalidades. El organizador puede incluir los cancelados y finalizados, que se distinguen con un distintivo. Si la API está caída, se muestra un mensaje claro. | — |
| Pantalla 3: detalle y compra | ⚠️ | La compra, los códigos, el descuento y el rechazo por cupo funcionan. Pero el organizador ve el botón de compra, y en eventos cancelados, finalizados o agotados el botón queda habilitado con un mensaje que confunde. | Obligatorio (organizador) / Recomendado (resto) |
| Pantalla 4: control de acceso (obligatoria para equipos de 3) | ⚠️ | Valida, rechaza la revalidación, el código inexistente, la entrada de otro evento y la cancelada, con el motivo. Pero solo lista los eventos que todavía no empezaron, así que el día del evento deja de funcionar. Además, el resultado no se muestra en verde o en rojo. | Obligatorio |
| Pantalla 5: consulta de compra (obligatoria para equipos de 3) | ✅ | Muestra cada entrada con su estado: disponible, usada (con fecha y hora) o cancelada. Hay accesos rápidos a las compras del comprador. | — |
| Rol en la interfaz | ⚠️ | El comprador no ve las opciones de organizador y, si entra por URL a sus páginas, lo redirige. Falta ocultar el panel de compra al organizador. | Obligatorio |
| Sin frameworks | ✅ | HTML, CSS y JS sin frameworks. Solo usan Google Fonts y el iframe de Google Maps. | — |
| Integración de las dos APIs | ✅ | Una entrada comprada en la pantalla 3 se valida en la 4 sin reiniciar nada. Después de reiniciar cada API los datos siguen. | — |
| README (APIs y frontend) | ⚠️ | Las instrucciones para levantar todo alcanzan. Pero la descripción del frontend, del mapa y de los tests no coincide con lo entregado (pendiente de la 1ra entrega). | Para promoción (pendiente de la 1ra entrega) |
| Tests en verde en las dos lógicas | ✅ | Gestión 64/64, Validación 17/17. | — |
| Video de la demo | ⚠️ | Dura 1:36 y muestra todo lo pedido. Se entregó el 17/10, fuera de término. | — (queda registrado) |
| Estructura de carpetas | ✅ | `ApiGestion`, `ApiValidacion` y `CarpetaFrontend` en la raíz, más `data/`. | — |
| Google Maps (promoción) | ⚠️ | El mapa de Google se ve con un marcador. Cambiaron el modelo (latitud y longitud) en dominio, persistencia, API y frontend. Falla cuando no se busca la dirección: quedan las coordenadas del Obelisco. | Para promoción |
| Cancelación de entradas (promoción) | ✅ | Se cancela desde la consulta de la compra. Se rechaza si la entrada ya fue usada o el evento ya empezó. El cupo y el total se ajustan, y el reporte queda consistente. | — |

---

## Pantalla por pantalla

### Pantalla 1: elegir usuario

Probé con un organizador (Lucía Fernández) y con un comprador (Sofía Gómez). El desplegable muestra nombre, rol y DNI, y el distintivo del rol cambia de color. El usuario elegido se guarda en `localStorage` y se mantiene al cambiar de página. Al pasar de organizador a comprador desaparecen del menú Control de Acceso, Gestión de Eventos y Reporte, y también el filtro "Incluir cancelados y finalizados" del catálogo. El DNI se manda a la API en el header `X-Dni`. Está bien resuelto.

- **[Recomendado]** La primera vez que se abre la página, sin nada guardado, queda elegido el primer usuario de la lista, que es un organizador. Conviene que no haya nadie elegido hasta que el usuario elija, o al menos que el valor por defecto sea explícito.

### Pantalla 2: catálogo

Como comprador, el catálogo muestra solo los eventos que se pueden comprar, porque la API ahora los filtra: es el pendiente de la 1ra entrega y está resuelto. Como organizador, con el filtro activado aparecen los cancelados ("Cancelado") y los pasados ("Finalizado"). Con la API de Gestión apagada aparece "No se pudo conectar con ApiGestion" y un botón para reintentar, que está muy bien.

- **[Recomendado]** "Concierto Sunset 2026" (el del 25/12) no tiene modalidades, pero al comprador le aparece con el botón "Comprar Entradas". Si el evento no tiene modalidades, el botón no debería invitar a comprar.
- **[Recomendado]** Varios eventos de los datos de prueba tienen como lugar un texto como "Ubicacion (-34.6362, -58.4003)", que quedó de la versión anterior del mapa. Para el coloquio, dejen datos prolijos: eventos futuros con modalidades y un lugar legible.

### Pantalla 3: detalle del evento y compra

Lo que probé y funcionó:

- Compra de 2 entradas de "Pase Estudiante" con Sofía: el modal muestra el ID de la compra, el total ($16.000) y los dos códigos de 6 caracteres. El cupo baja de 50 a 48 sin recargar la página.
- Compra de 5: el resumen muestra subtotal $40.000, descuento del 15% de $6.000 y total $34.000, y la API cobra eso mismo. Cada entrada queda a $6.800.
- Superar el cupo: en una modalidad con cupo 3 pedí 4 y la API rechazó con "No hay suficiente cupo disponible en 'Campo'. Cupo restante: 3.". El mensaje del backend se muestra tal cual, y se entiende.
- Cantidad 0 o negativa: no deja comprar.
- El botón "Ver Detalle de la Compra" del modal lleva a la consulta con la compra ya cargada.
- Un ID de evento inexistente avisa y vuelve al catálogo.

Lo que hay que corregir:

- **[Obligatorio para el coloquio]** **El organizador ve el panel de compra.** Con Lucía Fernández, el detalle de "Conferencia de Tecnología e IA 2026" muestra "Adquisición de Entradas", con la modalidad, la cantidad, el total y el botón "Comprar Entradas" habilitado. Recién al tocar el botón aparece un `alert` que dice que hace falta un comprador. La sección 8 dice que la compra es "solo visible para un comprador", y la sección 6 pide que la interfaz no muestre opciones para las que el usuario no tiene permiso. En `app.js`, `seleccionarUsuario` ya oculta los elementos con la clase `role-comprador-only`, pero esa clase no la usa ningún HTML. El panel `panelCompraComprador` de `comprar-entrada.html` tiene que ocultarse para el organizador.
- **[Recomendado]** **Evento cancelado, finalizado, agotado o sin modalidades: el botón queda habilitado.** En el detalle de un evento cancelado, el aviso rojo "Evento cancelado" está bien, pero el desplegable de modalidad aparece vacío y el botón "Confirmar Compra" está azul y habilitado. Al tocarlo, el mensaje es "Por favor seleccione una modalidad de entrada.", que no explica nada. Pasa lo mismo con un evento pasado y con un evento con todas sus modalidades agotadas. En el caso del agotado ni siquiera hay aviso: el comprador ve un desplegable vacío y un botón activo. La causa es que, como todas las opciones quedan deshabilitadas, `actualizarInfoModalidadSeleccionada` no encuentra la modalidad y termina antes de llegar a la parte que deshabilita el botón y le pone "Evento Cancelado" o "Cupo Agotado". Ese texto del botón ya está programado, pero nunca se llega a mostrar.
- **[Recomendado]** Con cantidad -3, el resumen muestra "Total a pagar: $-24.000". La compra se rechaza, pero el resumen no debería calcular con cantidades inválidas.
- **[Recomendado]** El texto del botón cambia sin criterio: dice "Comprar Entradas" al cargar y "Confirmar Compra" después de una compra o en los eventos no disponibles. Elijan uno.
- **[Recomendado]** El bloque del resumen de precio (`price-summary`) no tiene estilos en `styles.css` y se ve como texto suelto, a diferencia del resto de la pantalla (ver Calidad del código).

### Pantalla 4: control de acceso

Probé con Lucía Fernández, seleccionando "Conferencia de Tecnología e IA 2026":

| Caso | Resultado |
|---|---|
| Código recién comprado (`7MGJAK`) | "Acceso autorizado / Ingreso permitido", con evento, modalidad y hora |
| El mismo código otra vez | "Entrada rechazada", motivo "La entrada ya fue utilizada el 24/10/2026 09:15:23." |
| Código inexistente (`ZZZ999`) | Rechazada: "no existe en el sistema" |
| Entrada de la Conferencia validada con "concierto de rock" seleccionado | Rechazada: "La entrada no corresponde a este evento (es para 'Conferencia...')" |
| Código en minúsculas | Lo acepta y lo pasa a mayúsculas |
| Entrada cancelada por el comprador (`8P9TYC`) | Rechazada: "fue cancelada por el comprador" |
| Sin elegir evento | Avisa que hay que elegir el evento |

El historial de la sesión es un buen agregado. Los motivos son claros. Hacer obligatorio el evento corrige una observación de la 1ra entrega.

- **[Obligatorio para el coloquio]** **La puerta no sirve una vez que empezó el evento.** El desplegable de eventos se carga con `GET /api/eventos` (`inicializarPuerta` en `app.js`), que ahora devuelve solo los eventos que todavía no ocurrieron. Como la API de Validación exige el evento, en cuanto llega la hora de inicio el evento desaparece de la lista y no se puede validar ninguna entrada, que es justo el momento en que se usa esta pantalla. Hoy pasa con "Festival Primavera Sound 2026" (15/10): sus entradas sin usar (`OMJ4YL`, `8TMHAP`, `UQ853M`) no se pueden validar desde el frontend porque el evento no aparece. Si "concierto de rock" empieza el 31/10 a las 18:21, a las 18:22 ya no se puede controlar. La puerta tiene que poder elegir los eventos del día, aunque ya hayan empezado. Decidan con qué criterio (por ejemplo, los no cancelados del día o de las últimas horas) y resuélvanlo con un endpoint o un filtro pensado para esto, no con el del catálogo.
- **[Recomendado]** **El resultado no se ve en verde o en rojo.** En `mostrarResultadoValidacion` la tarjeta recibe las clases `success` y `error`, pero en `styles.css` los estilos se llaman `result-success` y `result-danger`. Por eso "Ingreso permitido" y "Entrada rechazada" se ven iguales, en blanco, y solo cambia el texto. Para una pantalla que se usa en la puerta, el color es lo primero que hay que ver. El README promete un "semáforo visual grande (Verde / Rojo)" que hoy no existe.
- **[Recomendado]** Con la API de Validación apagada, la pantalla muestra "Acceso denegado / Entrada rechazada" con el motivo "Error al conectar con la API de Validación". Un error del sistema no es un rechazo de la entrada: el personal de la puerta podría no dejar pasar a alguien con una entrada válida. Muéstrenlo como un error distinto.
- **[Recomendado]** Debajo del desplegable sigue el texto "Opcional. Si seleccionas un evento puntual...", pero el evento ahora es obligatorio. Actualicen el texto.
- **[Recomendado]** La validación en la puerta quedó solo para el organizador. Es una decisión válida (no hay un rol "personal de puerta"), pero tienen que poder justificarla, porque la API de Validación no pide DNI.

### Pantalla 5: consulta de compra

Como Sofía, la compra de 2 entradas mostró las dos como "Usada (24/10/2026, 09:15:23)" después de validarlas en la puerta, sin recargar nada más. La compra de 5 mostró las cinco "Disponible", con un botón "Cancelar Entrada" en cada una. Como otro comprador (Valentina Ríos), la API rechaza ver una compra ajena, y como organizador se puede consultar cualquier compra. Un ID inexistente avisa "Compra no encontrada".

- **[Recomendado]** Cuando la API devuelve 403 (compra de otro comprador), el frontend muestra "Error al consultar la compra." y no el motivo que manda la API ("No tiene permiso para consultar una compra de otro comprador."). En `buscarCompra` solo se trata el 404. Usen el mensaje de error que devuelve el backend, como ya hacen en la compra.
- **[Recomendado]** Después de cancelar una entrada, "Total Abonado" pasa de $34.000 a $27.200. Lo que abonó el comprador fue $34.000: lo que bajó es el total vigente. Cambien la etiqueta o muestren las dos cosas (lo pagado y lo devuelto).
- **[Recomendado]** La compra precargada `6087528b...` tiene una entrada cancelada pero sigue con un total de $63.750 en `compras.json`, el de las 5 entradas, mientras que el reporte le calcula $51.000. Es un dato viejo, anterior a la corrección del total. Regeneren los datos de prueba para que el coloquio no muestre dos números distintos para la misma compra.

---

## Rol en la interfaz

- El comprador no ve en el menú Control de Acceso, Gestión de Eventos ni Reporte. Si entra por URL directa a `control-acceso.html`, `gestion-eventos.html` o `reporte-recaudacion.html`, aparece "Acceso denegado" y vuelve al catálogo. ✅
- En el catálogo y en el detalle, el comprador no ve los botones para cancelar eventos o modalidades. ✅
- El backend responde 403 cuando un comprador intenta crear o editar un evento, ver el reporte o pedir los eventos no disponibles, y cuando un organizador intenta comprar. ✅
- **[Obligatorio para el coloquio]** El organizador ve el panel de compra (desarrollado en Pantalla 3).
- **[Recomendado]** La protección de las páginas de organizador depende de que la lista de usuarios haya cargado. Con la API de Gestión apagada, `gestion-eventos.html` muestra todos los formularios de alta y cancelación sin ningún usuario elegido. Además, mientras carga, el contenido se ve un instante antes de la redirección. Las acciones igual fallan en el backend, pero conviene ocultar el contenido hasta saber quién es el usuario.
- **[Recomendado]** En el reporte, un comprador que entra por URL ve dos `alert` seguidos con el mismo mensaje: uno de `seleccionarUsuario` y otro de `cargarReporteRecaudacion`.

### Pantallas extra (gestión de eventos y reporte)

Probé crear un evento con la dirección "Teatro Lasserre, Rafaela, Santa Fe" y "Buscar en Mapa", agregarle dos modalidades, cancelar una modalidad y cancelar el evento desde la tarjeta del catálogo. Todo funcionó y se reflejó en el catálogo y en el detalle. Si se intenta crear un evento con fecha pasada, se muestra el error del backend. El reporte cuadra con las cancelaciones: el evento de prueba informó 4 entradas y $3.400 después de cancelar una de las 5 compradas con descuento.

- **[Recomendado]** En el reporte, los eventos ya finalizados ("Festival Primavera Sound 2026", "Noche de Jazz & Blues") tienen el botón "Cancelar", aunque en el catálogo y en el detalle no lo tienen. Unifiquen el criterio.
- **[Recomendado]** El reporte suma lo recaudado por los eventos cancelados ("Concierto Sunset 2026": $102.000). Si se cancela un evento, ¿se devuelve el dinero? Decidan qué muestra el reporte en ese caso y tengan la respuesta para el coloquio.

---

## Pendientes de la 1ra entrega

| Observación de la 1ra entrega | Estado | Comentario |
|---|---|---|
| Controller de Validación dentro del proyecto de lógica | Parcialmente resuelto | El controller ya está en `ApiValidacion/Api`, pero `ApiValidacion/Clases/Clases.csproj` sigue con `Sdk="Microsoft.NET.Sdk.Web"`. Tiene que ser una biblioteca de clases común, como el `Clases` de Gestión. **[Para promoción]** (condición para mantener la promoción). |
| `Api.http` con `weatherforecast` | Resuelto | Ya no está el ejemplo. **[Recomendado]** Los archivos quedaron solo con la variable del host: agreguen los pedidos de prueba o bórrenlos. |
| Video de 21 MB en el repo | Resuelto | Se borró y ahora hay un link en el README. |
| Endpoints que devuelven entidades y no DTOs | Resuelto | Hay Request y Response DTOs y un mapper en cada API. |
| DNI repetido en `GET /api/usuarios` | Resuelto | Ahora sale un solo `dni`. |
| `GET /api/compras` sin DNI devuelve todo | Resuelto | Responde 403 sin DNI. El comprador ve solo sus compras y el organizador todas. |
| `GET /api/eventos` con cancelados y pasados | Resuelto | Filtra por defecto. Con `incluirNoDisponibles=true` pide rol organizador. Pero ver Pantalla 4: ese filtro rompió la puerta. |
| Reporte de recaudación inconsistente al cancelar | Resuelto | Suma lo pagado por cada entrada vigente. Lo verifiqué con curl y en la pantalla. |
| Cancelación de entradas con descuento | Resuelto | Cada entrada guarda lo que efectivamente pagó, y al cancelarla se descuenta eso. Tienen que poder explicarlo. |
| Estado "NoExiste" para una entrada cancelada | Resuelto | Existe el estado `Cancelada` y la validación lo devuelve. |
| Validación sin evento | Resuelto | `IdEvento` es obligatorio. |
| Evento con fecha pasada en `eventos.json` | Sin cambios (informativo) | Ahora hay dos eventos pasados. Ténganlo en cuenta para la demo. |
| Ruta de datos dependiente del directorio | Resuelto | `DataPathHelper` busca la carpeta `data` subiendo desde el directorio de trabajo. |
| Validación sin dominio ni repositorio | Resuelto | Tiene `Dominio/` (Compra, Entrada, Evento), repositorios con interfaz y servicio. |
| Concurrencia entre los dos procesos | Sin cambios (explicar) | El riesgo sigue: `CompraService.RealizarCompra` lee las compras y las guarda al final. No se exige resolverlo, pero sí poder explicarlo. |
| DNI de la compra tomado del body o del header | Resuelto | Ahora siempre viene del header `X-Dni` (o de la query). |
| Casos de borde faltantes en los tests | Resuelto | Están todos los que se pidieron, y más. |
| Tests que verifican varias cosas a la vez | Resuelto | Ahora cada test prueba un solo comportamiento. |
| Setters públicos en las entidades | Resuelto | `private set` y métodos que validan. |
| Excepciones del framework | Resuelto | Hay excepciones propias (`ReglaNegocioException`, `AccesoDenegadoException`, etc.). |
| Doble mecanismo de inyección | Resuelto | Los servicios exigen sus dependencias por constructor. |
| DTO del reporte en la lógica | Resuelto | La lógica tiene clases de reporte y los DTOs están en la API. |
| `ModalidadService` y `EntradaRepository` sin uso | Resuelto | Se borraron. Quedaron otros métodos sin uso (ver Calidad del código). |
| Commits concentrados en pocas horas | Pendiente | En esta etapa se repite (ver Git). |
| Identidades distintas para la misma persona | Resuelto | Cada integrante commitea con un solo nombre y mail. |
| README con la carpeta `programacionTrabajo/` | Resuelto | |
| Puertos del README distintos de `launchSettings.json` | Resuelto | Ahora todo usa 5001 y 5002. |
| README que describe un mapa distinto del implementado | Pendiente | Sigue describiendo cosas que no existen (ver README). **[Para promoción]** (condición para mantener la promoción). |

---

## Calidad del código del frontend

Lo que está bien: la URL de cada API está definida una sola vez al principio de `app.js` (`API_GESTION` y `API_VALIDACION`). Todos los `fetch` usan `async`/`await` dentro de `try`/`catch`, revisan `res.ok` y, en la mayoría de los casos, muestran el `error` que devuelve el backend. Las tablas y los desplegables se arman manipulando el DOM. El usuario se guarda en `localStorage`.

- **[Recomendado]** **Clases CSS que no coinciden con el HTML y el JS.** Además del caso de la puerta, varias clases que usan el HTML o el JS no existen en `styles.css`: `price-summary`, `total-row`, `search-box-container`, `purchase-info-summary`, `info-row`, `metric-card`, `grid-2-cols`. Al revés, hay estilos que no usa nadie: `ticket-input-large`, `result-success`, `result-danger`. En `renderizarModalidadesParaCancelar` se usa `var(--gray-500)`, que no está definida (el resto del CSS usa `--slate-*`). Revisen pantalla por pantalla cómo se ve cada estado.
- **[Recomendado]** **Duplicación.** El encabezado completo (logo, selector de usuario y menú) está copiado en los seis HTML. El header `"X-Dni": state.usuarioActivo.dni.toString()` se arma a mano en más de diez `fetch`. Una función que arme los headers, o que haga el `fetch` a Gestión con el DNI, evitaría repetirlo.
- **[Recomendado]** **Un solo `app.js` de 1.400 líneas para todas las pantallas.** Cada página decide qué inicializar preguntando si existe un id (`if (document.getElementById("eventsGrid"))`). Funciona, pero es difícil de mantener. Separen por pantalla, o al menos dejen en un archivo común lo compartido (usuario, headers, API).
- **[Recomendado]** **`innerHTML` con datos sin escapar.** El nombre, el lugar y la descripción del evento, los nombres de modalidades y los mensajes de la API se insertan con `innerHTML` (`renderizarCatalogo`, `renderizarDetalleCompra`, `mostrarResultadoValidacion`, `cargarReporteRecaudacion`). Un nombre de evento con HTML se interpretaría como código. Donde son datos, usen `textContent`, como ya hacen en el detalle del evento.
- **[Recomendado]** **Código que no se usa o que no hace nada.** `actualizarDesdeInputs` escucha el evento `change` de dos inputs ocultos, que nunca lo disparan. El parámetro `buscarNombre` de `actualizarUbicacionSeleccionada` nunca recibe `true`. En el backend no se usan `EventoService.ObtenerTodos`, `Modalidad.HayCupoDisponible` ni `Usuario.EsComprador`. `GET /api/eventos/gestion` duplica lo que hace `?incluirNoDisponibles=true`. En `ApiGestion/Api/Program.cs` sigue la ruta `/eventos`, que apunta a `eventos.html` (lo borraron) y da error 500.
- **[Recomendado]** **Responsive.** Con el ancho de un celular (390 px), el selector de usuario y las tarjetas del catálogo se salen del ancho de la pantalla y aparece scroll horizontal. Hay `@media` en el CSS, pero no alcanzan.
- **[Recomendado]** **Mensajes con `alert`.** Toda la comunicación de errores y confirmaciones es con `alert`. No está mal, pero en la pantalla de compra y en la consulta convendría mostrar el mensaje en la página, como ya hacen en el catálogo y en la puerta.

---

## README y video

**README:** las instrucciones para levantar las APIs (`cd ApiGestion/Api` y `dotnet run`, lo mismo para Validación) funcionan tal cual y los puertos coinciden. Para abrir el frontend indica dos caminos: abrir `index.html` o entrar a `http://localhost:5001`, que lo sirve la propia API. Los dos funcionan.

- **[Para promoción]** (condición para mantener la promoción; pendiente de la 1ra entrega) El README tiene que describir lo que está hecho. Hoy dice:
  - que el frontend es una "SPA con Hash Routing" con URLs `#catalogo`, `#evento={id}`, etc., y "deep linking". Son seis páginas HTML separadas que se pasan datos por query string (`?id=`).
  - que el mapa es "Google Maps (Calles / Satélite sin API Key ni marcas de agua)". Es un iframe de Google Maps sin selector de capas, y la búsqueda de direcciones usa Nominatim (OpenStreetMap).
  - que la carpeta del frontend tiene solo `index.html`, `styles.css` y `app.js`.
  - que hay "19 pruebas en verde" (12 + 7). Son 64 y 17.
  - que la puerta tiene un "semáforo visual grande (Verde / Rojo)", que hoy no se ve (ver Pantalla 4).
- **[Recomendado]** Indiquen en el README cómo levantar el frontend con un servidor (Live Server o la misma API) y qué usuario usar para cada parte de la demo.

**Video:** está en Google Drive (link al final del README), es público y dura 1:36. Muestra el alta de "concierto de rock" con su ubicación en el mapa, el alta de la modalidad "platea", la compra de 5 entradas con el descuento, la validación exitosa de `UNN53M` y el rechazo al revalidarla ("ya fue utilizada"). Cumple con el contenido y la duración. Está grabado y subido el 17/10, después del vencimiento.

---

## Git y trabajo en equipo en esta etapa

Desde la corrección de la 1ra entrega (04/10) hasta hoy hay 18 commits:

- 14/10: 15 commits. Marcos a la 01:35 (un commit grande con 25 archivos), Uriel entre las 18:18 y las 20:58 (8 commits), Faustino entre las 22:58 y las 23:20 (4) y Marcos a las 23:28 y 23:44.
- 16/10: Faustino a las 11:32 y Uriel a las 13:40 (2 commits con el mismo mensaje).
- 17/10: Marcos, fuera de término.

Entre el 05/10 y el 13/10 no hay ningún commit. Los tres tocaron backend y frontend, así que en ese sentido rotaron. Pero el frontend de esta entrega es casi el mismo que el del 02/10. Lo que cambió en esta etapa son unas 100 líneas de `app.js`: el reemplazo del mapa, el header con el DNI, el filtro de cancelados y los puertos. Y ese único cambio del mapa está repartido en tres commits de tres personas en dos días: `870b9ec` (Marcos), `0f2fcb5` (Faustino) y `51b878a` (Uriel). Entre el primero y el último, `app.js` llamaba a una función (`crearUrlMapaGoogle`) que todavía no existía. Pasa lo mismo en el backend: el 14/10 a las 20:56, `f946530` usa clases de dominio y un repositorio de Validación que recién se suben a las 23:20 y 23:28, en commits de otros integrantes.

- **[Recomendado]** El enunciado pide commits distribuidos en el tiempo y que reflejen el trabajo real de cada uno. Eso ya no se puede corregir para atrás, pero en el coloquio cada integrante tiene que poder explicar y modificar cualquier parte del frontend y del backend, incluida la que figura a nombre de otro.
- **[Recomendado]** Usen mensajes de commit que digan qué cambió y por qué. "Modificado el App.js" no lo dice, y `d5df586` ("Modificado App.js...") ni siquiera toca `app.js`.

---

## Promoción (Marcos Weppler)

En la 1ra entrega se les dijo que, para mantener la promoción, todo lo que no funcionaba y todas las observaciones tenían que estar corregidas en la entrega final. El backend cumple casi todo. En la tabla de pendientes quedan dos observaciones sin resolver, el README y el proyecto de lógica de Validación, a las que se suman la entrega del video y del link fuera de término (17/10) y los puntos de Google Maps que siguen.

**Google Maps:**

- El detalle del evento muestra un mapa de Google con el marcador en la ubicación del evento, y un botón "Ver en Google Maps" que abre la ubicación en una pestaña nueva. ✅
- El mapa se ubica con **latitud y longitud**. Cambiaron el modelo en todas las capas: `Evento` (dominio) tiene `Latitud` y `Longitud`, se guardan en `eventos.json`, `EventoRequest` las valida con rangos y `EventoResponse` las devuelve, y el frontend las manda y las usa. ✅
- El alta de evento busca la dirección con Nominatim (OpenStreetMap) y muestra la vista previa en el mapa. Probé con "Teatro Lasserre, Rafaela, Santa Fe" y lo ubicó bien. ✅
- La API key no está en el repositorio. El mapa es un iframe con la URL `maps.google.com/maps?q=lat,lng&output=embed`, que no pide clave. Tienen que poder explicar la diferencia con la Maps Embed API o la Maps JavaScript API oficiales, que sí la piden, y por qué eligieron esta forma.
- **[Para promoción]** **Si no se busca la dirección, el evento queda en el Obelisco.** Los campos ocultos de latitud y longitud de `gestion-eventos.html` arrancan en -34.6037 / -58.3816, y `crearNuevoEvento` usa esos valores si no hay otros. Creé "Evento sin buscar mapa" con lugar "Club Atlético de Rafaela" sin tocar "Buscar en Mapa", y el detalle lo muestra en el centro de Buenos Aires. Lo mismo hacen `renderizarMapa` y `crearUrlMapaGoogle` con un evento sin coordenadas. Un mapa que muestra un lugar equivocado es peor que no mostrar el mapa. Si no hay coordenadas, no las inventen: obliguen a buscar la dirección antes de crear el evento, o guarden el evento sin coordenadas y muestren "Ubicación no disponible".
- **[Recomendado]** El texto de ayuda del alta dice "o hacer clic directamente sobre el mapa", pero en el iframe no se puede elegir un punto: era de la versión con Leaflet. Además, la edición de un evento no permite cambiar la ubicación desde el frontend.

**Cancelación de entradas:** se hace desde la consulta de la compra, con el botón "Cancelar Entrada" y una confirmación. El botón solo aparece para el comprador dueño de la compra y en entradas sin usar ni cancelar. Probé cancelar `8P9TYC` (de una compra de 5 con descuento): quedó "Cancelada", el cupo volvió de 43 a 44, el total vigente bajó en $6.800 (lo que pagó esa entrada) y la puerta la rechaza como cancelada. Por API verifiqué que no se puede cancelar una entrada ya usada ("No se puede cancelar una entrada que ya fue utilizada...") ni una ajena (403). El análisis de qué pasa con una entrada usada está resuelto. ✅

---

## Resumen final de correcciones

## Obligatorio para el coloquio

1. Ocultar al organizador el panel de compra del detalle del evento (`panelCompraComprador` en `comprar-entrada.html`). Hoy ve la modalidad, la cantidad y el botón "Comprar Entradas".
2. Hacer que la pantalla de control de acceso permita elegir y validar los eventos del día aunque ya hayan empezado. Hoy usa la lista del catálogo y el evento desaparece a la hora de inicio, así que en ese momento no se puede validar ninguna entrada.

## Para mantener la promoción (no obligatorio para el coloquio)

1. Google Maps: no ubicar en el Obelisco los eventos creados sin buscar la dirección. Hay que exigir la búsqueda o mostrar que no hay ubicación.
2. Actualizar el README para que describa lo que realmente está hecho: páginas separadas, el mapa que usan, la cantidad de tests y el resultado de la puerta. Quedó pendiente de la 1ra entrega.
3. Convertir `ApiValidacion/Clases` en una biblioteca de clases común, sin `Sdk.Web`. Quedó pendiente de la 1ra entrega.

## Recomendado (no obligatorio para el coloquio)

1. Corregir el botón de compra en eventos cancelados, finalizados, agotados o sin modalidades: que quede deshabilitado con el motivo, en lugar de habilitado con "seleccione una modalidad".
2. Hacer que el resultado de la puerta se vea en verde o en rojo (clases `success` y `error` contra `result-success` y `result-danger`) y revisar las demás clases CSS que no existen o no se usan.
3. Mostrar un error de conexión con la API de Validación como error del sistema y no como "Entrada rechazada".
4. Mostrar el mensaje del backend también en la consulta de compra (403) y no solo en la compra.
5. Corregir el texto "Opcional" del selector de evento de la puerta y el de "hacer clic sobre el mapa" del alta.
6. Regenerar los datos de prueba para el coloquio: totales consistentes, eventos futuros con modalidades y lugares legibles.
7. Aclarar "Total Abonado" después de cancelar una entrada (lo pagado y lo devuelto).
8. Validar la cantidad en el resumen de precio (sin totales negativos) y unificar el texto del botón de compra.
9. Ocultar el contenido de las páginas de organizador hasta saber quién es el usuario y evitar el doble `alert` del reporte.
10. Reducir la duplicación: encabezado repetido en seis HTML, armado del header `X-Dni` en cada `fetch` y un único `app.js` para todas las pantallas.
11. Usar `textContent` en lugar de `innerHTML` para los datos que vienen de la API.
12. Borrar el código sin uso: `actualizarDesdeInputs`, `ObtenerTodos`, `HayCupoDisponible`, `EsComprador`, `GET /api/eventos/gestion` y la ruta `/eventos` de `Program.cs`.
13. Arreglar el desborde horizontal en pantallas chicas.
14. Definir y unificar el criterio del reporte para eventos cancelados y finalizados (botón Cancelar y recaudación).
15. Completar los `Api.http` con pedidos de prueba o borrarlos, y agregar al README qué usuario usar para cada paso de la demo.
16. Elegir un usuario por defecto explícito (o ninguno) en la primera carga.
17. Trabajar con commits distribuidos y mensajes descriptivos.
