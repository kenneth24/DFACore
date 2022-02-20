$(document).ready(function () {
    $('#loading').hide();


    $(".btnsummary").on('click', function () {

        $.ajax({
            type: "POST",
            url: `${urlBase}Home/PaymentMethod`,
            success: function (data) {
                if (data.status == 'Success') {
                    window.location.href = `${urlBase}Home/PaymentMethod`
                }
                else {
                    $('#errMsg').text(data.message);
                    $('#error').show();
                }
            },
            error: function (data) {

            }
        }).done(function (data) {
            console.log(data);
        });

    });
    
});