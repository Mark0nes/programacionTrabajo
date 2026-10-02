// Configuracion de endpoints de las dos APIs
const API_GESTION = "http://localhost:5001/api";
const API_VALIDACION = "http://localhost:5002/api";

// Estado global de la aplicacion
const state = {
  usuarios: [],
  usuarioActivo: null,
  eventos: [],
  eventoSeleccionado: null,
  compraActual: null,
  historialValidaciones: [],
  leafletMap: null,
  mapMarker: null,
  mapaCrearEvento: null,
  markerCrearEvento: null
};

// Inicializacion al cargar la pagina
document.addEventListener("DOMContentLoaded", async () => {
  await cargarUsuarios();
  setupComun();

  // Inicializar segun la pagina actual
  if (document.getElementById("eventsGrid")) {
    cargarEventos();
  }
  if (document.getElementById("panelCompraComprador")) {
    cargarDetalleDesdeUrl();
  }
  if (document.getElementById("btnBuscarCompra")) {
    inicializarConsultaCompra();
  }
  if (document.getElementById("doorTicketInput")) {
    inicializarPuerta();
  }
  if (document.getElementById("formCrearEvento")) {
    inicializarGestionEventos();
  }
  if (document.getElementById("reporteTableBody")) {
    cargarReporteRecaudacion();
  }
});

// Carga de usuarios y manejo del usuario activo
async function cargarUsuarios() {
  try {
    const res = await fetch(`${API_GESTION}/usuarios`);
    if (!res.ok) throw new Error("No se pudo cargar la lista de usuarios.");
    state.usuarios = await res.json();

    const select = document.getElementById("userSelect");
    if (!select) return;

    select.innerHTML = "";
    state.usuarios.forEach(u => {
      const opt = document.createElement("option");
      opt.value = u.dni;
      opt.textContent = `${u.nombre} (${u.rol}) - DNI: ${u.dni}`;
      select.appendChild(opt);
    });

    // Recuperar usuario guardado en localStorage o usar el primero
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
    const select = document.getElementById("userSelect");
    if (select) {
      select.innerHTML = "<option>Error al conectar con ApiGestion (:5001)</option>";
    }
  }
}

function seleccionarUsuario(usuario) {
  state.usuarioActivo = usuario;
  localStorage.setItem("ticketflow_user_dni", usuario.dni);

  // Actualizar badge de rol en el encabezado
  const badge = document.getElementById("userRoleBadge");
  if (badge) {
    badge.textContent = usuario.rol;
    badge.className = `role-badge role-${usuario.rol.toLowerCase()}`;
  }

  // Controlar visibilidad de enlaces segun el rol
  const organizadorElements = document.querySelectorAll(".role-organizador-only");
  const compradorElements = document.querySelectorAll(".role-comprador-only");

  if (usuario.rol === "Organizador") {
    organizadorElements.forEach(el => el.style.display = "");
    compradorElements.forEach(el => el.style.display = "none");
  } else {
    organizadorElements.forEach(el => el.style.display = "none");
    compradorElements.forEach(el => el.style.display = "");

    // Si el usuario es Comprador y esta en una pagina exclusiva de Organizador, redirigir al catalogo
    const path = window.location.pathname.toLowerCase();
    if (path.includes("control-acceso") || path.includes("gestion-eventos") || path.includes("reporte-recaudacion")) {
      alert("Acceso denegado: Esta seccion es exclusiva para organizadores.");
      window.location.href = "index.html";
      return;
    }
  }

  // Refrescar vistas segun la pagina actual
  if (document.getElementById("eventsGrid")) {
    renderizarCatalogo();
  }
  if (document.getElementById("misComprasBotones")) {
    actualizarMisComprasRapidas();
  }
  if (document.getElementById("selectModalidad")) {
    actualizarInfoModalidadSeleccionada();
  }
}

function setupComun() {
  document.getElementById("btnRefrescarEventos")?.addEventListener("click", cargarEventos);
}

// -------------------------------------------------------------
// Catalogo de Eventos (index.html / eventos.html)
// -------------------------------------------------------------
async function cargarEventos() {
  const container = document.getElementById("eventsGrid");
  if (!container) return;

  container.innerHTML = "<p class='loading-state'>Cargando eventos...</p>";

  try {
    const res = await fetch(`${API_GESTION}/eventos`);
    if (!res.ok) throw new Error("Error al obtener eventos.");
    state.eventos = await res.json();
    renderizarCatalogo();
  } catch (err) {
    console.error("Error al cargar eventos:", err);
    container.innerHTML = `
      <div style="grid-column: 1/-1; padding: 2rem; text-align: center; background: #fff1f2; border: 1px solid #fecdd3; border-radius: 8px;">
        <h3 style="color: #be123c;">No se pudo conectar con ApiGestion</h3>
        <p style="color: #4b5563; margin-top: 0.5rem;">Asegurese de que ApiGestion este ejecutandose en http://localhost:5001.</p>
        <button onclick="cargarEventos()" class="btn btn-outline" style="margin-top: 1rem;">Reintentar</button>
      </div>`;
  }
}

