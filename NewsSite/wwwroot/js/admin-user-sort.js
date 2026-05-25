(() => {
    const staffRoles = ['admin', 'editor', 'writer'];
    const toNumber = (value) => Number(value ?? 0);
    const toText = (value) => String(value ?? '').toLowerCase();

    const applyAdminFilters = () => {
        const search = toText(document.getElementById('adminSearch')?.value);

        document.querySelectorAll('.admin-sortable-table tbody tr').forEach(row => {
            const name = toText(row.dataset.name);
            const email = toText(row.dataset.email);
            const matchSearch = !search || name.includes(search) || email.includes(search);
            row.style.display = matchSearch ? '' : 'none';
        });
    };

    const sortTables = () => {
        document.querySelectorAll('.admin-sortable-table').forEach(table => {
            let sortBy = '';
            let sortDirection = 'asc';
            table.querySelectorAll('.admin-sort').forEach(button => {
                button.addEventListener('click', () => {
                    const selectedSort = button.dataset.sort;
                    sortDirection = sortBy === selectedSort && sortDirection === 'asc' ? 'desc' : 'asc';
                    sortBy = selectedSort;
                    const tbody = table.querySelector('tbody');
                    if (!tbody) return;
                    const rows = Array.from(tbody.querySelectorAll('tr')).filter(row => row.dataset.name);
                    rows.sort((a, b) => {
                        const av = a.dataset[sortBy] ?? '';
                        const bv = b.dataset[sortBy] ?? '';
                        const isNumeric = ['dob'].includes(sortBy);
                        if (isNumeric) return sortDirection === 'asc' ? toNumber(av) - toNumber(bv) : toNumber(bv) - toNumber(av);
                        return sortDirection === 'asc' ? toText(av).localeCompare(toText(bv)) : toText(bv).localeCompare(toText(av));
                    });
                    rows.forEach(row => tbody.appendChild(row));
                });
            });
        });
    };

    const getTargetTableId = (row, isDeleted) => {
        const role = toText(row.dataset.role);
        const isStaff = staffRoles.includes(role);
        if (isStaff && isDeleted) return 'staff-inactive';
        if (isStaff && !isDeleted) return 'staff-active';
        if (!isStaff && isDeleted) return 'regular-inactive';
        return 'regular-active';
    };

    const wireStateForms = () => {
        document.addEventListener('submit', async (event) => {
            const form = event.target;
            if (!(form instanceof HTMLFormElement) || !form.classList.contains('js-user-state-form')) return;
            event.preventDefault();
            if (form.dataset.userAction === 'softdelete' && !window.confirm('Inaktivera?')) return;
            const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const data = new FormData(form);
            const response = await fetch(form.action, { method: 'POST', headers: token ? { RequestVerificationToken: token, 'X-Requested-With': 'XMLHttpRequest' } : { 'X-Requested-With': 'XMLHttpRequest' }, body: data });
            if (!response.ok) return;
            const row = form.closest('tr'); if (!row) return;
            const isDeleted = form.dataset.userAction === 'softdelete';
            row.dataset.status = isDeleted ? '1' : '0';
            row.className = isDeleted ? 'table-light text-muted' : '';
            const actionCell = row.lastElementChild;
            if (actionCell) {
                actionCell.innerHTML = isDeleted
                    ? `<form action="/Admin/Restore" method="post" class="d-inline js-user-state-form" data-user-action="restore"><input name="__RequestVerificationToken" type="hidden" value="${token ?? ''}" /><input type="hidden" name="userId" value="${row.dataset.userId}" /><button type="submit" class="btn btn-sm btn-success">Återställ</button></form>`
                    : `<form action="/Admin/SoftDelete" method="post" class="d-inline js-user-state-form" data-user-action="softdelete"><input name="__RequestVerificationToken" type="hidden" value="${token ?? ''}" /><input type="hidden" name="userId" value="${row.dataset.userId}" /><button type="submit" class="btn btn-sm btn-outline-danger">Inaktivera</button></form>`;
            }
            const targetBody = document.querySelector(`table[data-table-id="${getTargetTableId(row, isDeleted)}"] tbody`);
            if (targetBody) targetBody.appendChild(row);
            applyAdminFilters();
        });
    };

    document.getElementById('adminSearch')?.addEventListener('input', applyAdminFilters);
    document.getElementById('adminClearFilter')?.addEventListener('click', () => {
        const search = document.getElementById('adminSearch'); if (search) search.value = '';
        applyAdminFilters();
    });

    sortTables();
    wireStateForms();
    applyAdminFilters();
})();
