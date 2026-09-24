// =========================================================================
// TicketFlow — Frontend JavaScript Nativo (Vanilla JS)
// Consumo de ApiGestion (puerto 5001) y ApiValidacion (puerto 5002)
// =========================================================================

const API_GESTION = "http://localhost:5001/api";
const API_VALIDACION = "http://localhost:5002/api";

// Estado global de la aplicación
const state = {
  usuarios: [],
  usuarioActivo: null,
  eventos: [],
  eventoSeleccionado: null,
  compraActual: null,
  historialValidaciones: [],
  leafletMap: null,
  mapMarker: null
};

// =========================================================================
// Inicialización al cargar el DOM
// =========================================================================
document.addEventListener("DOMContentLoaded", async () => {
  setupNavigation();
  setupEventListeners();
  await cargarUsuarios();
  await cargarEventos();
});

// =========================================================================
// 1. Navegación por pestañas y control de permisos
// =========================================================================
function setupNavigation() {
  const tabs = document.querySelectorAll(".tab-btn");
  tabs.forEach(tab => {
    tab.addEventListener("click", () => {
      const targetId = tab.dataset.tab;
      mostrarPestaña(targetId);
    });
  });
}

function mostrarPestaña(targetId) {
  // Regla de seguridad: Vistas administrativas exclusivas para Organizadores / Administradores
  const vistasExclusivasOrganizador = ["puertaView", "organizadorView", "reporteView"];
  if (vistasExclusivasOrganizador.includes(targetId) && state.usuarioActivo?.rol !== "Organizador") {
    targetId = "catalogoView";
  }

  document.querySelectorAll(".tab-btn").forEach(t => t.classList.remove("active"));
  document.querySelectorAll(".section-view").forEach(s => s.classList.remove("active"));

  const targetTab = document.querySelector(`.tab-btn[data-tab="${targetId}"]`);
  const targetSection = document.getElementById(targetId);

  if (targetTab) targetTab.classList.add("active");
  if (targetSection) targetSection.classList.add("active");

  if (targetId === "reporteView" && state.usuarioActivo?.rol === "Organizador") {
    cargarReporteRecaudacion();
  } else if (targetId === "consultaCompraView") {
    actualizarMisComprasRapidas();
  }
}

// =========================================================================
// 2. Gestión de Usuarios y Roles
// =========================================================================
async function cargarUsuarios() {
  try {
    const res = await fetch(`${API_GESTION}/usuarios`);
    if (!res.ok) throw new Error("No se pudo cargar la lista de usuarios.");
    state.usuarios = await res.json();

    const select = document.getElementById("userSelect");
    select.innerHTML = "";

    state.usuarios.forEach((u) => {
      const opt = document.createElement("option");
      opt.value = u.dni;
      opt.textContent = `${u.nombre} (${u.rol}) — DNI: ${u.dni}`;
      select.appendChild(opt);
    });

    // Restaurar usuario preferido o tomar el primero
    const savedDni = localStorage.getItem("ticketflow_user_dni");
    const userToSelect = state.usuarios.find(u => u.dni.toString() === savedDni) || state.usuarios[0];

    if (userToSelect) {
      select.value = userToSelect.dni;
      seleccionarUsuario(userToSelect);
    }

    select.addEventListener("change", (e) => {
      const selected = state.usuarios.find(u => u.dni.toString() === e.target.value);
      if (selected) {
        seleccionarUsuario(selected);
      }
    });

  } catch (err) {
    console.error("Error al cargar usuarios:", err);
    document.getElementById("userSelect").innerHTML = "<option>Error al conectar con ApiGestion (:5001)</option>";
  }
}

function seleccionarUsuario(usuario) {
  state.usuarioActivo = usuario;
  localStorage.setItem("ticketflow_user_dni", usuario.dni);

  // Actualizar badge de rol en encabezado
  const badge = document.getElementById("userRoleBadge");
  badge.textContent = usuario.rol;
  badge.className = `role-badge role-${usuario.rol.toLowerCase()}`;

  // Ajustar visibilidad de pestañas y botones según el rol
  const organizadorElements = document.querySelectorAll(".role-organizador-only");
  const panelCompra = document.getElementById("panelCompraComprador");
  const panelOrganizadorAviso = document.getElementById("panelCompraOrganizadorAviso");

  if (usuario.rol === "Organizador") {
    organizadorElements.forEach(el => el.style.display = "inline-flex");
    if (panelCompra) panelCompra.style.display = "none";
    if (panelOrganizadorAviso) panelOrganizadorAviso.style.display = "block";
  } else {
    organizadorElements.forEach(el => el.style.display = "none");
    if (panelCompra) panelCompra.style.display = "block";
    if (panelOrganizadorAviso) panelOrganizadorAviso.style.display = "none";

    // Si el usuario estaba en una pestaña restringida (Puerta, Gestión o Reporte), redirigir al catálogo
    const activeTab = document.querySelector(".tab-btn.active")?.dataset.tab;
    if (activeTab === "organizadorView" || activeTab === "reporteView" || activeTab === "puertaView") {
      mostrarPestaña("catalogoView");
    }
  }

  // Aislamiento de seguridad: limpiar de pantalla las compras y entradas de la cuenta anterior
  limpiarVistaDetalleCompra();

  // Actualizar compras rápidas del nuevo usuario activo
  actualizarMisComprasRapidas();

  // Actualizar botones del catálogo para reflejar textos según el rol actual
  renderizarCatalogo();
}

