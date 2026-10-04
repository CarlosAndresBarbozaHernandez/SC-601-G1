// ===== Revisión en vivo del @usuario =====
// Funciona con cualquier input que tenga data-verificar-usuario
(function () {
    document.querySelectorAll('[data-verificar-usuario]').forEach(function (campo) {
        const estado = document.getElementById(campo.dataset.estado);
        const original = campo.value; // en Editar perfil: tu @usuario actual
        let temporizador = null;
        let ultimaConsulta = 0;

        function mostrar(texto, tipo) {
            estado.textContent = texto;
            estado.className = 'estado-usuario' + (tipo ? ' ' + tipo : '');
        }

        campo.addEventListener('input', function () {
            // Minúsculas y sin espacios, mientras escribe
            const limpio = campo.value.toLowerCase().replace(/\s/g, '');
            if (limpio !== campo.value) {
                campo.value = limpio;
            }

            clearTimeout(temporizador);

            if (limpio === '') {
                mostrar('');
                return;
            }
            if (original && limpio === original) {
                mostrar('Es tu usuario actual.', 'ok');
                return;
            }

            mostrar('Revisando…');

            // Espera a que deje de escribir 400 ms antes de preguntar al servidor
            temporizador = setTimeout(async function () {
                const numero = ++ultimaConsulta;
                try {
                    const urlBase = campo.dataset.url || '/Cuenta/UsuarioDisponible';
                    const respuesta = await fetch(urlBase + '?nombre=' + encodeURIComponent(limpio));
                    const datos = await respuesta.json();
                    if (numero !== ultimaConsulta) return; // llegó una respuesta vieja, se ignora
                    mostrar(datos.mensaje, datos.disponible ? 'ok' : 'mal');
                } catch (error) {
                    mostrar('');
                }
            }, 400);
        });
    });
})();