function renderizarCatalogo() {
  const container = document.getElementById("eventsGrid");
  if (!container) return;

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
      modalidadesHtml = `<p style="font-size: 0.825rem; color: var(--slate-500); font-style: italic;">Sin modalidades cargadas aun.</p>`;
    }

    const card = document.createElement("div");
    card.className = "event-card";

    let textoBoton = "Ver Detalle";
    if (state.usuarioActivo?.rol === "Comprador") {
      if (ev.cancelado) textoBoton = "Evento Cancelado";
      else if (esPasado) textoBoton = "Evento Finalizado";
      else textoBoton = "Comprar Entradas";
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
        <p class="event-description">${ev.descripcion || "Sin descripcion adicional."}</p>
        ${modalidadesHtml}
      </div>
      <div class="event-card-footer" style="display: flex; gap: 0.5rem;">
        <a href="comprar-entrada.html?id=${ev.id}" class="btn ${ev.cancelado || esPasado ? 'btn-outline' : 'btn-primary'} btn-block">
          ${textoBoton}
        </a>
        ${state.usuarioActivo?.rol === "Organizador" && !ev.cancelado ? `
          <button type="button" class="btn btn-danger btn-sm btn-cancelar-card" data-id="${ev.id}" data-nombre="${ev.nombre}" title="Cancelar Evento">
            Cancelar
          </button>
        ` : ''}
      </div>
    `;

    const btnCanc = card.querySelector(".btn-cancelar-card");
    if (btnCanc) {
      btnCanc.addEventListener("click", () => cancelarEvento(ev.id, ev.nombre));
    }

    container.appendChild(card);
  });
}

// -------------------------------------------------------------
// Detalle de Evento y Compra (comprar-entrada.html)
// -------------------------------------------------------------
async function cargarDetalleDesdeUrl() {
  const params = new URLSearchParams(window.location.search);
  const eventoId = params.get("id");

  if (!eventoId) {
    alert("No se especifico el ID del evento.");
    window.location.href = "index.html";
    return;
  }

  try {
    const res = await fetch(`${API_GESTION}/eventos/${eventoId}`);
    if (!res.ok) throw new Error("Evento no encontrado.");
    const evento = await res.json();
    state.eventoSeleccionado = evento;
    renderDetalleEvento(evento);
  } catch (err) {
    alert(`Error: ${err.message}`);
    window.location.href = "index.html";
  }
}

function renderDetalleEvento(evento) {
  const fecha = new Date(evento.fecha);
  const fechaFormat = fecha.toLocaleDateString("es-AR", { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' });
  const esPasado = fecha < new Date();

  document.getElementById("detalleTitulo").textContent = evento.nombre;
  document.getElementById("detalleDescripcion").textContent = evento.descripcion || "Sin descripcion.";
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

  // Boton para cancelar evento completo (Organizador)
  const btnCancEv = document.getElementById("btnCancelarEventoDetalle");
  if (btnCancEv) {
    if (state.usuarioActivo?.rol === "Organizador" && !evento.cancelado) {
      btnCancEv.style.display = "inline-flex";
      btnCancEv.onclick = () => cancelarEvento(evento.id, evento.nombre);
    } else {
      btnCancEv.style.display = "none";
    }
  }

  // Aviso visual si el evento no admite compras
  const aviso = document.getElementById("avisoEventoNoDisponible");
  if (aviso) {
    if (evento.cancelado) {
      aviso.style.display = "block";
      aviso.style.background = "#fee2e2";
      aviso.style.color = "#991b1b";
      aviso.style.border = "1px solid #f87171";
      aviso.innerHTML = "<strong>Evento cancelado:</strong> La venta de entradas se encuentra cerrada.";
    } else if (esPasado) {
      aviso.style.display = "block";
      aviso.style.background = "#f1f5f9";
      aviso.style.color = "#475569";
      aviso.style.border = "1px solid #cbd5e1";
      aviso.innerHTML = "<strong>Evento finalizado:</strong> La fecha de este evento ya paso. No se pueden comprar entradas.";
    } else {
      aviso.style.display = "none";
    }
  }

  // Cargar selector de modalidades
  const selectMod = document.getElementById("selectModalidad");
  selectMod.innerHTML = "";

  if (evento.modalidades && evento.modalidades.length > 0) {
    evento.modalidades.forEach(m => {
      const opt = document.createElement("option");
      opt.value = m.id;
      const estadoTxt = m.cancelada ? ' [CANCELADA]' : ` (Disponibles: ${m.cupoDisponible})`;
      opt.textContent = `${m.nombre} - $${m.precio.toLocaleString('es-AR')}${estadoTxt}`;
      if ((m.cancelada || esPasado || evento.cancelado) && state.usuarioActivo?.rol === "Comprador") {
        opt.disabled = true;
      }
      selectMod.appendChild(opt);
    });
    actualizarInfoModalidadSeleccionada();
  } else {
    selectMod.innerHTML = "<option value=''>No hay modalidades disponibles</option>";
    document.getElementById("modalidadInfoBox").style.display = "none";
  }

  selectMod.disabled = (evento.cancelado || esPasado) && state.usuarioActivo?.rol === "Comprador";
  const inputCant = document.getElementById("inputCantidad");
  if (inputCant) {
    inputCant.disabled = (evento.cancelado || esPasado) && state.usuarioActivo?.rol === "Comprador";
  }

  // Enlace a Google Maps externo
  const btnGmaps = document.getElementById("btnAbrirGoogleMapsExt");
  if (btnGmaps) {
    const lat = evento.latitud || -34.6037;
    const lng = evento.longitud || -58.3816;
    btnGmaps.href = `https://www.google.com/maps/search/?api=1&query=${lat},${lng}`;
  }

  // Renderizar mapa interactivo
  renderizarMapa(evento.latitud, evento.longitud, evento.nombre, evento.lugar);

  // Listeners del panel de compra
  selectMod.onchange = actualizarInfoModalidadSeleccionada;
  if (inputCant) inputCant.oninput = calcularPrecioCompra;

  const btnConfirmar = document.getElementById("btnConfirmarCompra");
  if (btnConfirmar) btnConfirmar.onclick = procesarCompra;

  calcularPrecioCompra();
}