function limpiarVistaDetalleCompra() {
  const detalleResultado = document.getElementById("detalleCompraResultado");
  if (detalleResultado) detalleResultado.style.display = "none";

  const inputBuscar = document.getElementById("inputBuscarCompraId");
  if (inputBuscar) inputBuscar.value = "";

  state.compraActual = null;
}

// =========================================================================
// 3. Catálogo de Eventos
// =========================================================================
async function cargarEventos() {
  const container = document.getElementById("eventsGrid");
  container.innerHTML = "<p class='loading-state'>Cargando eventos...</p>";

  try {
    const res = await fetch(`${API_GESTION}/eventos`);
    if (!res.ok) throw new Error("Error al obtener eventos.");
    state.eventos = await res.json();

    actualizarSelectsEventos();
    renderizarCatalogo();
  } catch (err) {
    console.error("Error al cargar eventos:", err);
    container.innerHTML = `
      <div style="grid-column: 1/-1; padding: 2rem; text-align: center; background: #fff1f2; border: 1px solid #fecdd3; border-radius: 8px;">
        <h3 style="color: #be123c;">No se pudo conectar con ApiGestion</h3>
        <p style="color: #4b5563; margin-top: 0.5rem;">Asegúrese de que ApiGestion esté ejecutándose en <strong>http://localhost:5001</strong>.</p>
        <button onclick="cargarEventos()" class="btn btn-outline" style="margin-top: 1rem;">Reintentar</button>
      </div>`;
  }
}

