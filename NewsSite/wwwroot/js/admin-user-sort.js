(() => {
    const toNumber = (value) => Number(value ?? 0);
    const toText = (value) => String(value ?? '').toLowerCase();

    document.querySelectorAll('.admin-sortable-table').forEach(table => {
        let sortBy = '';
        let sortDirection = 'asc';

        table.querySelectorAll('.admin-sort').forEach(button => {
            button.addEventListener('click', () => {
                const selectedSort = button.dataset.sort;
                sortDirection = sortBy === selectedSort && sortDirection === 'asc' ? 'desc' : 'asc';
                sortBy = selectedSort;

                const tbody = table.querySelector('tbody');
                if (!tbody) {
                    return;
                }

                const rows = Array.from(tbody.querySelectorAll('tr')).filter(row => row.dataset.name);
                rows.sort((a, b) => {
                    const av = a.dataset[sortBy] ?? '';
                    const bv = b.dataset[sortBy] ?? '';
                    const isNumeric = ['dob', 'status'].includes(sortBy);

                    if (isNumeric) {
                        return sortDirection === 'asc' ? toNumber(av) - toNumber(bv) : toNumber(bv) - toNumber(av);
                    }

                    return sortDirection === 'asc'
                        ? toText(av).localeCompare(toText(bv))
                        : toText(bv).localeCompare(toText(av));
                });

                rows.forEach(row => tbody.appendChild(row));
            });
        });
    });
})();
