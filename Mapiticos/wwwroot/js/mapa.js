(function () {
    const contenedor = document.getElementById('mapa');
    const urlPuntos = contenedor.dataset.puntos;
    const urlCrear = contenedor.dataset.crear;
    const urlVerLugar = contenedor.dataset.verLugar;   // /Lugares/Ver
    const urlFavorito = contenedor.dataset.favorito;   // /Lugares/Favorito
    const urlAgregar = contenedor.dataset.agregar;     // /Lugares/Agregar
    const urlFotos = contenedor.dataset.fotos;         // /Lugares/Fotos
    const lugarEnfocado = contenedor.dataset.lugar ? parseInt(contenedor.dataset.lugar) : null;

    const formulario = document.getElementById('form-punto');
    const botonAgregar = document.getElementById('btn-agregar');
    const ayuda = document.getElementById('mapa-ayuda');
    const textoError = document.getElementById('mapa-error');
    const token = formulario.elements['__RequestVerificationToken'].value;

    let lugares = [];         // todos los pines que devolvió el servidor
    let etiquetaActiva = 0;   // 0 = todas
    let modoAgregar = false;
    let marcadorNuevo = null; // el pin provisional mientras se llena el formulario

    // Mapa centrado en Costa Rica
    const mapa = L.map('mapa').setView([9.75, -84.1], 8);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(mapa);

    const capaPuntos = L.layerGroup().addTo(mapa);

    // ===== Popup de un lugar =====
    // Se arma con textContent (no innerHTML): los nombres los escribe
    // cualquier usuario y así no se puede inyectar HTML ni scripts.
    function crearPopup(l) {
        const caja = document.createElement('div');

        const titulo = document.createElement('strong');
        titulo.textContent = l.nombre;

        const detalle = document.createElement('div');
        detalle.textContent = l.provincia + ' · ' + l.cantidadAventuras +
            (l.cantidadAventuras === 1 ? ' aventura' : ' aventuras');

        caja.append(titulo, detalle);

        const acciones = document.createElement('div');
        acciones.className = 'popup-acciones';

        const ver = document.createElement('a');
        ver.className = 'boton-chip';
        ver.href = urlVerLugar + '/' + l.lugarId;
        ver.textContent = 'Ver aventuras';

        // Estrella de favorito
        const estrella = document.createElement('button');
        estrella.type = 'button';
        estrella.setAttribute('aria-label', 'Guardar en favoritos');
        const icono = document.createElement('i');
        icono.className = 'ti ti-star';
        estrella.append(icono);

        function pintarEstrella() {
            estrella.className = 'popup-favorito' + (l.esFavorito ? ' activo' : '');
            estrella.setAttribute('aria-pressed', l.esFavorito);
        }
        pintarEstrella();

        estrella.addEventListener('click', async () => {
            estrella.disabled = true;
            try {
                const respuesta = await fetch(urlFavorito, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': token
                    },
                    body: new URLSearchParams({ lugarId: l.lugarId })
                });
                if (respuesta.ok) {
                    l.esFavorito = (await respuesta.json()).esFavorito;
                    pintarEstrella();
                }
            } catch {
                // sin conexión: la estrella se queda como estaba
            }
            estrella.disabled = false;
        });

        // Agregar mi aventura a este lugar
        const agregar = document.createElement('a');
        agregar.className = 'boton-chip';
        agregar.href = urlAgregar + '/' + l.lugarId;
        const mas = document.createElement('i');
        mas.className = 'ti ti-plus';
        agregar.append(mas, 'Agregar');

        acciones.append(ver, agregar, estrella);
        caja.append(acciones);

        return caja;
    }

    // ===== Dibujar pines (aplica el filtro de etiqueta) =====
    function dibujar() {
        capaPuntos.clearLayers();

        lugares
            .filter(l => etiquetaActiva === 0 || l.etiquetas.some(e => e.etiquetaId === etiquetaActiva))
            .forEach(l => {
                const marcador = L.circleMarker([l.latitud, l.longitud], {
                    radius: 9, weight: 2, color: '#FFFFFF', fillOpacity: 0.95,
                    fillColor: l.tengoAventura ? '#22392B' : '#7B3F8C'   // yo estuve = bosque, solo otros = guaria
                }).bindPopup(crearPopup(l));
                marcador.lugarId = l.lugarId;
                marcador.addTo(capaPuntos);
            });
    }

    function enfocar(lugarId) {
        capaPuntos.eachLayer(m => {
            if (m.lugarId === lugarId) {
                mapa.setView(m.getLatLng(), 14);
                m.openPopup();
            }
        });
    }

    async function cargarPuntos(enfocarId) {
        const respuesta = await fetch(urlPuntos);
        if (!respuesta.ok) return;
        lugares = await respuesta.json();
        dibujar();
        if (enfocarId) enfocar(enfocarId);
    }

    // ===== Filtros =====
    const botonesFiltro = document.querySelectorAll('.mapa-filtros button');
    botonesFiltro.forEach(boton => boton.addEventListener('click', () => {
        etiquetaActiva = parseInt(boton.dataset.etiqueta);
        botonesFiltro.forEach(b => b.classList.toggle('activo', b === boton));
        dibujar();
    }));

    // ===== Agregar un lugar =====
    function cerrarFormulario() {
        formulario.hidden = true;
        formulario.reset();
        textoError.hidden = true;
        ayuda.hidden = true;
        modoAgregar = false;
        if (marcadorNuevo) { marcadorNuevo.remove(); marcadorNuevo = null; }
    }

    botonAgregar.addEventListener('click', () => {
        if (modoAgregar) { cerrarFormulario(); return; }
        modoAgregar = true;
        ayuda.hidden = false;
    });

    document.getElementById('btn-cancelar').addEventListener('click', cerrarFormulario);

    // En modo agregar, el toque en el mapa fija la ubicación y abre el formulario
    mapa.on('click', e => {
        if (!modoAgregar) return;
        if (marcadorNuevo) marcadorNuevo.remove();

        marcadorNuevo = L.circleMarker(e.latlng, {
            radius: 9, weight: 2, color: '#FFFFFF', fillOpacity: 0.95, fillColor: '#A7B08C'
        }).addTo(mapa);

        formulario.elements['latitud'].value = e.latlng.lat.toFixed(6);
        formulario.elements['longitud'].value = e.latlng.lng.toFixed(6);
        ayuda.hidden = true;
        formulario.hidden = false;
    });

    // Revisa las fotos antes de mandar nada. null = todo bien
    function errorEnFotos(archivos) {
        if (archivos.length > 5) return 'Podés subir máximo 5 fotos.';
        for (const a of archivos) {
            if (!['image/jpeg', 'image/png', 'image/webp'].includes(a.type))
                return '"' + a.name + '" no es una foto JPG, PNG o WEBP.';
            if (a.size > 5 * 1024 * 1024)
                return '"' + a.name + '" pesa más de 5 MB.';
        }
        return null;
    }

    function mostrarError(texto) {
        textoError.textContent = texto;
        textoError.hidden = false;
    }

    formulario.addEventListener('submit', async e => {
        e.preventDefault();
        const campos = formulario.elements;
        const fotos = [...campos['fotos'].files];

        const problemaFotos = errorEnFotos(fotos);
        if (problemaFotos) { mostrarError(problemaFotos); return; }

        const datos = {
            titulo: campos['titulo'].value.trim(),
            descripcion: campos['descripcion'].value.trim() || null,
            provinciaId: parseInt(campos['provinciaId'].value),
            latitud: parseFloat(campos['latitud'].value),
            longitud: parseFloat(campos['longitud'].value),
            esPublica: campos['esPublica'].checked,
            etiquetaIds: [...formulario.querySelectorAll('input[name="etiqueta"]:checked')]
                .map(c => parseInt(c.value))
        };

        let respuesta = null;
        try {
            respuesta = await fetch(urlCrear, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    // el token anti-falsificación va en el header
                    'RequestVerificationToken': campos['__RequestVerificationToken'].value
                },
                body: JSON.stringify(datos)
            });
        } catch {
            // sin conexión: se muestra el mismo mensaje de error
        }

        if (!respuesta || !respuesta.ok) {
            mostrarError('No se pudo guardar. Revisá los datos e intentá de nuevo.');
            return;
        }

        const { aventuraId, lugarId } = await respuesta.json();

        // El lugar ya quedó guardado; ahora se suben las fotos (si hay)
        if (fotos.length > 0) {
            const datosFotos = new FormData();
            datosFotos.append('aventuraId', aventuraId);
            fotos.forEach(f => datosFotos.append('fotos', f));

            let respuestaFotos = null;
            try {
                respuestaFotos = await fetch(urlFotos, {
                    method: 'POST',
                    headers: { 'RequestVerificationToken': token },
                    body: datosFotos      // sin Content-Type: el navegador lo pone solo
                });
            } catch {
                // sin conexión
            }
            if (!respuestaFotos || !respuestaFotos.ok) {
                alert('Tu lugar se guardó, pero las fotos no se pudieron subir. Podés agregarlas desde "+ Agregar".');
            }
        }

        cerrarFormulario();
        await cargarPuntos(lugarId);   // recarga desde la BD y abre el pin nuevo
    });

    cargarPuntos(lugarEnfocado);
})();