function renderizarCatalogo() {
  const container = document.getElementById("eventsGrid");
  container.innerHTML = "";

  if (state.eventos.length === 0) {
    container.innerHTML = "<p class='text-muted'>No hay eventos disponibles en este momento.</p>";
    return;
  }

  state.eventos.forEach(ev => {
    const fecha = new Date(ev.fecha);
    const fechaFormat = fecha.toLocaleDateString("es-AR", { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
    const esPasado = fecha < new Date();

    let statusBadge = `<span class="badge badge-success">Disponible</span>`;
    if (ev.cancelado) {
      statusBadge = `<span class="badge badge-danger">Cancelado</span>`;
    } else if (esPasado) {
      statusBadge = `<span class="badge badge-secondary">Finalizado</span>`;
    }

    const card = document.createElement("div");
    card.className = "event-card";

    // Vista previa de modalidades
    let modalidadesHtml = "";
    if (ev.modalidades && ev.modalidades.length > 0) {
      modalidadesHtml = `
        <div class="modalities-preview">
          <div class="modalities-preview-title">Modalidades disponibles</div>
          ${ev.modalidades.map(m => `
            <div class="modality-tag">
              <span>${m.nombre} (Cupo: ${m.cupoDisponible})</span>
              <strong>$${m.precio.toLocaleString('es-AR')}</strong>
            </div>
          `).join('')}
        </div>
      `;
    } else {
      modalidadesHtml = `<p style="font-size: 0.825rem; color: var(--slate-500); font-style: italic;">Sin modalidades cargadas aún.</p>`;
    }

    card.innerHTML = `
      <div class="event-card-header">
        <div class="event-card-top-row">
          <h3 class="event-card-title">${ev.nombre}</h3>
          ${statusBadge}
        </div>
        <div class="event-info-row">
          <span class="event-info-icon">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
              <line x1="16" y1="2" x2="16" y2="6"/>
              <line x1="8" y1="2" x2="8" y2="6"/>
              <line x1="3" y1="10" x2="21" y2="10"/>
            </svg>
          </span>
          <span>${fechaFormat}</span>
        </div>
      </div>
      <div class="event-card-body">
        <div class="event-info-row">
          <span class="event-info-icon">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/>
              <circle cx="12" cy="10" r="3"/>
            </svg>
          </span>
          <span>${ev.lugar}</span>
        </div>
        <p class="event-description">${ev.descripcion || "Sin descripción adicional."}</p>
        ${modalidadesHtml}
      </div>
      <div class="event-card-footer">
        <button class="btn btn-primary btn-block btn-ver-detalle" data-id="${ev.id}">
          ${state.usuarioActivo?.rol === "Comprador" ? "Comprar Entradas" : "Ver Detalle"}
        </button>
      </div>
    `;

    card.querySelector(".btn-ver-detalle").addEventListener("click", () => {
      abrirDetalleEvento(ev.id);
    });

    container.appendChild(card);
  });
}

// =========================================================================
// 4. Detalle de Evento, Mapa Interactivo y Compra
// =========================================================================
function abrirDetalleEvento(eventoId) {
  const evento = state.eventos.find(e => e.id === eventoId);
  if (!evento) return;

  state.eventoSeleccionado = evento;

  const fecha = new Date(evento.fecha);
  const fechaFormat = fecha.toLocaleDateString("es-AR", { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' });
  const esPasado = fecha < new Date();

  document.getElementById("detalleTitulo").textContent = evento.nombre;
  document.getElementById("detalleDescripcion").textContent = evento.descripcion || "Sin descripción.";
  document.getElementById("detalleFecha").textContent = fechaFormat;
  document.getElementById("detalleLugar").textContent = evento.lugar;

  const badge = document.getElementById("detalleStatusBadge");
  if (evento.cancelado) {
    badge.textContent = "Cancelado";
    badge.className = "badge badge-danger";
  } else if (esPasado) {
    badge.textContent = "Finalizado";
    badge.className = "badge badge-secondary";
  } else {
    badge.textContent = "Disponible";
    badge.className = "badge badge-success";
  }

  // Cargar selector de modalidades
  const selectMod = document.getElementById("selectModalidad");
  selectMod.innerHTML = "";

  if (evento.modalidades && evento.modalidades.length > 0) {
    evento.modalidades.forEach(m => {
      const opt = document.createElement("option");
      opt.value = m.id;
      opt.textContent = `${m.nombre} — $${m.precio.toLocaleString('es-AR')} (Disponibles: ${m.cupoDisponible})`;
      selectMod.appendChild(opt);
    });
    actualizarInfoModalidadSeleccionada();
  } else {
    selectMod.innerHTML = "<option value=''>No hay modalidades disponibles</option>";
    document.getElementById("modalidadInfoBox").style.display = "none";
  }

  // Inicializar o centrar Mapa Interactivo Leaflet con CARTO Voyager (evita 403 de OpenStreetMap)
  renderizarMapa(evento.latitud, evento.longitud, evento.nombre, evento.lugar);

  // Resetear cantidad y calcular precio
  document.getElementById("inputCantidad").value = 1;
  calcularPrecioCompra();

  mostrarPestaña("detalleView");
}

function renderizarMapa(lat, lng, titulo, lugar) {
  // Coordenadas por defecto (Obelisco, Buenos Aires) si no están provistas
  const defaultLat = -34.6037;
  const defaultLng = -58.3816;

  const finalLat = (lat !== null && lat !== undefined && !isNaN(lat)) ? lat : defaultLat;
  const finalLng = (lng !== null && lng !== undefined && !isNaN(lng)) ? lng : defaultLng;

  setTimeout(() => {
    const mapDiv = document.getElementById("mapContainer");
    if (!mapDiv) return;

    if (!state.leafletMap) {
      state.leafletMap = L.map('mapContainer').setView([finalLat, finalLng], 14);

      // Usar CARTO Voyager: infraestructura CDN confiable que no bloquea IPs compartidas (universidades)
      L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions" target="_blank" rel="noopener">CARTO</a>',
        subdomains: 'abcd',
        maxZoom: 19
      }).addTo(state.leafletMap);
    } else {
      state.leafletMap.invalidateSize();
      state.leafletMap.setView([finalLat, finalLng], 14);
    }

    if (state.mapMarker) {
      state.leafletMap.removeLayer(state.mapMarker);
    }

    state.mapMarker = L.marker([finalLat, finalLng]).addTo(state.leafletMap);
    state.mapMarker.bindPopup(`<strong>${titulo}</strong><br>${lugar}`).openPopup();
  }, 100);
}

function actualizarInfoModalidadSeleccionada() {
  const selectMod = document.getElementById("selectModalidad");
  const modId = selectMod.value;
  const modalidad = state.eventoSeleccionado?.modalidades?.find(m => m.id === modId);

  const box = document.getElementById("modalidadInfoBox");
  if (!modalidad) {
    box.style.display = "none";
    return;
  }

  box.style.display = "block";
  document.getElementById("modInfoNombre").textContent = modalidad.nombre;
  document.getElementById("modInfoBeneficios").textContent = modalidad.beneficios || "Sin beneficios especiales.";
  document.getElementById("modInfoCupo").textContent = modalidad.cupoDisponible;
  document.getElementById("modInfoPrecio").textContent = `$${modalidad.precio.toLocaleString('es-AR')}`;

  const inputCant = document.getElementById("inputCantidad");
  inputCant.max = Math.max(1, modalidad.cupoDisponible);

  calcularPrecioCompra();
}

function calcularPrecioCompra() {
  const selectMod = document.getElementById("selectModalidad");
  const modId = selectMod.value;
  const modalidad = state.eventoSeleccionado?.modalidades?.find(m => m.id === modId);
  const cantidad = parseInt(document.getElementById("inputCantidad").value, 10) || 1;

  if (!modalidad) return;

  const subtotal = modalidad.precio * cantidad;
  let total = subtotal;
  let tieneDescuento = false;
  let montoDescuento = 0;

  // Regla de negocio: si compra 5 entradas o más, 15% de descuento
  if (cantidad >= 5) {
    tieneDescuento = true;
    montoDescuento = Math.round(subtotal * 0.15);
    total = subtotal - montoDescuento;
  }

  document.getElementById("discountBanner").style.display = tieneDescuento ? "flex" : "none";
  document.getElementById("filaDescuento").style.display = tieneDescuento ? "flex" : "none";

  document.getElementById("resumenSubtotal").textContent = `$${subtotal.toLocaleString('es-AR')}`;
  document.getElementById("resumenDescuento").textContent = `-$${montoDescuento.toLocaleString('es-AR')}`;
  document.getElementById("resumenTotal").textContent = `$${total.toLocaleString('es-AR')}`;
}

async function procesarCompra() {
  if (!state.usuarioActivo || state.usuarioActivo.rol !== "Comprador") {
    alert("Debe seleccionar un usuario con rol Comprador para realizar la compra.");
    return;
  }

  const selectMod = document.getElementById("selectModalidad");
  const modId = selectMod.value;
  const cantidad = parseInt(document.getElementById("inputCantidad").value, 10);

  if (!modId) {
    alert("Por favor seleccione una modalidad de entrada.");
    return;
  }

  if (isNaN(cantidad) || cantidad <= 0) {
    alert("Ingrese una cantidad válida mayor a 0.");
    return;
  }

  const btnConfirmar = document.getElementById("btnConfirmarCompra");
  btnConfirmar.disabled = true;
  btnConfirmar.textContent = "Procesando compra...";

  try {
    const payload = {
      dniComprador: state.usuarioActivo.dni.toString(),
      idEvento: state.eventoSeleccionado.id,
      idModalidad: modId,
      cantidad: cantidad
    };

    const res = await fetch(`${API_GESTION}/compras`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Dni": state.usuarioActivo.dni.toString()
      },
      body: JSON.stringify(payload)
    });

    const data = await res.json();

    if (!res.ok) {
      throw new Error(data.error || "No se pudo completar la compra.");
    }

    // Éxito: abrir modal con las entradas generadas y códigos de 6 caracteres
    mostrarModalCompraExitosa(data);

    // Refrescar eventos para actualizar cupos
    await cargarEventos();
    abrirDetalleEvento(state.eventoSeleccionado.id);

  } catch (err) {
    alert(`Error en la compra: ${err.message}`);
  } finally {
    btnConfirmar.disabled = false;
    btnConfirmar.textContent = "Confirmar Compra";
  }
}