function renderizarMapa(lat, lng, titulo, lugar) {
  const defaultLat = -34.6037;
  const defaultLng = -58.3816;
  const finalLat = (lat !== null && lat !== undefined && !isNaN(lat)) ? lat : defaultLat;
  const finalLng = (lng !== null && lng !== undefined && !isNaN(lng)) ? lng : defaultLng;

  setTimeout(() => {
    const mapDiv = document.getElementById("mapContainer");
    if (!mapDiv) return;

    if (!state.leafletMap) {
      state.leafletMap = L.map('mapContainer', { attributionControl: false }).setView([finalLat, finalLng], 15);

      const googleCalles = L.tileLayer('https://{s}.google.com/vt/lyrs=m&x={x}&y={y}&z={z}', {
        maxZoom: 20,
        subdomains: ['mt0', 'mt1', 'mt2', 'mt3']
      });
      const googleSatelite = L.tileLayer('https://{s}.google.com/vt/lyrs=y&x={x}&y={y}&z={z}', {
        maxZoom: 20,
        subdomains: ['mt0', 'mt1', 'mt2', 'mt3']
      });

      googleCalles.addTo(state.leafletMap);
      L.control.layers({ "Google Maps (Calles)": googleCalles, "Google Maps (Satelite)": googleSatelite }, null, { position: 'topright' }).addTo(state.leafletMap);
    } else {
      state.leafletMap.invalidateSize();
      state.leafletMap.setView([finalLat, finalLng], 15);
    }

    if (state.mapMarker) {
      state.leafletMap.removeLayer(state.mapMarker);
    }

    const gmapsLink = `https://www.google.com/maps/search/?api=1&query=${finalLat},${finalLng}`;
    state.mapMarker = L.marker([finalLat, finalLng]).addTo(state.leafletMap);
    state.mapMarker.bindPopup(`
      <div style="min-width: 180px; padding: 2px;">
        <strong>${titulo}</strong><br>
        <span style="color: #64748b; font-size: 0.825rem; display: block; margin: 0.25rem 0;">${lugar}</span>
        <a href="${gmapsLink}" target="_blank" rel="noopener noreferrer" style="color: #2563eb; font-weight: 600; font-size: 0.8rem; text-decoration: underline;">
          Abrir en Google Maps
        </a>
      </div>
    `).openPopup();
  }, 100);
}

function actualizarInfoModalidadSeleccionada() {
  const selectMod = document.getElementById("selectModalidad");
  if (!selectMod) return;

  const modId = selectMod.value;
  const modalidad = state.eventoSeleccionado?.modalidades?.find(m => m.id === modId);
  const box = document.getElementById("modalidadInfoBox");

  if (!modalidad) {
    if (box) box.style.display = "none";
    return;
  }

  if (box) box.style.display = "block";
  document.getElementById("modInfoNombre").textContent = modalidad.nombre;
  document.getElementById("modInfoBeneficios").textContent = modalidad.beneficios || "Sin beneficios especiales.";
  document.getElementById("modInfoCupo").textContent = modalidad.cupoDisponible;
  document.getElementById("modInfoPrecio").textContent = `$${modalidad.precio.toLocaleString('es-AR')}`;

  const badgeEstado = document.getElementById("modInfoBadgeEstado");
  if (badgeEstado) {
    badgeEstado.textContent = modalidad.cancelada ? "Cancelada" : "Activa";
    badgeEstado.className = modalidad.cancelada ? "badge badge-danger" : "badge badge-success";
  }

  const boxCancMod = document.getElementById("boxCancelarModalidadDetalle");
  const btnCancMod = document.getElementById("btnCancelarModalidadDetalle");
  if (boxCancMod && btnCancMod) {
    if (state.usuarioActivo?.rol === "Organizador" && !modalidad.cancelada && !state.eventoSeleccionado?.cancelado) {
      boxCancMod.style.display = "block";
      btnCancMod.onclick = () => cancelarModalidad(state.eventoSeleccionado.id, modalidad.id, modalidad.nombre);
    } else {
      boxCancMod.style.display = "none";
    }
  }

  const esPasado = state.eventoSeleccionado && new Date(state.eventoSeleccionado.fecha) < new Date();
  const btnConfirmar = document.getElementById("btnConfirmarCompra");
  if (btnConfirmar) {
    if (state.eventoSeleccionado?.cancelado) {
      btnConfirmar.disabled = true;
      btnConfirmar.textContent = "Evento Cancelado";
    } else if (esPasado) {
      btnConfirmar.disabled = true;
      btnConfirmar.textContent = "Evento Finalizado";
    } else if (modalidad.cancelada) {
      btnConfirmar.disabled = true;
      btnConfirmar.textContent = "Modalidad Cancelada";
    } else if (modalidad.cupoDisponible <= 0) {
      btnConfirmar.disabled = true;
      btnConfirmar.textContent = "Cupo Agotado";
    } else {
      btnConfirmar.disabled = false;
      btnConfirmar.textContent = "Comprar Entradas";
    }
  }

  const inputCant = document.getElementById("inputCantidad");
  if (inputCant) {
    inputCant.max = Math.max(1, modalidad.cupoDisponible);
  }

  calcularPrecioCompra();
}

