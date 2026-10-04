// Botones de la pantalla "Ver lugar": like de cada aventura y favorito del lugar.
(function () {
    // Token anti-falsificación que pone @Html.AntiForgeryToken() en la vista
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    // Manda un POST y devuelve el JSON de respuesta (o null si falla)
    async function enviar(url, datos) {
        try {
            const respuesta = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: new URLSearchParams(datos)
            });
            return respuesta.ok ? await respuesta.json() : null;
        } catch {
            return null;   // sin conexión
        }
    }

    // ===== Like en cada aventura =====
    document.querySelectorAll('.btn-like').forEach(boton => {
        boton.addEventListener('click', async () => {
            boton.disabled = true;   // evita doble clic mientras responde el servidor
            const r = await enviar(boton.dataset.url, { aventuraId: boton.dataset.aventura });
            boton.disabled = false;
            if (!r) return;

            boton.classList.toggle('activo', r.meGusta);
            boton.setAttribute('aria-pressed', r.meGusta);
            boton.closest('.publicacion').querySelector('.cantidad-likes').textContent = r.cantidadLikes;
        });
    });

    // ===== Favorito del lugar =====
    const favorito = document.getElementById('btn-favorito');
    if (favorito) {
        favorito.addEventListener('click', async () => {
            favorito.disabled = true;
            const r = await enviar(favorito.dataset.url, { lugarId: favorito.dataset.lugar });
            favorito.disabled = false;
            if (!r) return;

            favorito.classList.toggle('activo', r.esFavorito);
            favorito.setAttribute('aria-pressed', r.esFavorito);
            document.getElementById('cantidad-favoritos').textContent = r.cantidadFavoritos;
        });
    }

    // ===== Agregar fotos a una aventura mía =====
    document.querySelectorAll('.input-fotos').forEach(input => {
        input.addEventListener('change', async () => {
            const fotos = [...input.files];
            if (fotos.length === 0) return;

            const disponibles = parseInt(input.dataset.disponibles);
            if (fotos.length > disponibles) {
                alert('Solo podés agregar ' + disponibles + (disponibles === 1 ? ' foto más.' : ' fotos más.'));
                input.value = '';
                return;
            }
            for (const f of fotos) {
                if (!['image/jpeg', 'image/png', 'image/webp'].includes(f.type)) {
                    alert('"' + f.name + '" no es una foto JPG, PNG o WEBP.');
                    input.value = '';
                    return;
                }
                if (f.size > 5 * 1024 * 1024) {
                    alert('"' + f.name + '" pesa más de 5 MB.');
                    input.value = '';
                    return;
                }
            }

            const datos = new FormData();
            datos.append('aventuraId', input.dataset.aventura);
            fotos.forEach(f => datos.append('fotos', f));

            const etiqueta = input.closest('label');
            etiqueta.classList.add('subiendo');

            let respuesta = null;
            try {
                respuesta = await fetch(input.dataset.url, {
                    method: 'POST',
                    headers: { 'RequestVerificationToken': token },
                    body: datos      // sin Content-Type: el navegador lo pone solo
                });
            } catch {
                // sin conexión
            }

            if (respuesta && respuesta.ok) {
                location.reload();   // recarga para mostrar las fotos nuevas
                return;
            }

            etiqueta.classList.remove('subiendo');
            input.value = '';
            const mensaje = respuesta ? await respuesta.text() : '';
            alert(mensaje || 'No se pudieron subir las fotos. Intentá de nuevo.');
        });
    });

    // ===== Borrar una foto de mi aventura =====
    document.querySelectorAll('.btn-eliminar-foto').forEach(boton => {
        boton.addEventListener('click', async () => {
            if (!confirm('¿Borrar esta foto? No se puede deshacer.')) return;

            boton.disabled = true;
            let respuesta = null;
            try {
                respuesta = await fetch(boton.dataset.url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': token
                    },
                    body: new URLSearchParams({ fotoId: boton.dataset.foto })
                });
            } catch {
                // sin conexión
            }

            if (respuesta && respuesta.ok) {
                location.reload();   // recarga para que se acomoden las fotos que quedan
                return;
            }

            boton.disabled = false;
            alert('No se pudo borrar la foto. Intentá de nuevo.');
        });
    });
})();