function mostrarModalCompraExitosa(compra) {
  state.compraActual = compra;

  document.getElementById("modalCompraId").textContent = compra.id;
  document.getElementById("modalCompraTotal").textContent = `$${compra.total.toLocaleString('es-AR')}`;

  const container = document.getElementById("modalCodigosEntradas");
  container.innerHTML = "";

  compra.entradas.forEach(e => {
    const tag = document.createElement("span");
    tag.className = "ticket-code-tag";
    tag.textContent = e.codigo;
    container.appendChild(tag);
  });

  document.getElementById("modalCompraExitosa").classList.add("active");
  actualizarMisComprasRapidas();
}

// =========================================================================
// 5. Control de Acceso en Puerta (Exclusivo Organizadores / Administradores)
// =========================================================================
async function validarEntradaEnPuerta() {
  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Acceso denegado: El control de acceso en puerta es exclusivo para usuarios con rol Organizador / Administrador.");
    mostrarPestaña("catalogoView");
    return;
  }

  const input = document.getElementById("doorTicketInput");
  const codigo = input.value.trim().toUpperCase();
  const selectEvento = document.getElementById("puertaEventoSelect");
  const idEvento = selectEvento.value || null;

  if (!codigo) {
    alert("Por favor ingrese el código de 6 caracteres de la entrada.");
    input.focus();
    return;
  }

  const btn = document.getElementById("btnValidarEntrada");
  btn.disabled = true;
  btn.textContent = "Validando código...";

  try {
    const payload = {
      codigo: codigo,
      idEvento: idEvento
    };

    const res = await fetch(`${API_VALIDACION}/validaciones`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    const resultado = await res.json();
    mostrarResultadoValidacion(resultado);
    registrarHistorialValidacion(resultado);

    input.value = "";
    input.focus();

  } catch (err) {
    mostrarResultadoValidacion({
      exitoso: false,
      mensaje: `Error al conectar con la API de Validación (puerto 5002): ${err.message}`
    });
  } finally {
    btn.disabled = false;
    btn.textContent = "Validar Ingreso";
  }
}