function calcularPrecioCompra() {
  const selectMod = document.getElementById("selectModalidad");
  if (!selectMod) return;

  const modId = selectMod.value;
  const modalidad = state.eventoSeleccionado?.modalidades?.find(m => m.id === modId);
  const inputCant = document.getElementById("inputCantidad");
  const cantidad = parseInt(inputCant?.value, 10) || 1;

  if (!modalidad) return;

  const subtotal = modalidad.precio * cantidad;
  let total = subtotal;
  let tieneDescuento = false;
  let montoDescuento = 0;

  // Descuento del 15% para 5 o mas entradas
  if (cantidad >= 5) {
    tieneDescuento = true;
    montoDescuento = Math.round(subtotal * 0.15);
    total = subtotal - montoDescuento;
  }

  const bannerDesc = document.getElementById("discountBanner");
  const filaDesc = document.getElementById("filaDescuento");
  if (bannerDesc) bannerDesc.style.display = tieneDescuento ? "flex" : "none";
  if (filaDesc) filaDesc.style.display = tieneDescuento ? "flex" : "none";

  document.getElementById("resumenSubtotal").textContent = `$${subtotal.toLocaleString('es-AR')}`;
  document.getElementById("resumenDescuento").textContent = `-$${montoDescuento.toLocaleString('es-AR')}`;
  document.getElementById("resumenTotal").textContent = `$${total.toLocaleString('es-AR')}`;
}

async function procesarCompra() {
  if (!state.usuarioActivo || state.usuarioActivo.rol !== "Comprador") {
    alert("Debe seleccionar un usuario con rol Comprador para realizar la compra.");
    return;
  }

  if (!state.eventoSeleccionado) return;

  if (state.eventoSeleccionado.cancelado) {
    alert("No se pueden comprar entradas para un evento cancelado.");
    return;
  }

  if (new Date(state.eventoSeleccionado.fecha) < new Date()) {
    alert("No se pueden comprar entradas para un evento que ya ha finalizado.");
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
    alert("Ingrese una cantidad valida mayor a 0.");
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

    mostrarModalCompraExitosa(data);
    cargarDetalleDesdeUrl();

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

  const modal = document.getElementById("modalCompraExitosa");
  modal.classList.add("active");

  const cerrarModal = () => modal.classList.remove("active");
  document.getElementById("btnCerrarModalCompra").onclick = cerrarModal;
  document.getElementById("btnCerrarModal").onclick = cerrarModal;

  const btnVerDetalle = document.getElementById("btnIrAConsultaCompra");
  if (btnVerDetalle) {
    btnVerDetalle.onclick = () => {
      window.location.href = `consultar-compra.html?id=${compra.id}`;
    };
  }
}

// -------------------------------------------------------------
// Consulta de Compras (consultar-compra.html)
// -------------------------------------------------------------
function inicializarConsultaCompra() {
  actualizarMisComprasRapidas();

  document.getElementById("btnBuscarCompra")?.addEventListener("click", () => buscarCompra());
  document.getElementById("inputBuscarCompraId")?.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
      e.preventDefault();
      buscarCompra();
    }
  });

  // Si vino con parametro id en la URL, consultar automaticamente
  const params = new URLSearchParams(window.location.search);
  const compraId = params.get("id");
  if (compraId) {
    document.getElementById("inputBuscarCompraId").value = compraId;
    buscarCompra(compraId);
  }
}

