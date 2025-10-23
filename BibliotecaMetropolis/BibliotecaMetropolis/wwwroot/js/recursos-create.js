// recursos-create.js
// Manejo de tags, autores, modales y validación client-side para Create.cshtml

(function () {
    /* ------------------ TAGS ------------------ */
    const maxTags = 8;
    const tagsContainer = document.getElementById('tagsContainer');
    const tagInput = document.getElementById('tagInput');
    const tagsCsv = document.getElementById('TagsCsv');
    const addTagBtn = document.getElementById('addTagBtn');

    function escapeHtml(text) { const div = document.createElement('div'); div.textContent = text; return div.innerHTML; }

    function updateHiddenTags() {
        const chips = tagsContainer ? tagsContainer.querySelectorAll('.tag-chip') : [];
        const tags = Array.from(chips).map(c => c.dataset.value);
        if (tagsCsv) tagsCsv.value = tags.join(',');
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

        const existing = Array.from(tagsContainer.querySelectorAll('.tag-chip')).map(c => c.dataset.value.toLowerCase());
        if (existing.includes(tag.toLowerCase())) {
            tagInput.value = '';
            return;
        }

        const chips = tagsContainer.querySelectorAll('.tag-chip');
        if (chips.length >= maxTags) {
            alert('Máximo ' + maxTags + ' etiquetas.');
            return;
        }

        const chip = createTagChip(tag);
        tagsContainer.appendChild(chip);
        updateHiddenTags();
        tagInput.value = '';
        tagInput.focus();
    }

    if (tagsContainer) {
        tagsContainer.addEventListener('click', function (e) {
            if (e.target && (e.target.matches('.btn-remove-tag') || e.target.closest('.btn-remove-tag'))) {
                const btn = e.target.closest('.btn-remove-tag');
                const chip = btn.closest('.tag-chip');
                if (chip) { chip.remove(); updateHiddenTags(); }
            }
        });
    }

    if (tagInput) {
        tagInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); addTag(tagInput.value); }
        });
    }
    if (addTagBtn) addTagBtn.addEventListener('click', function () { addTag(tagInput.value); });
    updateHiddenTags();

    /* ------------------ AUTORES ------------------ */
    const selectAutores = document.getElementById('selectAutores');
    const btnAddAutor = document.getElementById('btnAddAutor');
    const selectedContainer = document.getElementById('selectedAuthorsContainer');
    const form = document.getElementById('recursoForm');
    const authorsValidation = document.getElementById('authorsValidation');

    let selectedAuthors = [];

    function renderSelectedAuthors() {
        if (!selectedContainer) return;
        selectedContainer.innerHTML = '';
        // eliminar inputs antiguos
        const oldHidden = document.querySelectorAll('input[name="SelectedAuthorIds"]');
        oldHidden.forEach(h => h.remove());

        selectedAuthors.forEach(sa => {
            const chip = document.createElement('span');
            chip.className = 'author-chip';
            chip.dataset.id = sa.id;
            chip.innerHTML = `<strong>${escapeHtml(sa.name)}</strong> <button type="button" class="btn-remove-author" aria-label="Eliminar">×</button>`;
            selectedContainer.appendChild(chip);

            const hidden = document.createElement('input');
            hidden.type = 'hidden';
            hidden.name = 'SelectedAuthorIds';
            hidden.value = sa.id;
            selectedContainer.appendChild(hidden);
        });
    }

    function addSelectedAuthor() {
        if (!selectAutores) return;
        if (authorsValidation) authorsValidation.classList.add('d-none');

        const val = selectAutores.value;
        if (!val) return;

        if (selectedAuthors.some(s => String(s.id) === String(val))) {
            selectAutores.value = '';
            return;
        }

        const text = selectAutores.options[selectAutores.selectedIndex]?.text || 'Autor';
        selectedAuthors.push({ id: parseInt(val, 10), name: text });
        renderSelectedAuthors();
        selectAutores.value = '';
    }

    if (btnAddAutor) btnAddAutor.addEventListener('click', addSelectedAuthor);

    if (selectedContainer) {
        selectedContainer.addEventListener('click', function (e) {
            if (e.target && (e.target.matches('.btn-remove-author') || e.target.closest('.btn-remove-author'))) {
                const btn = e.target.closest('.btn-remove-author');
                const chip = btn.closest('.author-chip');
                if (!chip) return;
                const id = parseInt(chip.dataset.id, 10);
                selectedAuthors = selectedAuthors.filter(s => s.id !== id);
                renderSelectedAuthors();
            }
        });
    }

    if (form) {
        form.addEventListener('submit', function (e) {
            if (selectedAuthors.length === 0) {
                e.preventDefault();
                if (authorsValidation) {
                    authorsValidation.textContent = 'Debes añadir al menos un autor.';
                    authorsValidation.classList.remove('d-none');
                }
                if (selectedContainer) selectedContainer.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        });
    }

    /* ------------------ MODALES (AJAX) ------------------ */
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

    // Autor modal: POST /api/Autor
    const autorForm = document.getElementById('autorForm');
    if (autorForm) {
        autorForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            const nombres = document.getElementById('autorNombres').value.trim();
            const apellidos = document.getElementById('autorApellidos').value.trim();
            const alertBox = document.getElementById('autorAlert');

            clearAlert(alertBox);
            if (!nombres) { showAlert(alertBox, 'El nombre del autor es obligatorio.', 'warning'); return; }

            try {
                const resp = await fetch('/api/Autor', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Nombres: nombres, Apellidos: apellidos })
                });
                const data = await resp.json();
                if (!resp.ok) { showAlert(alertBox, data?.error || 'Error al guardar autor.', 'danger'); return; }

                if (selectAutores) {
                    const opt = document.createElement('option');
                    opt.value = data.id;
                    opt.text = data.nombre || (nombres + (apellidos ? (' ' + apellidos) : ''));
                    selectAutores.appendChild(opt);
                }

                // añadir automáticamente a selectedAuthors
                addAuthorToSelected(data.id, data.nombre || (nombres + (apellidos ? (' ' + apellidos) : '')));

                // cerrar modal
                const modalEl = document.getElementById('autorModal');
                const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                modal.hide();

                document.getElementById('autorNombres').value = '';
                document.getElementById('autorApellidos').value = '';
            } catch (err) {
                showAlert(alertBox, 'Error de conexión', 'danger');
            }
        });
    }

    function addAuthorToSelected(id, name) {
        if (selectedAuthors.some(s => String(s.id) === String(id))) {
            if (selectAutores) selectAutores.value = '';
            return;
        }
        selectedAuthors.push({ id: parseInt(id, 10), name: name });
        renderSelectedAuthors();
        if (selectAutores) selectAutores.value = '';
    }

    // Editorial modal: POST /api/Editorial
    const editorialForm = document.getElementById('editorialForm');
    if (editorialForm) {
        editorialForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            const nombre = document.getElementById('editorialNombre').value.trim();
            const descripcion = document.getElementById('editorialDescripcion').value.trim();
            const alertBox = document.getElementById('editorialAlert');
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

                const selectEd = document.getElementById('selectEditorial');
                if (selectEd) {
                    const opt = document.createElement('option');
                    opt.value = data.id;
                    opt.text = data.nombre;
                    opt.selected = true;
                    selectEd.appendChild(opt);
                }

                // cerrar modal y limpiar
                const modalEl = document.getElementById('editorialModal');
                const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                modal.hide();
                document.getElementById('editorialNombre').value = '';
                document.getElementById('editorialDescripcion').value = '';
            } catch (err) {
                showAlert(alertBox, 'Error de conexión', 'danger');
            }
        });
    }
})();