function mostrarResultadoValidacion(res) {
  const card = document.getElementById("validationResultCard");
  card.style.display = "block";

  if (res.exitoso) {
    card.className = "validation-result-card result-success";
    card.innerHTML = `
      <div class="result-badge-indicator">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <path d="M20 6 9 17l-5-5"/>
        </svg>
        Ingreso Autorizado
      </div>
      <div class="result-title">Entrada Válida</div>
      <div class="result-details-grid">
        <div class="result-detail-item">
          <strong>Código</strong>
          <span class="ticket-code-tag">${res.codigo}</span>
        </div>
        <div class="result-detail-item">
          <strong>Evento</strong>
          <span>${res.nombreEvento || "Evento"}</span>
        </div>
        <div class="result-detail-item">
          <strong>Modalidad</strong>
          <span>${res.nombreModalidad || "General"}</span>
        </div>
        <div class="result-detail-item">
          <strong>Estado</strong>
          <span>${res.mensaje}</span>
        </div>
      </div>
    `;
  } else {
    card.className = "validation-result-card result-danger";
    card.innerHTML = `
      <div class="result-badge-indicator">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="10"/>
          <line x1="15" y1="9" x2="9" y2="15"/>
          <line x1="9" y1="9" x2="15" y2="15"/>
        </svg>
        Acceso Denegado
      </div>
      <div class="result-title">Entrada Rechazada</div>
      <div class="result-details-grid">
        <div class="result-detail-item">
          <strong>Código presentado</strong>
          <span class="ticket-code-tag">${res.codigo || "-"}</span>
        </div>
        <div class="result-detail-item" style="grid-column: 1 / -1;">
          <strong>Motivo del rechazo</strong>
          <span style="font-weight: 600;">${res.mensaje}</span>
        </div>
      </div>
    `;
  }
}

function registrarHistorialValidacion(res) {
  const hora = new Date().toLocaleTimeString("es-AR");
  state.historialValidaciones.unshift({
    hora: hora,
    codigo: res.codigo || "-",
    evento: res.nombreEvento ? `${res.nombreEvento} (${res.nombreModalidad || ""})` : "-",
    exitoso: res.exitoso,
    mensaje: res.mensaje
  });

  if (state.historialValidaciones.length > 10) {
    state.historialValidaciones.pop();
  }

  const tbody = document.getElementById("historialValidacionesBody");
  tbody.innerHTML = state.historialValidaciones.map(item => `
    <tr>
      <td>${item.hora}</td>
      <td><span class="ticket-code-tag">${item.codigo}</span></td>
      <td>${item.evento}</td>
      <td>
        <span class="badge ${item.exitoso ? 'badge-success' : 'badge-danger'}">
          ${item.exitoso ? 'Autorizado' : 'Rechazado'}
        </span>
      </td>
    </tr>
  `).join('');
}

// =========================================================================
// 6. Consulta de Compra y Aislamiento de Entradas por Usuario
// =========================================================================
async function buscarCompra(compraId) {
  const id = compraId || document.getElementById("inputBuscarCompraId").value.trim();
  if (!id) {
    alert("Por favor ingrese o seleccione un identificador de compra.");
    return;
  }

  try {
    const res = await fetch(`${API_GESTION}/compras/${id}`);
    if (!res.ok) {
      if (res.status === 404) throw new Error("Compra no encontrada.");
      throw new Error("Error al consultar la compra.");
    }

    const compra = await res.json();

    // Aislamiento de datos: Si el usuario activo es Comprador, solo puede ver sus propias compras
    if (state.usuarioActivo?.rol === "Comprador" && compra.dniComprador.toString() !== state.usuarioActivo.dni.toString()) {
      limpiarVistaDetalleCompra();
      alert("Acceso denegado: Esta compra no pertenece al usuario activo.");
      return;
    }

    renderizarDetalleCompra(compra);

  } catch (err) {
    alert(`Error: ${err.message}`);
  }
}

