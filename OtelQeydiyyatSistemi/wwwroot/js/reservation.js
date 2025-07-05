// Rezervasiya əməliyyatları üçün JavaScript funksiyaları
$(document).ready(function () {
    // Rezervasiyanı ləğv et düyməsi
    $('.btn-cancel-reservation').click(function (e) {
        e.preventDefault();
        var reservationId = $(this).data('id');
        var url = $(this).attr('href');
        
        if (confirm('Bu rezervasiyanı ləğv etmək istədiyinizə əminsiniz?')) {
            window.location.href = url;
        }
    });

    // Giriş et düyməsi
    $('.btn-check-in').click(function (e) {
        e.preventDefault();
        var reservationId = $(this).data('id');
        var url = $(this).attr('href');
        
        if (confirm('Müştərinin otağa giriş etdiyini təsdiqləmək istəyirsiniz?')) {
            window.location.href = url;
        }
    });

    // Çıxış et düyməsi
    $('.btn-check-out').click(function (e) {
        e.preventDefault();
        var reservationId = $(this).data('id');
        var url = $(this).attr('href');
        
        if (confirm('Müştərinin otağı tərk etdiyini təsdiqləmək istəyirsiniz?')) {
            window.location.href = url;
        }
    });
});
