// ===== Popup "Buscar personas" =====
// Usa Views/Shared/BuscarPersonas.cshtml
//  - Cuenta pública  -> "Seguir" queda en "Siguiendo" de una vez
//  - Cuenta privada  -> "Seguir" queda en "Solicitado" hasta que acepte
window.Amigos = (function () {
    const fondo = document.getElementById('amModal');
    if (!fondo) return {};

    const url = fondo.dataset;
    const token = fondo.querySelector('input[name="__RequestVerificationToken"]').value;
    const inputTexto = document.getElementById('amTexto');
    const resultados = document.getElementById('amResultados');
    const listaSolicitudes = document.getElementById('amSolicitudes');
    const contador = document.getElementById('amContadorSolicitudes');

    let temporizador = null;
    let ultimaBusqueda = 0;

    // ---------- utilidades ----------

    function post(direccion, datos) {
        datos.__RequestVerificationToken = token;
        return fetch(direccion, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams(datos)
        });
    }

    function vacio(contenedor, texto) {
        contenedor.innerHTML = '';
        const p = document.createElement('p');
        p.className = 'am-vacio';
        p.textContent = texto;
        contenedor.appendChild(p);
    }

    // Avatar + nombre + @usuario, con link al perfil
    function filaPersona(persona) {
        const fila = document.createElement('div');
        fila.className = 'am-fila';

        const link = document.createElement('a');
        link.className = 'am-persona';
        link.href = url.urlPerfil + '/' + encodeURIComponent(persona.nombreUsuario);

        let avatar;
        if (persona.avatarUrl) {
            avatar = document.createElement('img');
            avatar.src = persona.avatarUrl;
            avatar.alt = '';
        } else {
            avatar = document.createElement('span');
            avatar.textContent = (persona.nombreMostrar || '?').charAt(0).toUpperCase();
        }
        avatar.className = 'am-avatar';

        const textos = document.createElement('div');
        const nombre = document.createElement('div');
        nombre.className = 'am-nombre';
        nombre.textContent = persona.nombreMostrar;
        if (persona.esPrivada) {
            const candado = document.createElement('i');
            candado.className = 'ti ti-lock';
            candado.title = 'Cuenta privada';
            nombre.append(' ', candado);
        }
        const usuario = document.createElement('div');
        usuario.className = 'am-usuario';
        usuario.textContent = '@' + persona.nombreUsuario + (persona.meSigue ? ' · Te sigue' : '');
        textos.append(nombre, usuario);

        link.append(avatar, textos);
        fila.appendChild(link);
        return fila;
    }

    // ---------- Buscar ----------

    function pintarBotonSeguir(boton, estado, meSigue) {
        boton.dataset.estado = estado;
        boton.className = estado === 'Ninguno' ? 'am-btn' : 'am-btn-sec';
        boton.textContent =
            estado === 'Aceptado' ? 'Siguiendo' :
            estado === 'Pendiente' ? 'Solicitado' :
            (meSigue ? 'Seguir también' : 'Seguir');
    }

    async function buscar() {
        const texto = inputTexto.value.trim();
        if (texto.replace(/^@/, '').length < 2) {
            vacio(resultados, 'Escribí al menos 2 letras.');
            return;
        }

        const numero = ++ultimaBusqueda;
        try {
            const r = await fetch(url.urlBuscar + '?texto=' + encodeURIComponent(texto));
            const personas = await r.json();
            if (numero !== ultimaBusqueda) return;   // llegó una búsqueda más nueva

            resultados.innerHTML = '';
            if (personas.length === 0) {
                vacio(resultados, 'No encontramos a nadie con ese nombre.');
                return;
            }

            personas.forEach(function (persona) {
                const fila = filaPersona(persona);

                const boton = document.createElement('button');
                boton.type = 'button';
                pintarBotonSeguir(boton, persona.estadoSeguimiento, persona.meSigue);

                boton.addEventListener('click', async function () {
                    if (boton.dataset.estado === 'Aceptado' && persona.esPrivada &&
                        !confirm('Si dejás de seguir esta cuenta privada, vas a tener que volver a pedir permiso. ¿Seguir?')) {
                        return;
                    }
                    boton.disabled = true;
                    try {
                        const resp = await post(url.urlSeguir, { nombreUsuario: persona.nombreUsuario });
                        const res = await resp.json().catch(() => ({}));
                        if (resp.ok) pintarBotonSeguir(boton, res.estadoSeguimiento, persona.meSigue);
                        else alert(res.mensaje || 'No se pudo completar la acción.');
                    } finally {
                        boton.disabled = false;
                    }
                });

                fila.appendChild(boton);
                resultados.appendChild(fila);
            });
        } catch {
            vacio(resultados, 'Ocurrió un error al buscar.');
        }
    }

    inputTexto.addEventListener('input', function () {
        clearTimeout(temporizador);
        temporizador = setTimeout(buscar, 300);   // espera a que deje de escribir
    });

    // ---------- Solicitudes ----------

    function actualizarContador(cantidad) {
        contador.textContent = cantidad;
        contador.style.display = cantidad > 0 ? '' : 'none';

        // Puntito en el botón que abre el popup (si la página tiene uno)
        document.querySelectorAll('[data-am-punto]').forEach(function (punto) {
            punto.style.display = cantidad > 0 ? '' : 'none';
        });
    }

    async function cargarSolicitudes() {
        try {
            const r = await fetch(url.urlSolicitudes);
            const solicitudes = await r.json();
            actualizarContador(solicitudes.length);

            listaSolicitudes.innerHTML = '';
            if (solicitudes.length === 0) {
                vacio(listaSolicitudes, 'No tenés solicitudes pendientes.');
                return;
            }

            solicitudes.forEach(function (s) {
                const fila = filaPersona(s);
                const acciones = document.createElement('div');
                acciones.className = 'am-acciones';

                [['Aceptar', true, 'am-btn'], ['Rechazar', false, 'am-btn-sec']].forEach(function (op) {
                    const b = document.createElement('button');
                    b.type = 'button';
                    b.className = op[2];
                    b.textContent = op[0];
                    b.addEventListener('click', async function () {
                        acciones.querySelectorAll('button').forEach(x => x.disabled = true);
                        const resp = await post(url.urlResponder, { nombreUsuario: s.nombreUsuario, acepta: op[1] });
                        if (resp.ok) {
                            cargarSolicitudes();
                        } else {
                            const res = await resp.json().catch(() => ({}));
                            alert(res.mensaje || 'No se pudo responder.');
                            acciones.querySelectorAll('button').forEach(x => x.disabled = false);
                        }
                    });
                    acciones.appendChild(b);
                });

                fila.appendChild(acciones);
                listaSolicitudes.appendChild(fila);
            });
        } catch {
            vacio(listaSolicitudes, 'No se pudieron cargar las solicitudes.');
        }
    }

    // ---------- Pestañas y abrir/cerrar ----------

    function mostrarPestana(nombre) {
        fondo.querySelectorAll('[data-am-tab]').forEach(t => t.classList.toggle('activa', t.dataset.amTab === nombre));
        fondo.querySelectorAll('[data-am-panel]').forEach(p => p.style.display = p.dataset.amPanel === nombre ? '' : 'none');
        if (nombre === 'solicitudes') cargarSolicitudes();
        else inputTexto.focus();
    }

    fondo.querySelectorAll('[data-am-tab]').forEach(function (tab) {
        tab.addEventListener('click', () => mostrarPestana(tab.dataset.amTab));
    });

    function abrir() {
        fondo.classList.add('abierto');
        mostrarPestana('buscar');
    }

    function cerrar() {
        fondo.classList.remove('abierto');
    }

    fondo.addEventListener('click', function (e) {
        if (e.target === fondo || e.target.closest('[data-am-cerrar]')) cerrar();
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && fondo.classList.contains('abierto')) cerrar();
    });

    // Al cargar la página: ¿hay solicitudes? (para el puntito)
    cargarSolicitudes();

    return { abrir: abrir };
})();