function renderizarDetalleCompra(compra) {
  document.getElementById("detalleCompraResultado").style.display = "block";
  document.getElementById("inputBuscarCompraId").value = compra.id;

  document.getElementById("compraInfoId").textContent = compra.id;
  document.getElementById("compraInfoEvento").textContent = compra.nombreEvento;
  document.getElementById("compraInfoModalidad").textContent = `${compra.nombreModalidad} (${compra.cantidad} entradas)`;
  document.getElementById("compraInfoTotal").textContent = `$${compra.total.toLocaleString('es-AR')}`;

  const tbody = document.getElementById("entradasCompraTbody");
  tbody.innerHTML = "";

  const esDuenio = state.usuarioActivo?.dni?.toString() === compra.dniComprador;
  const esComprador = state.usuarioActivo?.rol === "Comprador";

  compra.entradas.forEach(entrada => {
    const tr = document.createElement("tr");

    let estadoBadge = `<span class="badge badge-success">Disponible</span>`;
    if (entrada.cancelada) {
      estadoBadge = `<span class="badge badge-danger">Cancelada</span>`;
    } else if (entrada.usada) {
      const fechaUso = entrada.fechaUso ? new Date(entrada.fechaUso).toLocaleString("es-AR") : "En puerta";
      estadoBadge = `<span class="badge badge-secondary">Usada (${fechaUso})</span>`;
    }

    let btnCancelarHtml = "-";
    // Solo puede cancelar si es Comprador, la entrada no fue usada, no está cancelada y es el comprador de la entrada
    if (esComprador && esDuenio && !entrada.usada && !entrada.cancelada) {
      btnCancelarHtml = `
        <button class="btn btn-danger btn-sm btn-cancelar-entrada" data-codigo="${entrada.codigo}">
          Cancelar Entrada
        </button>
      `;
    }

    tr.innerHTML = `
      <td><span class="ticket-code-tag">${entrada.codigo}</span></td>
      <td>${entrada.nombreModalidad}</td>
      <td>$${entrada.precioUnitario.toLocaleString('es-AR')}</td>
      <td>${estadoBadge}</td>
      <td>${btnCancelarHtml}</td>
    `;

    const btn = tr.querySelector(".btn-cancelar-entrada");
    if (btn) {
      btn.addEventListener("click", () => cancelarEntradaComprada(entrada.codigo, compra.id));
    }

    tbody.appendChild(tr);
  });
}

// Endpoint de Cancelación: DELETE /api/entradas/{codigo}
async function cancelarEntradaComprada(codigo, compraId) {
  if (!confirm(`¿Está seguro de cancelar la entrada con código ${codigo}?\nEl cupo se restablecerá y la entrada ya no podrá ser utilizada.`)) {
    return;
  }

  try {
    const res = await fetch(`${API_GESTION}/entradas/${codigo}`, {
      method: "DELETE",
      headers: {
        "X-Dni": state.usuarioActivo.dni.toString()
      }
    });

    const data = await res.json();
    if (!res.ok) {
      throw new Error(data.error || "No se pudo cancelar la entrada.");
    }

    alert(data.mensaje || "Entrada cancelada con éxito.");
    await buscarCompra(compraId);
    await cargarEventos();

  } catch (err) {
    alert(`Error al cancelar entrada: ${err.message}`);
  }
}

// Carga las compras reales y exclusivas del usuario activo consultando a ApiGestion
async function actualizarMisComprasRapidas() {
  const cont = document.getElementById("misComprasBotones");
  if (!cont) return;

  cont.innerHTML = "";

  if (!state.usuarioActivo) {
    cont.innerHTML = "<span class='text-muted' style='font-size: 0.85rem;'>Seleccione un usuario activo.</span>";
    return;
  }

  if (state.usuarioActivo.rol !== "Comprador") {
    cont.innerHTML = "<span class='text-muted' style='font-size: 0.85rem;'>El historial de compras es exclusivo para cuentas con rol Comprador.</span>";
    return;
  }

  cont.innerHTML = "<span class='text-muted' style='font-size: 0.85rem;'>Cargando compras del usuario...</span>";

  try {
    const res = await fetch(`${API_GESTION}/compras?dni=${state.usuarioActivo.dni}`);
    if (!res.ok) throw new Error("Error al obtener compras.");
    const comprasUsuario = await res.json();

    cont.innerHTML = "";

    if (!comprasUsuario || comprasUsuario.length === 0) {
      cont.innerHTML = "<span class='text-muted' style='font-size: 0.85rem;'>No tienes compras registradas con este usuario todavía.</span>";
      return;
    }

    comprasUsuario.slice(0, 8).forEach(compra => {
      const btn = document.createElement("button");
      btn.className = "btn btn-outline btn-sm font-mono";
      btn.textContent = `${compra.id.substring(0, 8)}... (${compra.nombreEvento})`;
      btn.title = `ID: ${compra.id} — ${compra.nombreEvento} (${compra.cantidad} entradas)`;
      btn.addEventListener("click", () => buscarCompra(compra.id));
      cont.appendChild(btn);
    });

  } catch (err) {
    console.error("Error al consultar compras del usuario:", err);
    cont.innerHTML = "<span class='text-muted' style='font-size: 0.85rem;'>No se pudieron cargar las compras del usuario activo.</span>";
  }
}

