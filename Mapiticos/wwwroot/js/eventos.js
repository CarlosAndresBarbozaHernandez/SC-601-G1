// ===== Popup "Planear salida" / "Invitar amigos" =====
// Usa Views/Eventos/ModalPlanear.cshtml
window.Eventos = (function () {
    const fondo = document.getElementById('evModal');
    if (!fondo) return {};

    const form = document.getElementById('evForm');
    const titulo = document.getElementById('evModalTitulo');
    const subtitulo = document.getElementById('evModalLugar');
    const camposEvento = document.getElementById('evCamposEvento');
    const listaAmigos = document.getElementById('evAmigos');
    const buscar = document.getElementById('evBuscarAmigo');
    const error = document.getElementById('evError');
    const botonGuardar = document.getElementById('evGuardar');
    const textoGuardar = document.getElementById('evGuardarTexto');
    const campoLugar = document.getElementById('evCampoLugar');
    const selectLugar = document.getElementById('evLugar');
    const inputTitulo = document.getElementById('evTitulo');

    let modo = 'crear';     // 'crear' | 'invitar'
    let lugarId = null;
    let eventoId = null;

    // "2026-10-31T08:00" en hora local, para el input datetime-local
    function aInputFecha(fecha) {
        const d = new Date(fecha.getTime() - fecha.getTimezoneOffset() * 60000);
        return d.toISOString().slice(0, 16);
    }

    function mostrarError(texto) {
        error.textContent = texto;
        error.style.display = texto ? 'block' : 'none';
    }

    function avatar(amigo) {
        if (amigo.avatarUrl) {
            const img = document.createElement('img');
            img.src = amigo.avatarUrl;
            img.alt = '';
            img.className = 'ev-avatar';
            return img;
        }
        const span = document.createElement('span');
        span.className = 'ev-avatar';
        span.textContent = (amigo.nombreMostrar || '?').charAt(0).toUpperCase();
        return span;
    }

    async function cargarAmigos() {
        listaAmigos.innerHTML = '<div class="ev-amigos-vacio">Cargando amigos...</div>';
        buscar.value = '';

        const url = fondo.dataset.urlAmigos + (eventoId ? '?eventoId=' + eventoId : '');
        try {
            const respuesta = await fetch(url);
            const amigos = await respuesta.json();
            listaAmigos.innerHTML = '';

            if (amigos.length === 0) {
                listaAmigos.innerHTML =
                    '<div class="ev-amigos-vacio">Todavía no tenés amigos para invitar.<br>Seguí a alguien y que te siga (o te acepte) para poder invitarle.</div>';
                return;
            }

            amigos.forEach(function (amigo) {
                const fila = document.createElement('label');
                fila.className = 'ev-amigo' + (amigo.yaInvitado ? ' ya' : '');
                fila.dataset.buscar = (amigo.nombreMostrar + ' ' + amigo.nombreUsuario).toLowerCase();

                const check = document.createElement('input');
                check.type = 'checkbox';
                check.name = 'Invitados';
                check.value = amigo.nombreUsuario;
                if (amigo.yaInvitado) { check.checked = true; check.disabled = true; }

                const textos = document.createElement('div');
                const nombre = document.createElement('div');
                nombre.className = 'nombre';
                nombre.textContent = amigo.nombreMostrar;
                const usuario = document.createElement('div');
                usuario.className = 'usuario';
                usuario.textContent = '@' + amigo.nombreUsuario + (amigo.yaInvitado ? ' · ya invitado' : '');
                textos.append(nombre, usuario);

                fila.append(check, avatar(amigo), textos);
                listaAmigos.appendChild(fila);
            });
        } catch {
            listaAmigos.innerHTML = '<div class="ev-amigos-vacio">No se pudieron cargar tus amigos.</div>';
        }
    }

    // Lista de lugares favoritos para el selector "¿A dónde van?"
    async function cargarLugares() {
        selectLugar.innerHTML = '<option value="">Cargando tus favoritos...</option>';
        try {
            const respuesta = await fetch(fondo.dataset.urlLugares);
            const lugares = await respuesta.json();

            selectLugar.innerHTML = '';
            const primera = document.createElement('option');
            primera.value = '';
            primera.textContent = lugares.length ? 'Elegí un lugar' : 'No tenés lugares favoritos todavía';
            selectLugar.appendChild(primera);

            lugares.forEach(function (lugar) {
                const opcion = document.createElement('option');
                opcion.value = lugar.lugarId;
                opcion.textContent = lugar.nombre + ' · ' + lugar.provincia;
                opcion.dataset.nombre = lugar.nombre;
                selectLugar.appendChild(opcion);
            });
        } catch {
            selectLugar.innerHTML = '<option value="">No se pudieron cargar los lugares</option>';
        }
    }

    // Si la persona no cambió el nombre, se sugiere "Salida a <lugar>"
    let tituloAutomatico = true;
    inputTitulo.addEventListener('input', function () { tituloAutomatico = false; });
    selectLugar.addEventListener('change', function () {
        const opcion = selectLugar.selectedOptions[0];
        if (tituloAutomatico && opcion && opcion.dataset.nombre) {
            inputTitulo.value = 'Salida a ' + opcion.dataset.nombre;
        }
    });

    function abrir() {
        mostrarError('');
        fondo.classList.add('abierto');
        cargarAmigos();
    }

    function cerrar() {
        fondo.classList.remove('abierto');
    }

    // Sin parámetros: botón de arriba (elige el lugar en la lista)
    // Con parámetros: botón de una tarjeta de favorito (lugar ya elegido)
    function abrirPlanear(idLugar, nombreLugar) {
        modo = 'crear';
        lugarId = idLugar || null;
        eventoId = null;

        titulo.textContent = 'Planear salida';
        camposEvento.style.display = '';
        textoGuardar.textContent = 'Crear y enviar invitaciones';

        form.reset();
        tituloAutomatico = true;

        if (lugarId) {
            subtitulo.textContent = '📍 ' + nombreLugar;
            subtitulo.style.display = '';
            campoLugar.style.display = 'none';
            inputTitulo.value = 'Salida a ' + nombreLugar;
        } else {
            subtitulo.textContent = '';
            subtitulo.style.display = 'none';
            campoLugar.style.display = '';
            inputTitulo.value = '';
            cargarLugares();
        }

        // Por defecto: mañana a las 8:00 a.m.
        const manana = new Date();
        manana.setDate(manana.getDate() + 1);
        manana.setHours(8, 0, 0, 0);
        const inputFecha = document.getElementById('evFecha');
        inputFecha.value = aInputFecha(manana);
        inputFecha.min = aInputFecha(new Date());

        abrir();
        (lugarId ? inputTitulo : selectLugar).focus();
    }

    // Desde Ver evento
    function abrirInvitar(idEvento) {
        modo = 'invitar';
        eventoId = idEvento;

        titulo.textContent = 'Invitar amigos';
        subtitulo.textContent = 'Elegí a quién más querés llevar.';
        subtitulo.style.display = '';
        camposEvento.style.display = 'none';
        textoGuardar.textContent = 'Enviar invitaciones';

        abrir();
    }

    buscar.addEventListener('input', function () {
        const texto = buscar.value.toLowerCase().trim();
        listaAmigos.querySelectorAll('.ev-amigo').forEach(function (fila) {
            fila.style.display = fila.dataset.buscar.includes(texto) ? '' : 'none';
        });
    });

    fondo.addEventListener('click', function (e) {
        if (e.target === fondo || e.target.closest('[data-ev-cerrar]')) cerrar();
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && fondo.classList.contains('abierto')) cerrar();
    });

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        mostrarError('');

        const datos = new URLSearchParams();
        datos.append('__RequestVerificationToken',
            form.querySelector('input[name="__RequestVerificationToken"]').value);

        const invitados = Array.from(listaAmigos.querySelectorAll('input[name="Invitados"]:checked:not(:disabled)'))
            .map(c => c.value);
        invitados.forEach(i => datos.append('Invitados', i));

        let url;
        if (modo === 'crear') {
            const idLugar = lugarId || selectLugar.value;
            const tituloEvento = inputTitulo.value.trim();
            const fecha = document.getElementById('evFecha').value;
            if (!idLugar) return mostrarError('Elegí a qué lugar van.');
            if (!tituloEvento) return mostrarError('Ponele un nombre a la salida.');
            if (!fecha) return mostrarError('Elegí la fecha y hora.');

            datos.append('LugarId', idLugar);
            datos.append('Titulo', tituloEvento);
            datos.append('FechaEvento', fecha);
            datos.append('Descripcion', document.getElementById('evDescripcion').value);
            url = fondo.dataset.urlCrear;
        } else {
            if (invitados.length === 0) return mostrarError('Elegí al menos un amigo.');
            datos.append('eventoId', eventoId);
            url = fondo.dataset.urlInvitar;
        }

        botonGuardar.disabled = true;
        try {
            const respuesta = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: datos
            });
            const resultado = await respuesta.json().catch(() => ({}));

            if (!respuesta.ok) {
                mostrarError(resultado.mensaje || 'No se pudo guardar. Intentá de nuevo.');
                return;
            }

            if (resultado.url) window.location.href = resultado.url;
            else window.location.reload();
        } catch {
            mostrarError('Ocurrió un error de conexión.');
        } finally {
            botonGuardar.disabled = false;
        }
    });

    return { abrirPlanear: abrirPlanear, abrirInvitar: abrirInvitar };
})();