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
    console.log('click');
    let code = codeInput.val();
    
    if (code == null || code == '') {
        codeError.text('This field is required.');
        //loading.modal('hide');
        return;
    }
    loading.modal('show');
    $.ajax({
        type: "post",
        url: `${urlBase}Home/TrackApplication`, //"/Home/ValidateAppointment",
        data: { 'code': code },
        datatype: "json",
        cache: false,
        success: function (data) {
            if (data.status == 'Success') {
                record = data.data;
                setTimeout(function () {
                    $('#closeLoading').click();
                }, 2000);
                //stepOne.hide();
                appointmentCode.text(record.applicationCode);
                appointmentDate.text(new Date(record.scheduleDate).toLocaleDateString());
                appointmentTime.text(new Date(record.scheduleDate).toLocaleTimeString());
                processingSite.text(record.processingSite);
                processingSiteAddress.text(record.processingSiteAddress);
                documentOwner.text(`${record.firstName} ${record.middleName != null ? record.middleName : ''} ${record.lastName} ${record.suffix != null ? record.suffix : ''}`);
                representative.text(record.nameOfRepresentative != null ? record.nameOfRepresentative : '-');
                destination.text(record.countryDestination);
                //fees.text(`PhP ${record.fees}.00`);
                var docs = JSON.parse(record.apostileData);

                for (let i = 0; i < docs.length; i++) {
                    let documentText = `<p class="mb-0">(${docs[i].Quantity}) (${docs[i].Transaction}) ${docs[i].Name}</p>`;
                    documents.append(documentText);
                }

                $("#ownerContactNumber").text(record.ownerContactNumber);
                $("#coContactNumber").text(record.coContactNumber);
                $("#coEmail").text(record.coEmail);
                $("#ownerEmail").text(record.email);
                $("#totalFees").text(`PhP ${record.fees}.00`);

                $('.status-container p').removeClass('green');
                $('.status-container p').removeClass('red');
                $('.status-container p').removeClass('orange');

                if (record.receivingStatus) {
                    $("#recevingStatus").parent().addClass('green');
                    $("#recevingStatus").text(`Document/s was received by the Authentication Division`);
                }
                if (record.assessmentStatus) {
                    $("#assessmentStatus").parent().addClass('green');
                    $("#assessmentStatus").text(`Document/s was received by the Processing Section`);
                }
                if (record.encodingStatus) {
                    $("#encodingStatus").parent().addClass('green');
                    $("#encodingStatus").text(`Document/s was encoded`);
                }
                if (record.printingStatus) {
                    $("#printingStatus").parent().addClass('green');
                    $("#printingStatus").text(`Document/s was printed`);
                }
                if (record.releasingStatus) {
                    $("#releasingStatus").parent().addClass('green');
                    $("#releasingStatus").text(`Document/s was received by the Authentication Officer`);
                }



                //$("#recevingStatus").text(`Receiving Status: ${record.receivingStatus}`);
                //$("#assessmentStatus").text(`Assessment Status: ${record.assessmentStatus}`);
                //$("#encodingStatus").text(`Encoding Status: ${record.encodingStatus}`);
                //$("#printingStatus").text(`Printing Status: ${record.printingStatus}`);
                //$("#releasingStatus").text(`Releasing Status: ${record.releasingStatus}`);

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
    window.location.href = `${urlBase}Home/LoginOptions`;
});

cancelAppointment.on('click', function () {
    let isChecked = condition.is(':checked'),
        code = codeInput.val();

    if (codeInput.val() == '') {
        conditionError.text('This field is required.');
        loading.hide();
        return;
    }

    $.ajax({
        type: "post",
        url: `${urlBase}Home/TrackApplication`,
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
    window.location.href = `${urlBase}Home/Cancellation`;
});

exit.on('click', function () {
    let baseUrl = window.location.origin;
    window.location = `${urlBase}/Account/LogOff`;
});

resendEmail.on('click', function () {
    $.ajax({
        type: "post",
        url: `${urlBase}Home/ResendCancellationEmail`,//"/Home/ResendCancellationEmail",
        data: record,
        datatype: "json",
        cache: false,
        success: function (data) {
            //loading.show();
            $('#DidntReceivedMail').attr('style', 'display: none!important');
            $('#AlreadySent').attr('style', 'display: block!important');
            stepTwo.hide();
            success.show();
        },
        error: function (data) {
            loading.hide();
        }
    });
});