// =========================================================================
// 7. Panel Organizador (Crear Eventos, Agregar Modalidades, Cancelar)
// =========================================================================
async function crearNuevoEvento(e) {
  e.preventDefault();

  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Acción restringida: debe operar como Organizador.");
    return;
  }

  const nombre = document.getElementById("nuevoEventoNombre").value.trim();
  const descripcion = document.getElementById("nuevoEventoDescripcion").value.trim();
  const fecha = document.getElementById("nuevoEventoFecha").value;
  const lugar = document.getElementById("nuevoEventoLugar").value.trim();
  const latitud = parseFloat(document.getElementById("nuevoEventoLatitud").value) || null;
  const longitud = parseFloat(document.getElementById("nuevoEventoLongitud").value) || null;

  try {
    const payload = {
      nombre,
      descripcion,
      fecha: new Date(fecha).toISOString(),
      lugar,
      latitud,
      longitud
    };

    const res = await fetch(`${API_GESTION}/eventos`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Dni": state.usuarioActivo.dni.toString()
      },
      body: JSON.stringify(payload)
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.error || "No se pudo crear el evento.");

    alert(`Evento '${data.nombre}' creado exitosamente.`);
    document.getElementById("formCrearEvento").reset();
    await cargarEventos();

  } catch (err) {
    alert(`Error al crear evento: ${err.message}`);
  }
}

async function agregarModalidadAEvento(e) {
  e.preventDefault();

  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Acción restringida: debe operar como Organizador.");
    return;
  }

  const idEvento = document.getElementById("modalidadEventoSelect").value;
  const nombre = document.getElementById("nuevaModalidadNombre").value.trim();
  const precio = parseFloat(document.getElementById("nuevaModalidadPrecio").value);
  const cupoMaximo = parseInt(document.getElementById("nuevaModalidadCupo").value, 10);
  const beneficios = document.getElementById("nuevaModalidadBeneficios").value.trim();

  try {
    const payload = {
      nombre,
      precio,
      beneficios,
      cupoMaximo
    };

    const res = await fetch(`${API_GESTION}/eventos/${idEvento}/modalidades`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Dni": state.usuarioActivo.dni.toString()
      },
      body: JSON.stringify(payload)
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.error || "No se pudo agregar la modalidad.");

    alert(`Modalidad '${data.nombre}' agregada con éxito.`);
    document.getElementById("formAgregarModalidad").reset();
    await cargarEventos();

  } catch (err) {
    alert(`Error al agregar modalidad: ${err.message}`);
  }
}

async function cancelarEvento(idEvento, nombreEvento) {
  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Acción restringida: debe operar como Organizador.");
    return;
  }

  if (!confirm(`¿Está seguro de cancelar el evento '${nombreEvento}'?\nYa no se podrán vender más entradas.`)) {
    return;
  }

  try {
    const res = await fetch(`${API_GESTION}/eventos/${idEvento}/cancelar`, {
      method: "PUT",
      headers: {
        "X-Dni": state.usuarioActivo.dni.toString()
      }
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.error || "No se pudo cancelar el evento.");

    alert(data.mensaje || "Evento cancelado exitosamente.");
    await cargarEventos();
    if (document.getElementById("reporteView").classList.contains("active")) {
      await cargarReporteRecaudacion();
    }

  } catch (err) {
    alert(`Error al cancelar evento: ${err.message}`);
  }
}

