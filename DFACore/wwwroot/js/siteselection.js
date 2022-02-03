

$(document).ready(function () {
    $('#loading').hide();
    let checkmark = $('.checkmark'),
        documentStatusRadio = $('input[type=radio][name=DocumentStatus]');


    documentStatusRadio.change(function () {
        let $this = $(this),
            value = $this.val(),
            philippinesOptions = $('option.ph');
        console.log(value);

        if (value == 'Abroad') {
            philippinesOptions.hide();
        }
        else {
            philippinesOptions.show();
        }
    });

});