async function buscarCompra(compraIdManual = null) {
  const input = document.getElementById("inputBuscarCompraId");
  const id = compraIdManual || input.value.trim();

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

    // Si el usuario activo es Comprador, validar que sea su compra
    if (state.usuarioActivo?.rol === "Comprador" && compra.dniComprador.toString() !== state.usuarioActivo.dni.toString()) {
      document.getElementById("detalleCompraResultado").style.display = "none";
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

async function cancelarEntradaComprada(codigo, compraId) {
  if (!confirm(`Desea cancelar la entrada con codigo ${codigo}?\nEl cupo se restablecera y la entrada ya no podra ser utilizada.`)) {
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

    alert(data.mensaje || "Entrada cancelada con exito.");
    buscarCompra(compraId);

  } catch (err) {
    alert(`Error al cancelar entrada: ${err.message}`);
  }
}

async function actualizarMisComprasRapidas() {
  const cont = document.getElementById("misComprasBotones");
  if (!cont) return;

  cont.innerHTML = "";

  if (!state.usuarioActivo || state.usuarioActivo.rol !== "Comprador") {
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
      cont.innerHTML = "<span class='text-muted' style='font-size: 0.85rem;'>No tienes compras registradas con este usuario todavia.</span>";
      return;
    }

    comprasUsuario.slice(0, 8).forEach(compra => {
      const btn = document.createElement("button");
      btn.className = "btn btn-outline btn-sm font-mono";
      btn.textContent = `${compra.id.substring(0, 8)}... (${compra.nombreEvento})`;
      btn.title = `ID: ${compra.id} - ${compra.nombreEvento} (${compra.cantidad} entradas)`;
      btn.addEventListener("click", () => {
        buscarCompra(compra.id);
      });
      cont.appendChild(btn);
    });

  } catch (err) {
    console.error("Error al consultar compras del usuario:", err);
    cont.innerHTML = "<span class='text-muted' style='font-size: 0.85rem;'>No se pudieron cargar las compras del usuario activo.</span>";
  }
}

// -------------------------------------------------------------
// Control de Acceso en Puerta (control-acceso.html)
// -------------------------------------------------------------
async function inicializarPuerta() {
  try {
    const res = await fetch(`${API_GESTION}/eventos`);
    if (res.ok) {
      state.eventos = await res.json();
      const sel = document.getElementById("puertaEventoSelect");
      if (sel) {
        sel.innerHTML = `<option value="">Cualquier evento (autodetectar)</option>`;
        state.eventos.forEach(ev => {
          const opt = document.createElement("option");
          opt.value = ev.id;
          opt.textContent = `${ev.nombre} (${ev.cancelado ? 'Cancelado' : 'Disponible'})`;
          sel.appendChild(opt);
        });
      }
    }
  } catch (err) {
    console.error("Error al cargar eventos para puerta:", err);
  }

  document.getElementById("btnValidarEntrada")?.addEventListener("click", validarEntradaEnPuerta);
  document.getElementById("doorTicketInput")?.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
      e.preventDefault();
      validarEntradaEnPuerta();
    }
  });
}

async function validarEntradaEnPuerta() {
  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Acceso denegado: El control de acceso en puerta es exclusivo para usuarios con rol Organizador.");
    window.location.href = "index.html";
    return;
  }

  const input = document.getElementById("doorTicketInput");
  const codigo = input.value.trim().toUpperCase();
  const selectEvento = document.getElementById("puertaEventoSelect");
  const idEvento = selectEvento.value || null;

  if (!codigo) {
    alert("Por favor ingrese el codigo de 6 caracteres de la entrada.");
    input.focus();
    return;
  }

  const btn = document.getElementById("btnValidarEntrada");
  btn.disabled = true;
  btn.textContent = "Validando codigo...";

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
      mensaje: `Error al conectar con la API de Validacion (:5002): ${err.message}`
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
    card.className = "validation-result-card success";
    card.innerHTML = `
      <div class="result-badge-indicator">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="20 6 9 17 4 12"/>
        </svg>
        Acceso Autorizado
      </div>
      <div class="result-title">Ingreso Permitido</div>
      <div class="result-details-grid">
        <div class="result-detail-item">
          <strong>Codigo de Entrada</strong>
          <span class="ticket-code-tag">${res.codigo || "-"}</span>
        </div>
        <div class="result-detail-item">
          <strong>Evento</strong>
          <span>${res.nombreEvento || "-"}</span>
        </div>
        <div class="result-detail-item">
          <strong>Modalidad</strong>
          <span>${res.nombreModalidad || "-"}</span>
        </div>
        <div class="result-detail-item">
          <strong>Hora de Ingreso</strong>
          <span>${new Date().toLocaleTimeString("es-AR")}</span>
        </div>
      </div>
    `;
  } else {
    card.className = "validation-result-card error";
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
          <strong>Codigo presentado</strong>
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
  if (!tbody) return;

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

// -------------------------------------------------------------
// Gestion de Eventos (gestion-eventos.html)
// -------------------------------------------------------------
async function inicializarGestionEventos() {
  await cargarEventosGestion();
  inicializarMapaCrearEvento();

  document.getElementById("formCrearEvento")?.addEventListener("submit", crearNuevoEvento);
  document.getElementById("formAgregarModalidad")?.addEventListener("submit", agregarModalidadAEvento);
  document.getElementById("btnBuscarLugarMapa")?.addEventListener("click", buscarLugarEnMapa);

  document.getElementById("cancelarEventoSelect")?.addEventListener("change", renderizarModalidadesParaCancelar);
  document.getElementById("btnCancelarEventoSeleccionado")?.addEventListener("click", () => {
    const sel = document.getElementById("cancelarEventoSelect");
    const ev = state.eventos.find(e => e.id === sel?.value);
    if (ev) {
      cancelarEvento(ev.id, ev.nombre);
    }
  });
}

async function cargarEventosGestion() {
  try {
    const res = await fetch(`${API_GESTION}/eventos`);
    if (!res.ok) throw new Error("Error al obtener eventos.");
    state.eventos = await res.json();

    const selects = ["modalidadEventoSelect", "cancelarEventoSelect"];
    selects.forEach(id => {
      const sel = document.getElementById(id);
      if (!sel) return;
      sel.innerHTML = "";
      state.eventos.forEach(ev => {
        const opt = document.createElement("option");
        opt.value = ev.id;
        opt.textContent = `${ev.nombre} (${ev.cancelado ? 'Cancelado' : 'Disponible'})`;
        sel.appendChild(opt);
      });
    });

    renderizarModalidadesParaCancelar();
  } catch (err) {
    console.error("Error al cargar eventos para gestion:", err);
  }
}

async function crearNuevoEvento(e) {
  e.preventDefault();

  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Accion restringida: debe operar como Organizador.");
    return;
  }

  const nombre = document.getElementById("nuevoEventoNombre").value.trim();
  const descripcion = document.getElementById("nuevoEventoDescripcion").value.trim();
  const fecha = document.getElementById("nuevoEventoFecha").value;
  const lugar = document.getElementById("nuevoEventoLugar").value.trim();
  const latitud = parseFloat(document.getElementById("nuevoEventoLatitud").value) || -34.6037;
  const longitud = parseFloat(document.getElementById("nuevoEventoLongitud").value) || -58.3816;

  if (!nombre || !fecha || !lugar) {
    alert("Complete los campos obligatorios: Nombre, Fecha y Lugar.");
    return;
  }

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

    alert(`Evento "${data.nombre}" creado exitosamente.`);
    document.getElementById("formCrearEvento").reset();

    const inputLat = document.getElementById("nuevoEventoLatitud");
    const inputLng = document.getElementById("nuevoEventoLongitud");
    if (inputLat) inputLat.value = "-34.6037";
    if (inputLng) inputLng.value = "-58.3816";

    await cargarEventosGestion();

  } catch (err) {
    alert(`Error al crear evento: ${err.message}`);
  }
}

