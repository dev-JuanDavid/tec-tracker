(function () {
    'use strict';
    var exportLibraries = {};
    function loadExportLibrary(kind) {
        if (kind === 'excel' && window.XLSX || kind === 'pdf' && window.jspdf) return Promise.resolve();
        if (!exportLibraries[kind]) exportLibraries[kind] = new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.src = kind === 'excel' ? 'https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js' : 'https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js';
            script.onload = resolve;
            script.onerror = function () { delete exportLibraries[kind]; script.remove(); reject(new Error()); };
            document.head.appendChild(script);
        });
        return exportLibraries[kind];
    }
    function normalized(value) {
        return value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('es');
    }
    function initialize(root) {
        if (root.dataset.initialized) return;
        root.dataset.initialized = 'true';
        var table = root.querySelector('table');
        var rows = Array.from(table.querySelectorAll('[data-table-row]'));
        var search = document.getElementById(table.id + '_search');
        var size = document.getElementById(table.id + '_size');
        var empty = root.querySelector('[data-table-empty]');
        var emptyText = root.querySelector('[data-empty-message]');
        var info = root.querySelector('[data-table-info]');
        var buttons = Array.from(root.querySelectorAll('[data-page-action]'));
        var paginate = root.dataset.paginate === 'true';
        var page = 0, pages = 1;
        if (size && !size.value) {
            var initial = Number(root.dataset.pageSize);
            if (!Number.isInteger(initial) || initial < 1) initial = 10;
            size.add(new Option(String(initial), String(initial), true, true));
        }
        var texts = new Map(rows.map(function (row) {
            return [row, normalized(Array.from(row.querySelectorAll('[data-searchable="true"]')).map(function (cell) { return cell.textContent; }).join(' '))];
        }));
        function render() {
            var query = normalized(search ? search.value.trim() : '');
            var filtered = rows.filter(function (row) { return texts.get(row).includes(query); });
            var limit = paginate ? Number(size.value) : -1;
            if (limit < 1) limit = Math.max(filtered.length, 1);
            pages = Math.max(1, Math.ceil(filtered.length / limit));
            page = Math.min(page, pages - 1);
            rows.forEach(function (row) { row.hidden = true; });
            var start = page * limit;
            filtered.slice(start, start + limit).forEach(function (row) { row.hidden = false; });
            empty.hidden = filtered.length > 0;
            emptyText.textContent = rows.length ? 'No se encontraron registros para esta búsqueda.' : emptyText.dataset.defaultMessage;
            if (info) info.textContent = (filtered.length ? start + 1 : 0) + ' - ' + Math.min(start + limit, filtered.length) + ' de ' + filtered.length + (query && rows.length !== filtered.length ? ' (' + rows.length + ' en total)' : '');
            buttons.forEach(function (button) {
                button.disabled = button.dataset.pageAction === 'first' || button.dataset.pageAction === 'previous' ? page === 0 : page === pages - 1;
            });
        }
        if (search) search.addEventListener('input', function () { page = 0; render(); });
        if (size) size.addEventListener('change', function () { page = 0; render(); });
        buttons.forEach(function (button) {
            button.addEventListener('click', function () {
                var action = button.dataset.pageAction;
                page = action === 'first' ? 0 : action === 'last' ? pages - 1 : action === 'next' ? page + 1 : page - 1;
                render();
            });
        });
        Array.from(table.querySelectorAll('thead th')).forEach(function (header, index) {
            if (!rows.length || rows.every(function (row) { return row.children[index].dataset.searchable === 'false'; })) return;
            var button = document.createElement('button');
            button.type = 'button';
            button.textContent = header.textContent;
            button.className = 'rounded focus:outline-none focus:ring-4 focus:ring-[#6d5fb9]/20';
            button.setAttribute('aria-label', 'Ordenar por ' + header.textContent);
            header.replaceChildren(button);
            button.addEventListener('click', function () {
                var ascending = header.getAttribute('aria-sort') !== 'ascending';
                table.querySelectorAll('thead th').forEach(function (cell) { cell.removeAttribute('aria-sort'); });
                header.setAttribute('aria-sort', ascending ? 'ascending' : 'descending');
                rows.sort(function (a, b) { return a.children[index].textContent.trim().localeCompare(b.children[index].textContent.trim(), 'es', { numeric: true }) * (ascending ? 1 : -1); });
                rows.forEach(function (row) { empty.parentNode.insertBefore(row, empty); });
                page = 0;
                render();
            });
        });
        root.querySelectorAll('[data-export]').forEach(function (button) {
            button.addEventListener('click', async function () {
                var status = root.querySelector('[data-export-status]');
                var selected = rows.filter(function (row) { return texts.get(row).includes(normalized(search ? search.value.trim() : '')); });
                if (!selected.length) { status.textContent = 'No hay registros para exportar.'; return; }
                var columns = Array.from(selected[0].children).map(function (cell, index) { return cell.dataset.searchable === 'true' ? index : -1; }).filter(function (index) { return index >= 0; });
                var headers = Array.from(table.querySelectorAll('thead th'));
                var values = [columns.map(function (index) { return headers[index].textContent.trim(); })].concat(selected.map(function (row) {
                    return columns.map(function (index) { return row.children[index].textContent.trim(); });
                }));
                button.disabled = true;
                status.textContent = 'Preparando exportación…';
                try {
                    await loadExportLibrary(button.dataset.export);
                    if (button.dataset.export === 'excel') {
                        var book = XLSX.utils.book_new();
                        XLSX.utils.book_append_sheet(book, XLSX.utils.aoa_to_sheet(values), 'Reporte');
                        XLSX.writeFile(book, 'reporte.xlsx');
                    } else {
                        var pdf = new window.jspdf.jsPDF({ orientation: 'landscape' });
                        var title = table.querySelector('caption').textContent.trim();
                        var y = 20;
                        pdf.setFontSize(14);
                        pdf.text(title, 14, y);
                        pdf.setFontSize(9);
                        y += 12;
                        values.forEach(function (row) {
                            var lines = pdf.splitTextToSize(row.join(' | '), 265);
                            lines.forEach(function (line) {
                                if (y > 190) { pdf.addPage(); y = 20; }
                                pdf.text(line, 14, y); y += 5;
                            });
                            y += 3;
                        });
                        pdf.save('reporte.pdf');
                    }
                    status.textContent = 'Exportación preparada.';
                } catch (error) { status.textContent = 'No se pudo exportar. Inténtalo nuevamente.'; }
                finally { button.disabled = false; }
            });
        });
        render();
    }
    function initializeAll() { document.querySelectorAll('[data-component-table]').forEach(initialize); }
    window.TecTables = { initialize: initializeAll };
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', initializeAll);
    else initializeAll();
})();

