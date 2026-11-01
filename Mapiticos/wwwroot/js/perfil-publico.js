// ===== Perfil de otra persona: botón Seguir =====
// Usa Views/Amigos/Perfil.cshtml
// Al terminar recarga, porque cambian los contadores y lo que se puede ver.
(function () {
    const boton = document.getElementById('botonSeguir');
    if (!boton) return;

    const token = document.querySelector('#formAntiforgery input[name="__RequestVerificationToken"]').value;

    boton.addEventListener('click', async function () {
        if (boton.dataset.confirmar === 'si' &&
            !confirm('Si dejás de seguir esta cuenta privada, vas a tener que volver a pedir permiso. ¿Continuar?')) {
            return;
        }

        boton.disabled = true;
        try {
            const respuesta = await fetch(boton.dataset.url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({
                    nombreUsuario: boton.dataset.usuario,
                    __RequestVerificationToken: token
                })
            });

            if (respuesta.ok) {
                location.reload();
                return;
            }

            const datos = await respuesta.json().catch(() => ({}));
            alert(datos.mensaje || 'No se pudo completar la acción.');
        } catch {
            alert('No se pudo conectar con el servidor.');
        }
        boton.disabled = false;
    });
})();