async function agregarModalidadAEvento(e) {
  e.preventDefault();

  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Accion restringida: debe operar como Organizador.");
    return;
  }

  const idEvento = document.getElementById("modalidadEventoSelect").value;
  const nombre = document.getElementById("modalidadNombre").value.trim();
  const precio = parseFloat(document.getElementById("modalidadPrecio").value);
  const cupoMaximo = parseInt(document.getElementById("modalidadCupo").value, 10);
  const beneficios = document.getElementById("modalidadBeneficios").value.trim();

  if (!idEvento || !nombre || isNaN(precio) || isNaN(cupoMaximo)) {
    alert("Complete los campos obligatorios de la modalidad.");
    return;
  }

  try {
    const payload = { nombre, precio, beneficios, cupoMaximo };

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

    alert(`Modalidad "${data.nombre}" agregada exitosamente.`);
    document.getElementById("formAgregarModalidad").reset();
    await cargarEventosGestion();

  } catch (err) {
    alert(`Error al agregar modalidad: ${err.message}`);
  }
}

async function cancelarEvento(idEvento, nombreEvento) {
  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Accion restringida: debe operar como Organizador.");
    return;
  }

  if (!confirm(`Esta seguro de cancelar el evento "${nombreEvento}"?\nYa no se podran vender mas entradas.`)) {
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

    if (document.getElementById("eventsGrid")) {
      cargarEventos();
    }
    if (document.getElementById("cancelarEventoSelect")) {
      cargarEventosGestion();
    }
    if (document.getElementById("reporteTableBody")) {
      cargarReporteRecaudacion();
    }
    if (document.getElementById("panelCompraComprador")) {
      cargarDetalleDesdeUrl();
    }

  } catch (err) {
    alert(`Error al cancelar evento: ${err.message}`);
  }
}

async function cancelarModalidad(idEvento, idModalidad, nombreModalidad) {
  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Accion restringida: debe operar como Organizador.");
    return;
  }

  if (!confirm(`Desea cancelar la modalidad "${nombreModalidad}"?\nLos compradores ya no podran adquirir entradas de esta modalidad.`)) {
    return;
  }

  try {
    const res = await fetch(`${API_GESTION}/eventos/${idEvento}/modalidades/${idModalidad}/cancelar`, {
      method: "PUT",
      headers: {
        "X-Dni": state.usuarioActivo.dni.toString()
      }
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.error || "No se pudo cancelar la modalidad.");

    alert(data.mensaje || "Modalidad cancelada exitosamente.");

    if (document.getElementById("cancelarEventoSelect")) {
      cargarEventosGestion();
    }
    if (document.getElementById("panelCompraComprador")) {
      cargarDetalleDesdeUrl();
    }

  } catch (err) {
    alert(`Error al cancelar modalidad: ${err.message}`);
  }
}

