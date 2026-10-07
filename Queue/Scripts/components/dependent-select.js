(function () {
    'use strict';
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('select[data-depends-on]').forEach(function (select) {
            var source = document.getElementById(select.dataset.dependsOn);
            if (!source) return;
            var status = document.getElementById(select.id + '_error');
            var emptyLabel = select.options.length ? select.options[0].text : 'Todos';
            var pending;
            source.addEventListener('change', function () {
                if (pending) pending.abort();
                select.replaceChildren(new Option(emptyLabel, ''));
                select.disabled = false;
                if (status) status.textContent = '';
                if (!source.value || source.value === '00000000-0000-0000-0000-000000000000') return;
                pending = new AbortController();
                select.disabled = true;
                if (status) status.textContent = 'Cargando opciones…';
                fetch(select.dataset.optionsUrl, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ idGroupEmployee: source.value }), signal: pending.signal
                }).then(function (response) {
                    if (!response.ok || response.redirected) throw new Error();
                    return response.json();
                }).then(function (data) {
                    if (!Array.isArray(data)) throw new Error();
                    data.forEach(function (item) { select.add(new Option(item.Text, item.Value)); });
                    select.disabled = false;
                    if (status) status.textContent = data.length ? '' : 'No hay opciones disponibles.';
                }).catch(function (error) {
                    if (error.name === 'AbortError') return;
                    select.disabled = false;
                    if (status) status.textContent = 'No se pudieron cargar las opciones. Vuelve a seleccionar el grupo.';
                });
            });
            if (select.form) select.form.addEventListener('submit', function (event) {
                if (select.disabled) { event.preventDefault(); if (status) status.textContent = 'Espera a que termine la carga.'; }
            });
        });
    });
})();
