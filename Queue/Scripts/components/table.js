(function () {
    'use strict';
    var exportLibraries = {};

    function loadScript(url) {
        return new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.src = url;
            script.onload = resolve;
            script.onerror = function () { script.remove(); reject(new Error('No fue posible cargar la librería de exportación.')); };
            document.head.appendChild(script);
        });
    }

    function loadExportLibrary(kind) {
        if (kind === 'excel' && window.XLSX) return Promise.resolve();
        if (kind === 'pdf' && window.jspdf && window.jspdf.jsPDF && window.jspdf.jsPDF.API.autoTable) return Promise.resolve();
        if (!exportLibraries[kind]) {
            exportLibraries[kind] = (async function () {
                if (kind === 'excel') {
                    await loadScript('https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js');
                    if (!window.XLSX) throw new Error('La librería de Excel no quedó disponible.');
                    return;
                }
                await loadScript('https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js');
                await loadScript('https://cdnjs.cloudflare.com/ajax/libs/jspdf-autotable/3.5.31/jspdf.plugin.autotable.min.js');
                if (!window.jspdf || !window.jspdf.jsPDF || !window.jspdf.jsPDF.API.autoTable) {
                    throw new Error('La librería para crear tablas PDF no quedó disponible.');
                }
            })().catch(function (error) {
                delete exportLibraries[kind];
                throw error;
            });
        }
        return exportLibraries[kind];
    }

    function normalized(value) {
        return value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('es');
    }

    function localDateStamp() {
        var date = new Date();
        return date.getFullYear() + '-' + String(date.getMonth() + 1).padStart(2, '0') + '-' + String(date.getDate()).padStart(2, '0');
    }

    function fileName(title, extension) {
        var safeTitle = normalized(title).replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') || 'reporte';
        return safeTitle + '-' + localDateStamp() + '.' + extension;
    }

    function exportExcel(title, description, context, headers, data, count) {
        var XLSX = window.XLSX;
        var generated = new Date().toLocaleString('es-CO');
        var rows = [[title], [description || 'Reporte TEC Tracker']];
        if (context) rows.push([context]);
        rows.push(['Generado: ' + generated + ' | Registros exportados: ' + count], [], headers);
        var headerRow = rows.length - 1;
        rows = rows.concat(data);
        var sheet = XLSX.utils.aoa_to_sheet(rows);
        var lastColumn = headers.length - 1;
        sheet['!merges'] = rows.slice(0, headerRow).map(function (_, row) {
            return { s: { r: row, c: 0 }, e: { r: row, c: lastColumn } };
        });
        sheet['!autofilter'] = { ref: XLSX.utils.encode_range({ s: { r: headerRow, c: 0 }, e: { r: data.length + headerRow, c: lastColumn } }) };
        sheet['!rows'] = rows.map(function (_, row) {
            if (row === 0) return { hpt: 26 };
            if (row === 1) return { hpt: 32 };
            if (row === headerRow) return { hpt: 22 };
            return { hpt: 19 };
        });
        sheet['!cols'] = headers.map(function (header, column) {
            var maxLength = String(header).length;
            data.forEach(function (row) { maxLength = Math.max(maxLength, String(row[column] || '').length); });
            return { wch: Math.min(Math.max(maxLength + 2, 12), 42) };
        });
        var book = XLSX.utils.book_new();
        book.Props = { Title: title, Subject: description || 'Reporte TEC Tracker', Author: 'TEC Tracker', CreatedDate: new Date() };
        XLSX.utils.book_append_sheet(book, sheet, 'Reporte');
        XLSX.writeFile(book, fileName(title, 'xlsx'), { compression: true });
    }

    function exportPdf(title, description, context, headers, data, count) {
        var Pdf = window.jspdf.jsPDF;
        var pdf = new Pdf({ orientation: headers.length > 5 ? 'landscape' : 'portrait', unit: 'mm', format: 'a4' });
        var pageWidth = pdf.internal.pageSize.getWidth();
        var generated = new Date().toLocaleString('es-CO');
        var headerHeight = context ? 38 : 30;
        var tableStartY = context ? 44 : 36;
        pdf.setProperties({ title: title, subject: description || 'Reporte TEC Tracker', creator: 'TEC Tracker' });
        pdf.autoTable({
            head: [headers],
            body: data,
            startY: tableStartY,
            margin: { top: tableStartY, right: 12, bottom: 16, left: 12 },
            theme: 'striped',
            styles: { font: 'helvetica', fontSize: 8, cellPadding: 2.5, overflow: 'linebreak', valign: 'middle', textColor: [51, 65, 85], lineColor: [226, 232, 240], lineWidth: 0.15 },
            headStyles: { fillColor: [109, 95, 185], textColor: [255, 255, 255], fontStyle: 'bold' },
            alternateRowStyles: { fillColor: [248, 247, 252] },
            didDrawPage: function () {
                pdf.setFillColor(109, 95, 185);
                pdf.rect(0, 0, pageWidth, headerHeight, 'F');
                pdf.setTextColor(255, 255, 255);
                pdf.setFont('helvetica', 'bold');
                pdf.setFontSize(13);
                pdf.text(title, 12, 10);
                pdf.setFont('helvetica', 'normal');
                pdf.setFontSize(8);
                var details = pdf.splitTextToSize(description || 'Reporte TEC Tracker', pageWidth - 24);
                pdf.text(details.slice(0, 1), 12, 16);
                if (context) {
                    pdf.setTextColor(255, 255, 255);
                    pdf.text(pdf.splitTextToSize(context, pageWidth - 24).slice(0, 2), 12, 23);
                }
                pdf.setTextColor(226, 232, 240);
                pdf.text('Generado: ' + generated + ' | Registros exportados: ' + count, 12, context ? 33 : 25);
            }
        });
        var pageCount = pdf.internal.getNumberOfPages();
        for (var page = 1; page <= pageCount; page++) {
            pdf.setPage(page);
            pdf.setFont('helvetica', 'normal');
            pdf.setFontSize(8);
            pdf.setTextColor(100, 116, 139);
            pdf.text('TEC Tracker', 12, pdf.internal.pageSize.getHeight() - 7);
            pdf.text('Página ' + page + ' de ' + pageCount, pageWidth - 12, pdf.internal.pageSize.getHeight() - 7, { align: 'right' });
        }
        pdf.save(fileName(title, 'pdf'));
    }

    function initialize(root) {
        if (root.dataset.initialized) return;
        root.dataset.initialized = 'true';
        var table = root.querySelector('table');
        if (!table) return;
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
                var columnNames = columns.map(function (index) { return headers[index].textContent.trim(); });
                var values = selected.map(function (row) { return columns.map(function (index) { return row.children[index].textContent.trim(); }); });
                var title = root.dataset.exportTitle || table.querySelector('caption').textContent.trim() || 'Reporte TEC Tracker';
                var descriptionElement = root.querySelector('header p');
                var description = root.dataset.exportSubtitle || (descriptionElement ? descriptionElement.textContent.trim() : 'Resultados de la consulta');
                var context = root.dataset.exportContext || '';
                button.disabled = true;
                button.setAttribute('aria-busy', 'true');
                status.textContent = 'Preparando archivo…';
                try {
                    await loadExportLibrary(button.dataset.export);
                    if (button.dataset.export === 'excel') exportExcel(title, description, context, columnNames, values, values.length);
                    else exportPdf(title, description, context, columnNames, values, values.length);
                    status.textContent = 'Archivo descargado correctamente.';
                } catch (error) {
                    status.textContent = 'No se pudo generar el archivo. Comprueba tu conexión e inténtalo nuevamente.';
                } finally {
                    button.disabled = false;
                    button.removeAttribute('aria-busy');
                }
            });
        });
        render();
    }
    function initializeAll() { document.querySelectorAll('[data-component-table]').forEach(initialize); }
    window.TecTables = { initialize: initializeAll };
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', initializeAll);
    else initializeAll();
})();
