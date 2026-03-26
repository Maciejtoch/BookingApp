// Dynamic time slot loading via AJAX
document.addEventListener('DOMContentLoaded', function () {
    const serviceSelect = document.getElementById('ServiceId');
    const slotSelect = document.getElementById('TimeSlotId');

    if (serviceSelect && slotSelect) {
        serviceSelect.addEventListener('change', function () {
            const serviceId = this.value;
            slotSelect.innerHTML = '<option value="">Ładowanie terminów...</option>';
            if (!serviceId) { slotSelect.innerHTML = '<option value="">-- Wybierz usługę --</option>'; return; }

            fetch(`/Reservations/GetTimeSlots?serviceId=${serviceId}`)
                .then(r => r.json())
                .then(data => {
                    slotSelect.innerHTML = '<option value="">-- Wybierz termin --</option>';
                    if (data.length === 0) {
                        slotSelect.innerHTML += '<option disabled>Brak dostępnych terminów</option>';
                    } else {
                        data.forEach(slot => {
                            slotSelect.innerHTML += `<option value="${slot.id}">${slot.label}</option>`;
                        });
                    }
                })
                .catch(() => { slotSelect.innerHTML = '<option>Błąd ładowania terminów</option>'; });
        });
    }
});