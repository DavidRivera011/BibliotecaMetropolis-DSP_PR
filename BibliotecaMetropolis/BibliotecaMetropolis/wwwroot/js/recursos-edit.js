// wwwroot/js/recursos-edit.js
// Script compartido para Create / Edit - maneja tags widget, modales (AJAX) y sincronización de TagsCsv.
// Diseñado para funcionar con la vista que prerenderiza:
//  - select#selectAutores (multiple)
//  - div#tagsContainer (chips prerenderizados o vacío)
//  - input#TagsCsv (hidden)
//  - tags inputs: #tagInput, #addTagBtn
//  - modales con ids: autorModal/autorForm, editorialModal/editorialForm
(function () {
    'use strict';

    /* ------------------ UTIL ------------------ */
    function escapeHtml(text) { const div = document.createElement('div'); div.textContent = text; return div.innerHTML; }
    function qs(sel) { return document.querySelector(sel); }
    function qsa(sel) { return Array.from(document.querySelectorAll(sel)); }

    /* ------------------ TAGS WIDGET ------------------ */
    const maxTags = 8;
    const tagsContainer = qs('#tagsContainer');
    const tagInput = qs('#tagInput');
    const tagsCsv = qs('#TagsCsv');
    const addTagBtn = qs('#addTagBtn');

    function updateHiddenTags() {
        if (!tagsContainer || !tagsCsv) return;
        const chips = qsa('#tagsContainer .tag-chip');
        const tags = chips.map(c => c.dataset.value);
        tagsCsv.value = tags.join(',');
    }

    function createTagChip(tag) {
        const span = document.createElement('span');
        span.className = 'tag-chip';
        span.dataset.value = tag;
        span.innerHTML = escapeHtml(tag) + ' <button type="button" class="btn-remove-tag" aria-label="Eliminar">×</button>';
        return span;
    }

    function addTag(tag) {
        if (!tagsContainer || !tag) return;
        tag = tag.trim();
        if (!tag) return;

        const existing = qsa('#tagsContainer .tag-chip').map(c => c.dataset.value.toLowerCase());
        if (existing.includes(tag.toLowerCase())) {
            if (tagInput) tagInput.value = '';
            return;
        }

        const chips = qsa('#tagsContainer .tag-chip');
        if (chips.length >= maxTags) {
            alert('Máximo ' + maxTags + ' etiquetas.');
            return;
        }

        tagsContainer.appendChild(createTagChip(tag));
        updateHiddenTags();
        if (tagInput) { tagInput.value = ''; tagInput.focus(); }
    }

    if (tagsContainer) {
        // delegación para eliminar chip
        tagsContainer.addEventListener('click', function (e) {
            const btn = e.target.closest('.btn-remove-tag');
            if (!btn) return;
            const chip = btn.closest('.tag-chip');
            if (chip) { chip.remove(); updateHiddenTags(); }
        });
    }

    if (tagInput) {
        tagInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); addTag(tagInput.value); }
        });
    }
    if (addTagBtn) addTagBtn.addEventListener('click', function () { addTag(tagInput.value); });

    // inicializar tags ya prerenderizadas en la vista (si las hay)
    (function initExistingTags() {
        if (!tagsContainer) return;
        const chips = qsa('#tagsContainer .tag-chip');
        chips.forEach(function (c) {
            if (!c.dataset.value) {
                const txt = c.textContent.trim();
                c.dataset.value = txt.replace(/×$/, '').trim();
            }
        });
        updateHiddenTags();
    })();

    /* ------------------ MODALES: Autor / Editorial (AJAX) ------------------ */
    function showAlert(container, msg, type = 'danger') {
        if (!container) return;
        container.classList.remove('d-none', 'alert-success', 'alert-danger', 'alert-warning');
        container.classList.add('alert-' + type);
        container.textContent = msg;
    }
    function clearAlert(container) {
        if (!container) return;
        container.classList.add('d-none');
        container.textContent = '';
        container.classList.remove('alert-success', 'alert-danger', 'alert-warning');
    }

    // AUTOR modal -> POST /api/Autor, añadir option al select (mantiene selección)
    (function autorModalHandler() {
        const autorForm = qs('#autorForm');
        if (!autorForm) return;

        autorForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            const nombresEl = qs('#autorNombres');
            const apellidosEl = qs('#autorApellidos');
            const alertBox = qs('#autorAlert');
            clearAlert(alertBox);
            const nombres = (nombresEl && nombresEl.value || '').trim();
            const apellidos = (apellidosEl && apellidosEl.value || '').trim();
            if (!nombres) { showAlert(alertBox, 'El nombre del autor es obligatorio.', 'warning'); return; }

            try {
                const resp = await fetch('/api/Autor', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Nombres: nombres, Apellidos: apellidos })
                });
                const data = await resp.json();
                if (!resp.ok) { showAlert(alertBox, data?.error || 'Error al guardar autor.', 'danger'); return; }

                const select = qs('#selectAutores');
                if (select) {
                    const opt = document.createElement('option');
                    opt.value = data.id;
                    opt.text = data.nombre || (nombres + (apellidos ? (' ' + apellidos) : ''));
                    opt.selected = true;
                    select.appendChild(opt);
                }

                const modalEl = qs('#autorModal');
                const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                modal.hide();

                if (nombresEl) nombresEl.value = '';
                if (apellidosEl) apellidosEl.value = '';
            } catch (err) {
                showAlert(alertBox, 'Error de conexión', 'danger');
            }
        });
    })();

    // EDITORIAL modal -> POST /api/Editorial, añadir option al selectEditorial
    (function editorialModalHandler() {
        const editorialForm = qs('#editorialForm');
        if (!editorialForm) return;

        editorialForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            const nombre = (qs('#editorialNombre')?.value || '').trim();
            const descripcion = (qs('#editorialDescripcion')?.value || '').trim();
            const alertBox = qs('#editorialAlert');
            clearAlert(alertBox);
            if (!nombre) { showAlert(alertBox, 'El nombre de la editorial es obligatorio.', 'warning'); return; }

            try {
                const resp = await fetch('/api/Editorial', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Nombre: nombre, Descripcion: descripcion })
                });
                const data = await resp.json();
                if (!resp.ok) { showAlert(alertBox, data?.error || 'Error al guardar editorial.', 'danger'); return; }

                const selectEd = qs('#selectEditorial');
                if (selectEd) {
                    const opt = document.createElement('option');
                    opt.value = data.id;
                    opt.text = data.nombre;
                    opt.selected = true;
                    selectEd.appendChild(opt);
                }

                const modalEl = qs('#editorialModal');
                const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                modal.hide();

                const nombreEl = qs('#editorialNombre');
                const descEl = qs('#editorialDescripcion');
                if (nombreEl) nombreEl.value = '';
                if (descEl) descEl.value = '';
            } catch (err) {
                showAlert(alertBox, 'Error de conexión', 'danger');
            }
        });
    })();