function renderizarModalidadesParaCancelar() {
  const selectEv = document.getElementById("cancelarEventoSelect");
  const tbody = document.getElementById("tablaModalidadesCancelarBody");
  if (!tbody) return;

  const eventoId = selectEv ? selectEv.value : null;
  const evento = state.eventos.find(e => e.id === eventoId);

  tbody.innerHTML = "";

  if (!evento) {
    tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; color: var(--gray-500);">Selecciona un evento para gestionar sus modalidades.</td></tr>`;
    return;
  }

  if (!evento.modalidades || evento.modalidades.length === 0) {
    tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; color: var(--gray-500);">Este evento no tiene modalidades registradas aun.</td></tr>`;
    return;
  }

  evento.modalidades.forEach(m => {
    const tr = document.createElement("tr");

    let estadoBadge = `<span class="badge badge-success">Activa</span>`;
    let btnAccionHtml = `
      <button class="btn btn-danger btn-sm btn-canc-mod" data-mod-id="${m.id}" data-mod-nombre="${m.nombre}">
        Cancelar
      </button>
    `;

    if (m.cancelada) {
      estadoBadge = `<span class="badge badge-danger">Cancelada</span>`;
      btnAccionHtml = `<span style="color: var(--gray-500); font-size: 0.825rem; font-style: italic;">Cancelada</span>`;
    } else if (evento.cancelado) {
      btnAccionHtml = `<span style="color: var(--gray-500); font-size: 0.825rem; font-style: italic;">Evento Cancelado</span>`;
    }

    tr.innerHTML = `
      <td><strong>${m.nombre}</strong><br><small style="color: var(--gray-500);">${m.beneficios || "Sin beneficios"}</small></td>
      <td>$${m.precio.toLocaleString('es-AR')}</td>
      <td>${m.cupoDisponible} / ${m.cupoMaximo}</td>
      <td>${estadoBadge}</td>
      <td>${btnAccionHtml}</td>
    `;

    const btnCanc = tr.querySelector(".btn-canc-mod");
    if (btnCanc) {
      btnCanc.addEventListener("click", () => cancelarModalidad(evento.id, m.id, m.nombre));
    }

    tbody.appendChild(tr);
  });
}

function inicializarMapaCrearEvento() {
  setTimeout(() => {
    const mapDiv = document.getElementById("mapaCrearEvento");
    if (!mapDiv) return;

    let defaultLat = parseFloat(document.getElementById("nuevoEventoLatitud")?.value) || -34.6037;
    let defaultLng = parseFloat(document.getElementById("nuevoEventoLongitud")?.value) || -58.3816;

    if (!state.mapaCrearEvento) {
      state.mapaCrearEvento = L.map('mapaCrearEvento', { attributionControl: false }).setView([defaultLat, defaultLng], 13);

      const googleCalles = L.tileLayer('https://{s}.google.com/vt/lyrs=m&x={x}&y={y}&z={z}', {
        maxZoom: 20,
        subdomains: ['mt0', 'mt1', 'mt2', 'mt3']
      });
      googleCalles.addTo(state.mapaCrearEvento);

      state.markerCrearEvento = L.marker([defaultLat, defaultLng], { draggable: true }).addTo(state.mapaCrearEvento);

      state.markerCrearEvento.on('dragend', function (event) {
        const marker = event.target;
        const position = marker.getLatLng();
        actualizarUbicacionSeleccionada(position.lat, position.lng, true);
      });

      state.mapaCrearEvento.on('click', function (e) {
        state.markerCrearEvento.setLatLng(e.latlng);
        actualizarUbicacionSeleccionada(e.latlng.lat, e.latlng.lng, true);
      });
    } else {
      state.mapaCrearEvento.invalidateSize();
      state.mapaCrearEvento.setView([defaultLat, defaultLng], 13);
      if (state.markerCrearEvento) {
        state.markerCrearEvento.setLatLng([defaultLat, defaultLng]);
      }
    }
  }, 150);
}

function actualizarUbicacionSeleccionada(lat, lng, buscarNombre = false) {
  const inputLat = document.getElementById("nuevoEventoLatitud");
  const inputLng = document.getElementById("nuevoEventoLongitud");

  if (inputLat) inputLat.value = lat.toFixed(6);
  if (inputLng) inputLng.value = lng.toFixed(6);

  if (buscarNombre) {
    const inputLugar = document.getElementById("nuevoEventoLugar");
    if (inputLugar && !inputLugar.value.trim()) {
      inputLugar.value = `Ubicacion (${lat.toFixed(4)}, ${lng.toFixed(4)})`;
    }
  }
}

