let codeInput = $('#code');
let searchButton = $('#searchButton');
let backButton = $('#backButton');
let returnModal = $('#returnModal');
let errorMessage = $('#errorMessage');
let message = $('#message');
let date = $('#date');
let exitButton = $('#exitButton');
let codeError = $('#codeError');
let loading = $('#loading');
let stepOne = $('#stepOne');
let stepTwo = $('#stepTwo');
let appointmentCode = $("#appointmentCode");
let appointmentDate = $('#appointmentDate');
let appointmentTime = $('#appointmentTime');
let processingSite = $('#processingSite');
let processingSiteAddress = $('#processingSiteAddress')
let documentOwner = $('#documentOwner');
let representative = $('#representative');
let destination = $('#destination');
let documents = $('#documents');
let fees = $('#fees');
let stepTwoBackButton = $('#stepTwoBackButton');
let cancelAppointment = $('#cancelAppointment');
let condition = $('#condition');
let conditionError = $('#conditionError');
let success = $('#success');
let resendEmail = $('#resendEmail');
let cancelAnother = $('#cancelAnother');
let exit = $('#exit');
let record;

stepTwo.hide();

searchButton.on('click', function () {
    let code = codeInput.val();
    loading.modal('show');

    if (code == null || code == '') {
        codeError.text('This field is required.');
        loading.modal('show');
        return;
    }


    $.ajax({
        type: "post",
        url: "/Home/ValidateAppointment",
        data: { 'code': code },
        datatype: "json",
        cache: false,
        success: function (data) {
            if (data.status == 'Success') {
                record = data.data;
                setTimeout(function () {
                    $('#closeLoading').click();
                }, 2000);
                stepOne.hide();
                appointmentCode.text(record.applicationCode);
                appointmentDate.text(new Date(record.scheduleDate).toLocaleDateString());
                appointmentTime.text(new Date(record.scheduleDate).toLocaleTimeString());
                processingSite.text(record.processingSite);
                processingSiteAddress.text(record.processingSiteAddress);
                documentOwner.text(`${record.firstName} ${record.middleName != null ? record.middleName : ''} ${record.lastName} ${record.suffix != null ? record.suffix : ''}`);
                representative.text(record.nameOfRepresentative != null ? record.nameOfRepresentative : '-');
                destination.text(record.countryDestination);
                fees.text(`PhP ${record.fees}.00`);
                var docs = JSON.parse(record.apostileData);

                for (let i = 0; i < docs.length; i++ )
                {
                    let documentText = `<p class="mb-0">(${docs[i].Quantity}) (${docs[i].Transaction}) ${docs[i].Name}</p>`;
                    documents.append(documentText);
                }

                stepTwo.show();
            }
            else {
                setTimeout(function () {
                    errorMessage.text(data.errorMessage);
                    message.text(data.message);
                    date.text(data.date);
                    $('#closeLoading').click();
                    returnModal.modal('show');
                }, 2000);
            }
            loading.modal('hide');
        },
        error: function (data) {
            loading.hide();
        }
    });
});

stepTwoBackButton.on('click', function () {
    stepTwo.hide();
    stepOne.show();
});

backButton.on('click', function () {
    window.location.href = '/Home/LoginOptions';
});

cancelAppointment.on('click', function () {
    let isChecked = condition.is(':checked'),
        code = codeInput.val();

    if (!isChecked) {
        conditionError.text('This field is required.');
        return;
    }


    $.ajax({
        type: "post",
        url: "/Home/CancelAppointment",
        data: { 'code': code },
        datatype: "json",
        cache: false,
        success: function (data) {
            stepTwo.hide();
            success.show();
        },
        error: function (data) {
            loading.hide();
        }
    });
});

cancelAnother.on('click', function () {
    window.location.href = '/Home/Cancellation';
});

exit.on('click', function () {
    let baseUrl = window.location.origin;
    window.location = `${baseUrl}/Account/LogOff`;
});

resendEmail.on('click', function () {
    $.ajax({
        type: "post",
        url: "/Home/ResendCancellationEmail",
        data: record,
        datatype: "json",
        cache: false,
        success: function (data) {
            stepTwo.hide();
            success.show();
        },
        error: function (data) {
            loading.hide();
        }
    });
});