// =========================================================================
// 8. Reporte de Recaudación (Organizador)
// =========================================================================
async function cargarReporteRecaudacion() {
  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("El reporte de recaudación es exclusivo para usuarios con rol Organizador.");
    return;
  }

  try {
    const res = await fetch(`${API_GESTION}/reportes/recaudacion`, {
      headers: {
        "X-Dni": state.usuarioActivo.dni.toString()
      }
    });

    if (!res.ok) {
      const data = await res.json();
      throw new Error(data.error || "Error al obtener reporte.");
    }

    const reporte = await res.json();

    document.getElementById("kpiRecaudacionTotal").textContent = `$${reporte.totalGeneralRecaudado.toLocaleString('es-AR')}`;
    document.getElementById("kpiEntradasVendidas").textContent = reporte.totalGeneralEntradasVendidas.toLocaleString('es-AR');

    const tbody = document.getElementById("reporteTbody");
    tbody.innerHTML = "";

    if (reporte.eventos.length === 0) {
      tbody.innerHTML = "<tr><td colspan='7' style='text-align: center;'>No hay eventos registrados.</td></tr>";
      return;
    }

    reporte.eventos.forEach(ev => {
      const tr = document.createElement("tr");

      let estadoHtml = ev.cancelado 
        ? `<span class="badge badge-danger">Cancelado</span>` 
        : `<span class="badge badge-success">Activo</span>`;

      let modHtml = ev.modalidades.map(m => `
        <div style="font-size: 0.825rem; margin-bottom: 0.25rem;">
          <strong>${m.nombreModalidad}:</strong> ${m.entradasVendidas}/${m.cupoMaximo} — $${m.recaudacion.toLocaleString('es-AR')}
        </div>
      `).join('');

      let btnCancelarHtml = !ev.cancelado 
        ? `<button class="btn btn-danger btn-sm btn-cancelar-ev" data-id="${ev.idEvento}" data-nombre="${ev.nombreEvento}">Cancelar</button>`
        : `<span style="color: var(--slate-400); font-size: 0.8rem;">Cancelado</span>`;

      tr.innerHTML = `
        <td><strong>${ev.nombreEvento}</strong><br><small style="color:var(--slate-500);">${ev.lugar}</small></td>
        <td>${new Date(ev.fecha).toLocaleDateString("es-AR")}</td>
        <td>${estadoHtml}</td>
        <td><strong>${ev.entradasVendidas}</strong></td>
        <td><strong style="color: var(--primary);">$${ev.recaudacionTotal.toLocaleString('es-AR')}</strong></td>
        <td>${modHtml || "-"}</td>
        <td>${btnCancelarHtml}</td>
      `;

      const btnCanc = tr.querySelector(".btn-cancelar-ev");
      if (btnCanc) {
        btnCanc.addEventListener("click", () => cancelarEvento(ev.idEvento, ev.nombreEvento));
      }

      tbody.appendChild(tr);
    });

  } catch (err) {
    console.error("Error al cargar reporte:", err);
  }
}

function actualizarSelectsEventos() {
  const selectPuerta = document.getElementById("puertaEventoSelect");
  const selectModalidadEv = document.getElementById("modalidadEventoSelect");

  if (selectPuerta) {
    selectPuerta.innerHTML = `<option value="">-- Todos los eventos / Sin filtro estricto --</option>`;
    state.eventos.forEach(e => {
      const opt = document.createElement("option");
      opt.value = e.id;
      opt.textContent = `${e.nombre} (${new Date(e.fecha).toLocaleDateString('es-AR')})`;
      selectPuerta.appendChild(opt);
    });
  }

  if (selectModalidadEv) {
    selectModalidadEv.innerHTML = "";
    state.eventos.filter(e => !e.cancelado).forEach(e => {
      const opt = document.createElement("option");
      opt.value = e.id;
      opt.textContent = e.nombre;
      selectModalidadEv.appendChild(opt);
    });
  }
}

// =========================================================================
// 9. Configuración de Listeners
// =========================================================================
function setupEventListeners() {
  // Catálogo
  document.getElementById("btnRefrescarEventos")?.addEventListener("click", cargarEventos);
  document.getElementById("btnVolverCatalogo")?.addEventListener("click", () => mostrarPestaña("catalogoView"));

  // Compra
  document.getElementById("selectModalidad")?.addEventListener("change", actualizarInfoModalidadSeleccionada);
  document.getElementById("inputCantidad")?.addEventListener("input", calcularPrecioCompra);
  document.getElementById("btnConfirmarCompra")?.addEventListener("click", procesarCompra);

  // Puerta de Acceso
  document.getElementById("btnValidarEntrada")?.addEventListener("click", validarEntradaEnPuerta);
  document.getElementById("doorTicketInput")?.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
      e.preventDefault();
      validarEntradaEnPuerta();
    }
  });

  // Búsqueda de Compra
  document.getElementById("btnBuscarCompra")?.addEventListener("click", () => buscarCompra());

  // Organizador
  document.getElementById("formCrearEvento")?.addEventListener("submit", crearNuevoEvento);
  document.getElementById("formAgregarModalidad")?.addEventListener("submit", agregarModalidadAEvento);
  document.getElementById("btnRefrescarReporte")?.addEventListener("click", cargarReporteRecaudacion);

  // Modal Compra Exitosa
  const cerrarModal = () => document.getElementById("modalCompraExitosa").classList.remove("active");
  document.getElementById("btnCerrarModalCompra")?.addEventListener("click", cerrarModal);
  document.getElementById("btnCerrarModal")?.addEventListener("click", cerrarModal);
  document.getElementById("btnIrAConsultaCompra")?.addEventListener("click", () => {
    cerrarModal();
    if (state.compraActual) {
      mostrarPestaña("consultaCompraView");
      buscarCompra(state.compraActual.id);
    }
  });
}