async function buscarLugarEnMapa() {
  const inputLugar = document.getElementById("nuevoEventoLugar");
  const query = inputLugar ? inputLugar.value.trim() : "";

  if (!query) {
    alert("Ingrese una direccion o nombre de lugar para buscar en el mapa.");
    inputLugar?.focus();
    return;
  }

  const btnBuscar = document.getElementById("btnBuscarLugarMapa");
  const textoOriginal = btnBuscar ? btnBuscar.innerHTML : "";
  if (btnBuscar) {
    btnBuscar.disabled = true;
    btnBuscar.innerHTML = "Buscando...";
  }

  try {
    const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=1`;
    const res = await fetch(url, { headers: { 'Accept-Language': 'es' } });
    const data = await res.json();

    if (!data || data.length === 0) {
      alert(`No se encontraron coordenadas para "${query}". Puede hacer clic en el mapa para marcar la ubicacion.`);
      return;
    }

    const lat = parseFloat(data[0].lat);
    const lon = parseFloat(data[0].lon);

    if (state.mapaCrearEvento && state.markerCrearEvento) {
      state.mapaCrearEvento.setView([lat, lon], 15);
      state.markerCrearEvento.setLatLng([lat, lon]);
      state.markerCrearEvento.bindPopup(`<strong>Ubicacion seleccionada:</strong><br>${data[0].display_name}`).openPopup();
    }

    actualizarUbicacionSeleccionada(lat, lon, false);

  } catch (err) {
    console.error("Error al buscar en mapa:", err);
    alert("No se pudo conectar con el servicio de busqueda. Puede posicionar el marcador haciendo clic en el mapa.");
  } finally {
    if (btnBuscar) {
      btnBuscar.disabled = false;
      btnBuscar.innerHTML = textoOriginal;
    }
  }
}

// -------------------------------------------------------------
// Reporte de Recaudacion (reporte-recaudacion.html)
// -------------------------------------------------------------
async function cargarReporteRecaudacion() {
  const tbody = document.getElementById("reporteTableBody");
  if (!tbody) return;

  if (state.usuarioActivo?.rol !== "Organizador") {
    alert("Acceso denegado: El reporte de recaudacion es exclusivo para usuarios con rol Organizador.");
    window.location.href = "index.html";
    return;
  }

  tbody.innerHTML = "<tr><td colspan='8' style='text-align: center;'>Cargando reporte de recaudacion...</td></tr>";

  try {
    const res = await fetch(`${API_GESTION}/reportes/recaudacion`, {
      headers: {
        "X-Dni": state.usuarioActivo.dni.toString()
      }
    });

    if (!res.ok) {
      if (res.status === 403) throw new Error("Acceso denegado: No tiene permisos de Organizador.");
      throw new Error("Error al obtener el reporte.");
    }

    const reporte = await res.json();

    const totalRecaudado = Number(reporte.totalGeneralRecaudado || 0);
    const totalEntradas = reporte.totalGeneralEntradasVendidas || 0;

    document.getElementById("totalGeneralRecaudado").textContent = `$${totalRecaudado.toLocaleString('es-AR')}`;
    document.getElementById("totalGeneralEntradas").textContent = totalEntradas;

    tbody.innerHTML = "";

    if (!reporte.eventos || reporte.eventos.length === 0) {
      tbody.innerHTML = "<tr><td colspan='8' style='text-align: center; color: var(--slate-400);'>No hay eventos registrados.</td></tr>";
      return;
    }

    reporte.eventos.forEach(ev => {
      const tr = document.createElement("tr");

      const esPasado = new Date(ev.fecha) < new Date();
      let estadoHtml = `<span class="badge badge-success">Activo</span>`;
      if (ev.cancelado) {
        estadoHtml = `<span class="badge badge-danger">Cancelado</span>`;
      } else if (esPasado) {
        estadoHtml = `<span class="badge badge-secondary">Finalizado</span>`;
      }

      const modalidades = ev.modalidades || [];
      let modHtml = modalidades.map(m => {
        const monto = Number(m.recaudacion ?? m.recaudado ?? 0);
        return `
          <div style="font-size: 0.825rem; margin-bottom: 0.25rem;">
            <strong>${m.nombreModalidad}:</strong> ${m.entradasVendidas ?? 0} vendidas ($${monto.toLocaleString('es-AR')})
          </div>
        `;
      }).join('');

      let compradoresHtml = "-";
      const compradores = ev.compradores || [];
      if (compradores.length > 0) {
        compradoresHtml = compradores.map(c => {
          const tot = Number(c.total || 0);
          return `
            <div style="font-size: 0.825rem; margin-bottom: 0.25rem;">
              <strong>${c.nombre}</strong> <small style="color: var(--slate-500);">(${c.cantidadEntradas ?? 0} ent. - $${tot.toLocaleString('es-AR')})</small>
            </div>
          `;
        }).join('');
      } else {
        compradoresHtml = `<span style="color: var(--slate-400); font-size: 0.8rem; font-style: italic;">Sin compradores</span>`;
      }

      let btnCancelarHtml = !ev.cancelado 
        ? `<button class="btn btn-danger btn-sm btn-cancelar-ev" data-id="${ev.idEvento}" data-nombre="${ev.nombreEvento}">Cancelar</button>`
        : `<span style="color: var(--slate-400); font-size: 0.8rem;">Cancelado</span>`;

      const evRecaudacion = Number(ev.recaudacionTotal || 0);

      tr.innerHTML = `
        <td><strong>${ev.nombreEvento}</strong><br><small style="color: var(--slate-500);">${ev.lugar}</small></td>
        <td>${new Date(ev.fecha).toLocaleDateString('es-AR')}</td>
        <td>${estadoHtml}</td>
        <td><strong>${ev.entradasVendidas ?? 0}</strong></td>
        <td><strong style="color: var(--primary);">$${evRecaudacion.toLocaleString('es-AR')}</strong></td>
        <td>${modHtml || "-"}</td>
        <td>${compradoresHtml}</td>
        <td>${btnCancelarHtml}</td>
      `;

      const btnCanc = tr.querySelector(".btn-cancelar-ev");
      if (btnCanc) {
        btnCanc.addEventListener("click", () => cancelarEvento(ev.idEvento, ev.nombreEvento));
      }

      tbody.appendChild(tr);
    });

  } catch (err) {
    alert(`Error: ${err.message}`);
  }
}
