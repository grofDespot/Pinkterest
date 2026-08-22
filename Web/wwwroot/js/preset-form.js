document.querySelector('#preset-name')?.form?.addEventListener('submit', () => {
    document.querySelector('#preset-format').value = document.querySelector('#Format').value;
    document.querySelector('#preset-width').value = document.querySelector('#MaxWidth').value;
    document.querySelector('#preset-height').value = document.querySelector('#MaxHeight').value;

    const target = document.querySelector('#preset-filters');
    target.innerHTML = '';

    document.querySelectorAll('input[name="Filters"]:checked').forEach(checked => {
        const hidden = document.createElement('input');
        hidden.type = 'hidden';
        hidden.name = 'Filters';
        hidden.value = checked.value;
        target.appendChild(hidden);
    });
});
