(() => {
    const toNumber = (value) => Number(value ?? 0);
    const toText = (value) => String(value ?? '').toLowerCase();

    const applyWriterFilters = () => {
        const search = toText(document.getElementById('writerSearch')?.value);
        const status = toText(document.getElementById('writerStatusFilter')?.value ?? 'all');

        document.querySelectorAll('.writer-sortable-table tbody tr').forEach(row => {
            const title = toText(row.dataset.title);
            const rowStatus = toText(row.dataset.status);
            const matchSearch = !search || title.includes(search);
            const matchStatus = status === 'all' || rowStatus === status;
            row.style.display = matchSearch && matchStatus ? '' : 'none';
        });
    };

    document.querySelectorAll('.writer-sortable-table').forEach(table => {
        let sortBy = '';
        let sortDirection = 'asc';

        table.querySelectorAll('.writer-sort').forEach(button => {
            button.addEventListener('click', () => {
                const selectedSort = button.dataset.sort;
                sortDirection = sortBy === selectedSort && sortDirection === 'asc' ? 'desc' : 'asc';
                sortBy = selectedSort;

                const tbody = table.querySelector('tbody');
                if (!tbody) return;

                const rows = Array.from(tbody.querySelectorAll('tr')).filter(row => row.dataset.title);
                rows.sort((a, b) => {
                    const av = a.dataset[sortBy] ?? '';
                    const bv = b.dataset[sortBy] ?? '';
                    const isNumeric = ['views', 'likes', 'created'].includes(sortBy);
                    if (isNumeric) return sortDirection === 'asc' ? toNumber(av) - toNumber(bv) : toNumber(bv) - toNumber(av);
                    return sortDirection === 'asc' ? toText(av).localeCompare(toText(bv)) : toText(bv).localeCompare(toText(av));
                });
                rows.forEach(row => tbody.appendChild(row));
            });
        });
    });

    document.getElementById('writerSearch')?.addEventListener('input', applyWriterFilters);
    document.getElementById('writerStatusFilter')?.addEventListener('change', applyWriterFilters);
    document.getElementById('writerClearFilter')?.addEventListener('click', () => {
        const s = document.getElementById('writerSearch'); if (s) s.value = '';
        const st = document.getElementById('writerStatusFilter'); if (st) st.value = 'all';
        applyWriterFilters();
    });

    applyWriterFilters();
})();
