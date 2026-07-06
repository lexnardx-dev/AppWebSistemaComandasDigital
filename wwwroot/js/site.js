window.crearPaginacionTabla = function ({
    tableId,
    paginationId,
    pageSize,
    emptyId,
    rowSelector = 'tbody tr[data-filter-row]'
}) {
    const table = document.getElementById(tableId);
    const pagination = document.getElementById(paginationId);
    const empty = emptyId ? document.getElementById(emptyId) : null;

    if (!table || !pagination || !pageSize) {
        return { render: () => { } };
    }

    const rows = Array.from(table.querySelectorAll(rowSelector));
    let currentPage = 1;

    pagination.classList.add('table-pagination');
    pagination.innerHTML = `
        <button type="button" class="btn btn-sm btn-outline-secondary" data-page-prev aria-label="Pagina anterior">
            <i class="bi bi-chevron-left"></i>
        </button>
        <span data-page-indicator></span>
        <button type="button" class="btn btn-sm btn-outline-secondary" data-page-next aria-label="Pagina siguiente">
            <i class="bi bi-chevron-right"></i>
        </button>
    `;

    const prev = pagination.querySelector('[data-page-prev]');
    const next = pagination.querySelector('[data-page-next]');
    const indicator = pagination.querySelector('[data-page-indicator]');

    const getVisibleRows = () => rows.filter(row => !row.classList.contains('data-filter-hidden'));

    const render = (reset = false) => {
        if (reset) currentPage = 1;

        const visibleRows = getVisibleRows();
        const totalPages = Math.max(1, Math.ceil(visibleRows.length / pageSize));
        currentPage = Math.min(currentPage, totalPages);

        const start = (currentPage - 1) * pageSize;
        const end = start + pageSize;

        rows.forEach(row => row.classList.add('data-pagination-hidden'));
        visibleRows.forEach((row, index) => {
            row.classList.toggle('data-pagination-hidden', index < start || index >= end);
        });

        if (empty) {
            empty.style.display = visibleRows.length === 0 ? 'block' : 'none';
        }

        pagination.classList.toggle('d-none', visibleRows.length <= pageSize);
        indicator.textContent = `Pagina ${currentPage} de ${totalPages}`;
        prev.disabled = currentPage === 1;
        next.disabled = currentPage === totalPages;
    };

    prev.addEventListener('click', () => {
        if (currentPage > 1) {
            currentPage--;
            render();
        }
    });

    next.addEventListener('click', () => {
        const totalPages = Math.max(1, Math.ceil(getVisibleRows().length / pageSize));
        if (currentPage < totalPages) {
            currentPage++;
            render();
        }
    });

    render();

    return { render };
};
