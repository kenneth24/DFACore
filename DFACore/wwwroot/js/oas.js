

let siteAndScheduleSelection = $('#step-select-processing-site');
let applicationTypeSelection = $('#applicationTypeSelection');
let siteAndScheduleButton = $('#stepTwoNextBtn');
let ownerButton = $('#ownerButton');
let authorizedButton = $('#authorizedButton');
let backToStepTwoButton = $('#backToStepTwoButton');
let stepTwo = $('#step-two');
let ownerDocument = $('#step-one');
let authorizeDocument = $('#step-one-authorized');
let stepFourBackButton = $('#stepOneBackBtn');
let docTypeSelect = $('#docTypeSelect');
let documentTypeInfo = $('#documentTypeInfo');
let documentInfoText = $('#documentInfoText');
let documentQuantity = $('#documentQuantity');
let regularQuantityElement = $('#documentQuantity .regular');
let expediteQuantityElement = $('#documentQuantity .expedite');
let documentTypeSample = $('#documentTypeSample');
let selectDocumentsBtn = $('#selectDocumentsBtnModal');
let documentsTable = $("#documentsTable");
let documentsTableBody = $("#documentsTable tbody > tr");
let backToStepThreeBtn = $('#backToStepThreeBtn');
let backToStepThreeAuthBtn = $('#backToStepThreeAuthBtn');
let addDocumentOwnerBtn = $('#addDocumentOwnerBtn');
let goToStepFiveButton = $('#stepOneNextBtn');
let goToStepFiveButtonAuth = $('#stepOneNextBtnAuth');
let ownerContainer = $('#ownerContainer');
let authContainer = $('#authContainer');
let authNameLbl = $('#authContainer .LblNameOfApplicant');
let authContactNumber = $('#authContainer .LblContactNumber');
let stepFiveBackBtn = $('#stepThreeBackBtn');
let sendPdfToEmail = $('#sendPdfToEmail');
let schedAnother = $('#schedAnother');
let exit = $('#exit');
let resendEmail = $('#resendEmail');
let token = '';
let loading = $('#loading');
let termsAndConditions = $('#terms-and-conditions-modal');
let agreeterms = $('#terms-and-conditions-agree');
let hasTermsAndConditions = false;
let hasExpedite = false;

let ownerModel = {
    FirstName: '',
    MiddleName: '',
    LastName: '',
    Suffix: '',
    DateOfBirth: '',
    ContactNumber: '',
    CountryDestination: '',
    ApostileData: '',
    ProcessingSite: '',
    ProcessingSiteAddress: '',
    ApplicationCode: '',
    Fees: 0,
    ScheduleDate: ''
};

let record = {
    ScheduleDate: '',
    ApplicantCount: 0,
    Token: '',
    Records: [],
    Record: ''
};

let docOwner = 0;
let documentObject = [];
let code = '';

window['documentObject' + 1] = [];
window['documentObject' + 2] = [];
window['documentObject' + 3] = [];
window['documentObject' + 4] = [];
window['documentObject' + 5] = [];
window['documentObject' + 6] = [];
window['documentObject' + 7] = [];
window['documentObject' + 8] = [];
window['documentObject' + 9] = [];
window['documentObject' + 10] = [];


let documents = getDocumentsType();
let prices = getPrices();
// 0 for owner (default), 
// 1 for authorized person
let applicantType = 0;

applicationTypeSelection.hide();
ownerDocument.hide();
authorizeDocument.hide();
console.log($('#appointmentSchedule').val());

siteAndScheduleButton.on('click', function () {
    if ($('#appointmentSchedule').val() == '') {
        return;
    }
    
    loading.show();
    siteAndScheduleSelection.hide();
    applicationTypeSelection.show();
    loading.hide();
});

ownerButton.on('click', function () {
    loading.show();
    applicantType = 0;
    applicationTypeSelection.hide();
    ownerDocument.show();
    $('.transactionQuantity.regular').val("0");
    $("#ChckIfhasExpedite").text("(N/A for this Consular Office)");
    if (hasExpedite) {
        $("#ChckIfhasExpedite").text("");

        $('.transactionQuantity.expedite').val("0");
        console.log('hasExpedite');
        $('.transactionQuantity.expedite').attr('disabled', false);
        $('.transactionQuantity.regular').attr('disabled', false);
    }
    
    loading.hide();
});


authorizedButton.on('click', function () {
    loading.show();
    applicantType = 1;
    applicationTypeSelection.hide();
    console.log('pass');
    authorizeDocument.show();
    addDocumentOwnerBtn.click();
    loading.hide();
});

backToStepTwoButton.on('click', function () {
    loading.show();
    applicationTypeSelection.hide();
    stepTwo.show();
    siteAndScheduleSelection.show();
    loading.hide();
});

stepFourBackButton.on('click', function () {
    loading.show();
    ownerDocument.hide();
    applicationTypeSelection.show();
    $('#step-select-processing-site').hide();
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
    loading.hide();
});

docTypeSelect.on('change', function () {
    loading.show();
    let $this = $(this),
        selected = $this.find(":selected"),
        id = selected.attr('id'),
        value = selected.attr('value'),
        selectedData = documents.filter(x => x.id == id)[0],
        regularQuantity = selectedData.quantities.filter(x => x.name == 'Regular')[0],
        expediteQuantity = selectedData.quantities.filter(x => x.name == 'Expedite')[0],
        baseUrl = window.location.origin;

    documentTypeSample.children().remove();
    if (selectedData != null) {
        // change displayed document info
        if (selectedData.description == '') {
            documentTypeInfo.hide();
        } else {
            documentTypeInfo.show();
            documentInfoText.text(selectedData.description);
        }
        
        regularQuantityElement.attr('id', regularQuantity.id);
        expediteQuantityElement.attr('id', expediteQuantity.id);

        $("#" + regularQuantity.id).val("0");
        if (hasExpedite) {
            $("#" + expediteQuantity.id).val("0");
        }

        regularQuantityElement.attr('min', regularQuantity.min);
        regularQuantityElement.attr('max', regularQuantity.max);

        expediteQuantityElement.attr('min', expediteQuantity.min);
        expediteQuantityElement.attr('max', expediteQuantity.max);

        if (selectedData.info != null) {
            for (var i = 0; i < selectedData.info.length; i++) {
                if (selectedData.info[i].Name != '') {
                    var label = `<small class="bold">${selectedData.info[i].name}</small>`;
                    documentTypeSample.append(label);
                }
                var image = `<img id="${i}-img" class="w-100 mb-3 "/>`;
                documentTypeSample.append(image);
                var imageUrl = `${baseUrl}${selectedData.info[i].source}`;
                $(`#${i}-img`).attr('src', imageUrl);
            }
        }
        loading.hide();
    }
    else {
        loading.hide();
    }
});

selectDocumentsBtn.on('click', function () {
    loading.show();
    let $this = docTypeSelect;
    selected = $this.find(":selected"),
        id = selected.attr('id'),
        regularId = regularQuantityElement.attr('id'),
        expediteId = expediteQuantityElement.attr('id'),
        regularQuantity = parseInt(regularQuantityElement.val()),
        expediteQuantity = parseInt(expediteQuantityElement.val());


    if (regularQuantity > 51 || expediteQuantity > 51) {
        loading.hide();
        var errorModal = $('#errorModal');
        var errorMessageElement = $('#errorMessage');

        errorMessageElement.text('You have exceeded the maximum no. of document!');
        errorModal.modal('show');
        return;
    }

    console.log('passed here');
    console.log(documentObject);
    var docIndex = documentObject.findIndex(x => x.Name == $this.val());
    var doc = documentObject.find(x => x.Name == $this.val());

    if (expediteQuantity > 0 && !regularQuantity) {
        if (docIndex != -1 && doc.Transaction == 'Expedite') {
            documentObject[docIndex].Quantity = expediteQuantity;
        }
        else {
            documentObject.push({ Name: $this.val(), Quantity: expediteQuantity, Transaction: "Expedite" });
        }
    }
    else if (regularQuantity > 0 && !expediteQuantity) {
        if (docIndex != -1 && doc.Transaction == 'Regular') {
            documentObject[docIndex].Quantity = regularQuantity;
        }
        else {
            documentObject.push({ Name: $this.val(), Quantity: regularQuantity, Transaction: "Regular" });
        }
    }
    else if (expediteQuantity > 0 && regularQuantity > 0) {
        if (docIndex != -1 && doc.Transaction == 'Expedite') {
            documentObject[docIndex].Quantity = expediteQuantity;
        }
        else {
            documentObject.push({ Name: $this.val(), Quantity: expediteQuantity, Transaction: "Expedite" });
        }
        if (docIndex != -1 && doc.Transaction == 'Regular') {
            documentObject[docIndex].Quantity = regularQuantity;
        }
        else {
            documentObject.push({ Name: $this.val(), Quantity: regularQuantity, Transaction: "Regular" });
        }
    }
    else { loading.hide(); return; }

    $("#documentsTable tbody > tr").empty();
    //documentsTableBody.empty();
    var header = $('.docsHeader');
    documentsTable.append('<tr class="docsHeader"><th>Document</th><th>Quantity</th><th>Transaction</th></tr>');
    //var tbodyContent = '';
    var numberOfDocuments = 0;
    var newDocument = [];
    $.each(documentObject, function (index, value) {
        var isExists = newDocument.find(e => e.Name == value.Name && e.Transaction == value.Transaction);
        if (isExists == undefined) {
            $('#documentsTable tr:last').after('<tr><td>'.concat(value.Name, '</td><td>', value.Quantity, '</td><td>', value.Transaction, '</td></tr>'));
            numberOfDocuments += value.Quantity;
            newDocument.push(value);
        }
    });
    //console.log(tbodyContent);
    //$('#documentsTable tbody').append(tbodyContent);
    $('#documentsCount').text(numberOfDocuments);
    $('#apostileData').val(JSON.stringify(newDocument));
    console.log(JSON.stringify(newDocument));
    console.log(documentObject);
    if (documentObject.length === 0)
        alert('Please ADD and/or INSERT number of documents.');
    else
        $('.modal').modal('hide');//$("#apostileModal .close").click()//$('#apostileModal').modal('hide');

    loading.hide();
});

backToStepThreeBtn.on('click', function () {
    loading.show();
    documentObject = [];

    $("#documentsTable tbody > tr").empty();
    //documentsTableBody.empty();
    var header = $('.docsHeader');
    documentsTable.append('<tr class="docsHeader"><th>Document</th><th>Quantity</th><th>Transaction</th></tr>');
    $('#documentsCount').text(0);
    ownerDocument.hide();
    applicationTypeSelection.show();
    loading.hide();
});

backToStepThreeAuthBtn.on('click', function () {
    loading.show();
    documentObject = [];
    docOwner = 0;

    $("#documentOwners").empty();
    $("#documentsTable tbody > tr").empty();
    //documentsTableBody.empty();
    var header = $('.docsHeader');
    documentsTable.append('<tr class="docsHeader"><th>Document</th><th>Quantity</th><th>Transaction</th></tr>');
    $('#documentsCount').text(0);
    authorizeDocument.hide();
    applicationTypeSelection.show();
    loading.hide();
});

addDocumentOwnerBtn.on('click', function () {
    if (docOwner > 9) {
        loading.hide();
        var errorModal = $('#errorModal');
        var errorMessageElement = $('.modal #errorMessage');
        console.log(errorMessageElement);
        errorMessageElement.text('You have exceeded the maximum no. of document owners!');
        errorModal.modal('show');
        return;
    }

    loading.show();
    console.log('clicked');
    docOwner += 1;
    window['documentObject' + docOwner] = [];

    $.ajax({
        url: '/Home/PartialApplicant?i=' + docOwner + '&id=' + applicantType,
        cache: false,
        success: function (html) {


            if (docOwner == 0) {
                $("#documentOwners").append(html);
                //alert(docOwner);
            } else {
                $("#documentOwners").append(html).insertAfter('documentOwner-' + docOwner - 1);
                //alert(docOwner);
                //alert("#qtyNbiClearanceRegular" + docOwner);
                $("#qtyNbiClearanceRegular" + docOwner).inputSpinner({ buttonsOnly: true })
                $("#qtyNbiClearanceExpedite" + docOwner).inputSpinner({ buttonsOnly: true })
            }
            //$(`#documentsTable${docOwner} tbody > tr`).empty();
            //$(`#documentsTable${docOwner}`).append('<tr class="docsHeader"><th>Document</th><th>Quantity</th><th>Transaction</th></tr>');
            //$('#documentsCount').text(0);

            

            $(`#docTypeSelect${docOwner}`).on('change', function () {
                let $this = $(this),
                    selected = $this.find(":selected"),
                    docOwnerBase = parseInt(selected.attr('id').replace(/[^\d.]/g, '')),
                    id = selected.attr('id').replace(docOwnerBase.toString(), ""),
                    value = selected.attr('value');
                selectedData = documents.filter(x => x.id == id)[0],
                    regularQuantity = selectedData.quantities.filter(x => x.name == 'Regular')[0],
                    expediteQuantity = selectedData.quantities.filter(x => x.name == 'Expedite')[0],
                    baseUrl = window.location.origin;

                console.log('docOwner');
                console.log(docOwnerBase);
                $(`#documentTypeSample${docOwnerBase}`).children().remove();
                if (selectedData != null) {
                    // change displayed document info
                    if (selectedData.description == '') {
                        $(`#documentTypeInfo${docOwnerBase}`).hide();
                    } else {
                        $(`#documentTypeInfo${docOwnerBase}`).show();
                        $(`#documentInfoText${docOwnerBase}`).text(selectedData.description);
                    }

                    console.log(regularQuantity);
                    

                    $(`#documentQuantity${docOwnerBase} .regular`).attr('id', `${regularQuantity.id}${docOwnerBase}`);
                    $(`#documentQuantity${docOwnerBase} .expedite`).attr('id', `${expediteQuantity.id}${docOwnerBase}`);

                    $(`#${regularQuantity.id}${docOwnerBase}`).val("0");
                    if (hasExpedite) {
                        $(`#${regularQuantity.id}${docOwnerBase}`).val("0");
                    }

                    $(`#documentQuantity${docOwnerBase} .regular`).attr('min', `${regularQuantity.min}`);
                    $(`#documentQuantity${docOwnerBase} .regular`).attr('max', `${regularQuantity.max}`);

                    $(`#documentQuantity${docOwnerBase} .expedite`).attr('min', `${expediteQuantity.min}`);
                    $(`#documentQuantity${docOwnerBase} .expedite`).attr('max', `${expediteQuantity.max}`);

                    if (selectedData.info != null) {
                        for (var i = 0; i < selectedData.info.length; i++) {
                            if (selectedData.info[i].Name != '') {
                                var label = `<small class="bold">${selectedData.info[i].name}</small>`;
                                $(`#documentTypeSample${docOwnerBase}`).append(label);
                            }
                            var image = `<img id="${i}-img-${docOwnerBase}" class="w-100 mb-3 "/>`;
                            $(`#documentTypeSample${docOwnerBase}`).append(image);
                            var imageUrl = `${baseUrl}${selectedData.info[i].source}`;
                            $(`#${i}-img-${docOwnerBase}`).attr('src', imageUrl);
                        }
                    }
                }
            });

            $(`#selectDocumentsBtnModal${docOwner}`).on('click', function () {
                let docOwnerBase = $(this).attr('id').replace(/[^\d.]/g, ''),
                    $this = $(`#docTypeSelect${docOwnerBase}`);
                selected = $this.find(":selected"),
                    id = selected.attr('id').replace(docOwnerBase.toString(), ""),
                    regularId = $(`#documentQuantity${docOwnerBase} .regular`).attr('id'),
                    expediteId = $(`#documentQuantity${docOwnerBase} .expedite`).attr('id'),
                    regularQuantity = parseInt($(`#documentQuantity${docOwnerBase} .regular`).val()),
                    expediteQuantity = parseInt($(`#documentQuantity${docOwnerBase} .expedite`).val());

                console.log('docOwner');
                console.log(docOwnerBase);

                if (regularQuantity > 51 || expediteQuantity > 51) {
                    loading.hide();
                    var errorModal = $('#errorModal');
                    var errorMessageElement = $('#errorMessage');

                    errorMessageElement.text('You have exceeded the maximum no. of document!');
                    errorModal.modal('show');
                    return;
                }

                var docIndex = window['documentObject' + docOwnerBase].findIndex(x => x.Name == $this.val());
                var doc = window['documentObject' + docOwnerBase].find(x => x.Name == $this.val());

                if (expediteQuantity > 0 && !regularQuantity) {
                    if (docIndex != -1 && doc.Transaction == 'Expedite') {
                        window['documentObject' + docOwnerBase][docIndex].Quantity = expediteQuantity;
                    }
                    else {
                        window['documentObject' + docOwnerBase].push({ Name: $this.val(), Quantity: expediteQuantity, Transaction: "Expedite" });
                    }
                }
                else if (regularQuantity > 0 && !expediteQuantity) {
                    if (docIndex != -1 && doc.Transaction == 'Regular') {
                        window['documentObject' + docOwnerBase][docIndex].Quantity = regularQuantity;
                    }
                    else {
                        window['documentObject' + docOwnerBase].push({ Name: $this.val(), Quantity: regularQuantity, Transaction: "Regular" });
                    }
                }
                else if (expediteQuantity > 0 && regularQuantity > 0) {
                    if (docIndex != -1 && doc.Transaction == 'Expedite') {
                        window['documentObject' + docOwnerBase][docIndex].Quantity = expediteQuantity;
                    }
                    else {
                        window['documentObject' + docOwnerBase].push({ Name: $this.val(), Quantity: expediteQuantity, Transaction: "Expedite" });
                    }
                    if (docIndex != -1 && doc.Transaction == 'Regular') {
                        window['documentObject' + docOwnerBase][docIndex].Quantity = regularQuantity;
                    }
                    else {
                        window['documentObject' + docOwnerBase].push({ Name: $this.val(), Quantity: regularQuantity, Transaction: "Regular" });
                    }
                }
                else { loading.hide(); return; }

                $(`#documentsTable${docOwnerBase} tbody > tr`).empty();
                //documentsTableBody.empty();
                var header = $('.docsHeader');
                $(`#documentsTable${docOwnerBase}`).append('<tr class="docsHeader"><th>Document</th><th>Quantity</th><th>Transaction</th></tr>');
                //var tbodyContent = '';
                var numberOfDocuments = 0;
                var newDocument = [];
                $.each(window['documentObject' + docOwnerBase], function (index, value) {
                    var isExists = newDocument.find(e => e.Name == value.Name && e.Transaction == value.Transaction);
                    if (isExists == undefined) {
                        $(`#documentsTable${docOwnerBase} tr:last`).after('<tr><td>'.concat(value.Name, '</td><td>', value.Quantity, '</td><td>', value.Transaction, '</td></tr>'));
                        numberOfDocuments += value.Quantity;
                        newDocument.push(value);
                    }
                });
                //console.log(tbodyContent);
                //$('#documentsTable tbody').append(tbodyContent);
                $(`#documentsCount${docOwnerBase}`).text(numberOfDocuments);
                $(`#apostileData${docOwnerBase}`).val(JSON.stringify(newDocument));
                console.log(JSON.stringify(newDocument));
                console.log('DOC');
                console.log(window['documentObject' + docOwnerBase]);
                if (window['documentObject' + docOwnerBase].length === 0)
                    alert('Please ADD and/or INSERT number of documents.');
                else
                    $('.modal').modal('hide');//$("#apostileModal .close").click()//$('#apostileModal').modal('hide');

            });


            if (hasExpedite) {
                console.log('hasExpedite');
                $('.transactionQuantity.expedite').attr('disabled', false);
                $('.transactionQuantity.regular').attr('disabled', false);
            }
        },
        async: false
    });

    loading.hide();
   
});

goToStepFiveButton.on('click', function () {
    if (hasTermsAndConditions)
        termsAndConditions.modal('show');
    else {
        docOwnerStepFive();
    }
});

goToStepFiveButtonAuth.on('click', function () {
    if (hasTermsAndConditions)
        termsAndConditions.modal('show');
    else {
        authorizedStepFive();
    }
});

agreeterms.on('click', function () {
    if (applicantType == 0) {
        termsAndConditions.modal('hide');
        docOwnerStepFive();
    }
    else {
        termsAndConditions.modal('hide');
        authorizedStepFive();
    }
});

stepFiveBackBtn.on('click', function () {
    loading.show();
    if (applicantType == 0) {
        $("#step-three").hide();
        ownerDocument.show();
        loading.hide();
    }
    else {
        $("#step-three").hide();
        authorizeDocument.show();
        loading.hide();
    }
});

sendPdfToEmail.on('click', function () {
    loading.show();
    $.ajax({
        type: "post",
        url: "/Home/PostApplication",
        data: record,
        datatype: "json",
        cache: false,
        success: function (data) {
            $('#formContainer').attr('style', 'display: none!important');
            if (data.status == 'Success') {
                $('#error').hide();
                $('#success').show();
            }
            else {
                $('#success').hide();
                $('#error #errorMessage').val(data.Message);
                $('#error').show();
            }
            loading.hide();
        },
        error: function (data){
            loading.hide();
        }
    });
});

schedAnother.on('click', function () {
    window.location.href = '/Home/DocumentLocation';
});

exit.on('click', function () {
    let baseUrl = window.location.origin;
    window.location = `${baseUrl}/Account/LogOff`;
});

resendEmail.on('click', function () {
    loading.show();
    $.ajax({
        type: "post",
        url: "/Home/ResendEmail",
        data: record,
        datatype: "json",
        cache: false,
        success: function (data) {
            $('#formContainer').attr('style', 'display: none!important');
            $('#success').show();
            loading.hide();
        },
        error: function (data) {
            loading.hide();
        }
    });
});

function addPartialView() {
    let baseUrl = window.location.origin;
    window.location = `${baseUrl}/Account/LogOff`;
}

function getDocumentsType() {
    $.get("/Home/GetDocuments", function (data, status) {
        documents = data;
    });
};

function getPrices() {
    $.get("/Home/GetPrices", function (data, status) {
        prices = data;
    });
}

function SetCode(codeParam, ifHasTerms) {
    code = codeParam;
    hasTermsAndConditions = ifHasTerms;
    console.log(ifHasTerms);
}

function docOwnerStepFive() {

    loading.show();
    docuTotalCount = 0;
    if ($('.fNamePartial').val() == "" || $('.lNamePartial').val() == "" || $('#Title').val() == "" || $('#Record_FirstName').val() == "" || $('#Record_LastName').val() == ""
        || $('#Record_ContactNumber').val() == "" || $('#Record_CountryDestination').val() == "" || $('#apostileData').val() == "" || $('#apostileData').val() == "[]" || $('.apostiledataPartial').val() == "" || $('.apostiledataPartial').val() == "[]"
        || $('#dateOfbirthPartial').val() == "" || $('.countrydestinationPartial').val() == "" || $('#Record_DateOfBirth').val() == ""
    ) {
        if ($('.fNamePartial').val() == "") {
            $('.fNamePartialValidation').text('This field is required.');
        }
        else
            $('.fNamePartialValidation').text('');

        if ($('.lNamePartial').val() == "") {
            $('.lNamePartialValidation').text('This field is required.');
        }
        else
            $('.lNamePartialValidation').text('');

        if ($('.dateOfbirthPartial').val() == "") {
            $('.dateOfbirthPartialValidation').text('This field is required.');
        }
        else
            $('.dateOfbirthPartialValidation').text('');

        if ($('.countrydestinationPartial').val() == "") {
            $('.countrydestinationPartialValidation').text('This field is required.');
        }
        else
            $('.countrydestinationPartialValidation').text('');

        if ($('.apostiledataPartial').val() == "" || $('.apostiledataPartial').val() == null) {
            $('.apostiledataPartialValidation').text('Please add documents for apostillization.');
        }
        else if ($('.apostiledataPartial').val() == "[]") {
            $('.apostiledataPartialValidation').text('Please insert document quantity.');
        }
        else
            $('.apostiledataPartialValidation').text('');

        if ($('#Record_FirstName').val() == "")
            $('.fname').text('This field is required.');
        else
            $('.fname').text('');

        if ($('#Record_LastName').val() == "")
            $('.lname').text('This field is required.');
        else
            $('.lname').text('');


        if ($('#Record_DateOfBirth').val() == "")
            $('.dateofbirth').text('This field is required.');
        else
            $('.dateofbirth').text('');

        if ($('#Record_ContactNumber').val() == "")
            $('.contactnumber').text('This field is required.');
        else
            $('.contactnumber').text('');

        if ($('#Record_CountryDestination').val() == "")
            $('.countrydestination').text('This field is required.');
        else
            $('.countrydestination').text('');

        if ($('#apostileData').val() == "")
            $('.apostiledata').text('Please add documents for apostillization.');
        else if ($('#apostileData').val() == "[]")
            $('.apostiledata').text('Please insert document quantity.');
        else
            $('.apostiledata').text('');

        loading.hide();
        return;
    }
    else {
        $('.title').text('');
        $('.fname').text('');
        $('.lname').text('');
        $('.address').text('');
        $('.contactnumber').text('');
        $('.countrydestination').text('');
        $('.apostiledata').text('');
    }

    var fnamePartialClassReq = $('.fNamePartial').length;
    var fnamePartialClass = $('.fNamePartial').filter(function () {
        return this.value != '';
    });
    if ((fnamePartialClass.length >= 0 && (fnamePartialClass.length !== fnamePartialClassReq)) || fnamePartialClass == '[]') {
        $('.fNamePartialValidation').text('This field is required.');
        loading.hide();
        return;
    }

    var lnamePartialClassReq = $('.lNamePartial').length;
    var lnamePartialClass = $('.lNamePartial').filter(function () {
        return this.value != '';
    });
    if ((lnamePartialClass.length >= 0 && (lnamePartialClass.length !== lnamePartialClassReq)) || lnamePartialClass == '[]') {
        $('.lNamePartialValidation').text('This field is required.');
        loading.hide();
        return;
    }

    var dateOfbirthPartialClassReq = $('.dateOfbirthPartial').length;
    var dateOfbirthClass = $('.dateOfbirthPartial').filter(function () {
        return this.value != '';
    });
    if ((dateOfbirthClass.length >= 0 && (dateOfbirthClass.length !== dateOfbirthPartialClassReq)) || dateOfbirthClass == '[]') {
        $('.dateOfbirthPartialValidation').text('This field is required.');
        loading.hide();
        return;
    }


    var countrydestinationPartialClassReq = $('.countrydestinationPartial').length;
    var countrydestinationClass = $('.countrydestinationPartial').filter(function () {
        return this.value != '';
    });
    if ((countrydestinationClass.length >= 0 && (countrydestinationClass.length !== countrydestinationPartialClassReq)) || countrydestinationClass == '[]') {
        $('.countrydestinationPartialValidation').text('This field is required.');
        loading.hide();
        return;
    }

    var reqlength = $('.apostiledataPartial').length;
    console.log(reqlength);
    var value = $('.apostiledataPartial').filter(function () {
        return this.value != '';
    });
    if ((value.length >= 0 && (value.length !== reqlength)) || value == '[]') {
        alert('Please fill out all required fields.');
        $('.apostiledataPartialValidation').text('Please insert document quantity.');
        loading.hide();
        return;
    }

    $('#LblNameOfApplicant').text($('#Record_FirstName').val().toUpperCase() + ' ' + $('#Record_MiddleName').val().toUpperCase() + ' ' + $('#Record_LastName').val().toUpperCase() + ' ' + $('#Record_Suffix').val().toUpperCase());
    $('#LblCountryDestination').text($('#Record_CountryDestination').val());
    $('#LblTypeOfDocument').empty();
    let totalFee = 0;

    for (var i = 0; i < documentObject.length; i++) {
        let documentText = `<p class="mb-0">(${documentObject[i].Quantity}) (${documentObject[i].Transaction}) ${documentObject[i].Name}</p>`;
        $('#LblTypeOfDocument').append(documentText);
        if (documentObject[i].Transaction == 'Regular')
            totalFee += prices.regular * documentObject[i].Quantity;
        else
            totalFee += prices.expedite * documentObject[i].Quantity;

        if (i == documentObject.length - 1)
            $('#LblPayment').text(`PHP ${totalFee.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')}`);
    }

    $("#step-one").hide();
    $("#step-one-authorized").hide();
    $("#step-three").show();

    if (applicantType == 0) {
        if ($('#apostileData').val() !== '' || $('#apostileData').val() !== null) {
            var docuType = JSON.parse($('#apostileData').val());
            var docuCount = 0;
            docuType.forEach(function (v) {
                docuCount += parseInt(v.Quantity);
            });
            docuTotalCount += docuCount
        }
    }

    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;

    ownerModel.ApostileData = JSON.stringify(documentObject);
    ownerModel.ApplicationCode = code;
    ownerModel.ContactNumber = $('#Record_ContactNumber').val();
    ownerModel.CountryDestination = $('#Record_CountryDestination').val();
    ownerModel.DateOfBirth = $('#Record_DateOfBirth').val();
    ownerModel.Fees = totalFee;
    ownerModel.FirstName = $('#Record_FirstName').val();
    ownerModel.MiddleName = $('#Record_MiddleName').val();
    ownerModel.LastName = $('#Record_LastName').val();
    ownerModel.ProcessingSite = $('#site').val();
    ownerModel.ProcessingSiteAddress = $('#address').val();
    ownerModel.Suffix = $('#Record_Suffix').val();
    ownerModel.ScheduleDate = `${$('#appointmentDate').text()} ${$('input[name=option1]:checked').val()}`;

    record.ApplicationCode = code;
    record.ApplicantCount = 0;
    record.ScheduleDate = `${$('#appointmentDate').text()} ${$('input[name=option1]:checked').val()}`;
    record.Records = null;
    record.Record = ownerModel;
    record.Token = token;

    console.log(record);
    loading.hide();
}

function authorizedStepFive() {

    loading.show();
    docuTotalCount = 0;
    ownerContainer.attr('style', 'display: none');
    authContainer.show();

    if ($('.fNamePartial').val() == "" || $('.lNamePartial').val() == "" || $('#Title').val() == "" || $('#AuthRecord_FirstName').val() == "" || $('#AuthRecord_LastName').val() == ""
        || $('#AuthRecord_ContactNumber').val() == "" || $('#AuthRecord_CountryDestination').val() == "" || $('.apostiledataPartial').val() == "[]"
        || $('#dateOfbirthPartial').val() == "" || $('.countrydestinationPartial').val() == "" || $('#AuthRecord_DateOfBirth').val() == ""
    ) {
        console.log('not valid');
        if ($('.fNamePartial').val() == "") {
            $('.fNamePartialValidation').text('This field is required.');
            console.log(1);

        }
        else
            $('.fNamePartialValidation').text('');

        if ($('.lNamePartial').val() == "") {
            $('.lNamePartialValidation').text('This field is required.');
            console.log(2);
        }
        else
            $('.lNamePartialValidation').text('');

        if ($('.dateOfbirthPartial').val() == "") {
            $('.dateOfbirthPartialValidation').text('This field is required.');
            console.log(3);
        }
        else
            $('.dateOfbirthPartialValidation').text('');

        if ($('.countrydestinationPartial').val() == "") {
            $('.countrydestinationPartialValidation').text('This field is required.');
            console.log(4);
        }
        else
            $('.countrydestinationPartialValidation').text('');

        if ($('.apostiledataPartial').val() == "" || $('.apostiledataPartial').val() == null) {
            $('.apostiledataPartialValidation').text('Please add documents for apostillization.');
            console.log(5);
        }
        else if ($('.apostiledataPartial').val() == "[]") {
            $('.apostiledataPartialValidation').text('Please insert document quantity.');
            console.log(6);
        }
        else
            $('.apostiledataPartialValidation').text('');

        if ($('#AuthRecord_FirstName').val() == "") {
            $('.fname').text('This field is required.');
            console.log(7);
        }
        else
            $('.fname').text('');

        if ($('#AuthRecord_LastName').val() == "") {
            $('.lname').text('This field is required.');
            console.log(8);
        }
        else
            $('.lname').text('');


        if ($('#AuthRecord_DateOfBirth').val() == "")
            $('.dateofbirth').text('This field is required.');
        else
            $('.dateofbirth').text('');

        if ($('#AuthRecord_ContactNumber').val() == "")
            $('.contactnumber').text('This field is required.');
        else
            $('.contactnumber').text('');

        if ($('#AuthRecord_CountryDestination').val() == "")
            $('.countrydestination').text('This field is required.');
        else
            $('.countrydestination').text('');

        //if ($('#apostileData').val() == "") {
        //    $('.apostiledata').text('Please add documents for apostillization.');
        //    console.log(9);
        //}
        //else if ($('#apostileData').val() == "[]") {
        //    $('.apostiledata').text('Please insert document quantity.');
        //    console.log(10);
        //}
        //else
        //    $('.apostiledata').text('');

        loading.hide();
        return;
    }
    else {
        $('.title').text('');
        $('.fname').text('');
        $('.lname').text('');
        $('.address').text('');
        $('.contactnumber').text('');
        $('.countrydestination').text('');
        $('.apostiledata').text('');
    }

    var fnamePartialClassReq = $('.fNamePartial').length;
    var fnamePartialClass = $('.fNamePartial').filter(function () {
        return this.value != '';
    });
    if ((fnamePartialClass.length >= 0 && (fnamePartialClass.length !== fnamePartialClassReq)) || fnamePartialClass == '[]') {
        $('.fNamePartialValidation').text('This field is required.');
        loading.hide();
        return;
    }

    var lnamePartialClassReq = $('.lNamePartial').length;
    var lnamePartialClass = $('.lNamePartial').filter(function () {
        return this.value != '';
    });
    if ((lnamePartialClass.length >= 0 && (lnamePartialClass.length !== lnamePartialClassReq)) || lnamePartialClass == '[]') {
        $('.lNamePartialValidation').text('This field is required.');
        loading.hide();
        return;
    }

    var dateOfbirthPartialClassReq = $('.dateOfbirthPartial').length;
    var dateOfbirthClass = $('.dateOfbirthPartial').filter(function () {
        return this.value != '';
    });
    if ((dateOfbirthClass.length >= 0 && (dateOfbirthClass.length !== dateOfbirthPartialClassReq)) || dateOfbirthClass == '[]') {
        $('.dateOfbirthPartialValidation').text('This field is required.');
        loading.hide();
        return;
    }


    var countrydestinationPartialClassReq = $('.countrydestinationPartial').length;
    var countrydestinationClass = $('.countrydestinationPartial').filter(function () {
        return this.value != '';
    });
    if ((countrydestinationClass.length >= 0 && (countrydestinationClass.length !== countrydestinationPartialClassReq)) || countrydestinationClass == '[]') {
        $('.countrydestinationPartialValidation').text('This field is required.');
        loading.hide();
        return;
    }

    var reqlength = $('.apostiledataPartial').length;
    var value = $('.apostiledataPartial').filter(function () {
        return this.value != '';
    });
    if ((value.length >= 0 && (value.length !== reqlength)) || value == '[]') {
        alert('Please fill out all required fields.');
        $('.apostiledataPartialValidation').text('Please insert document quantity.');
        loading.hide();
        return;
    }

    let totalFees = 0;
    let documentsParent = $('#documentsContainer');
    documentsParent.empty();
    authNameLbl.text($('#AuthRecord_FirstName').val().toUpperCase() + ' ' + $('#AuthRecord_MiddleName').val().toUpperCase() + ' ' + $('#AuthRecord_LastName').val().toUpperCase() + ' ' + $('#Record_Suffix').val().toUpperCase());
    authContactNumber.text($('#AuthRecord_ContactNumber').val());
    let records = [];

    for (var i = 1; i <= docOwner; i++) {
        let fname = $(`#Records_${i}__FirstName`).val();
        let mname = $(`#Records_${i}__MiddleName`).val();
        let lname = $(`#Records_${i}__LastName`).val();
        let bday = $(`#Records_${i}__DateOfBirth`).val();
        let suffix = $(`#Records_${i}__Suffix`).val();
        let destination = $(`#Records_${i}__CountryDestination`).val();

        let codeContainer = `<div class="font-weight-bold mb-2"><span id="LblAppointmentCode-${i}">${code}-${i}</div>`;
        let ownerContainer = `<div class="row"><div class="step-three-field col-lg-3 col-md-4 col-sm-12"><span class="bold">Document Owner: </span></div><div class="col-lg-9 col-md-8 col-sm-12"><span class="black-text text-uppercase" id="documentOwner-${i}">${fname} ${mname} ${lname} ${suffix}</span></div></div>`;
        let destinationContainer = `<div class="row"><div class="step-three-field col-lg-3 col-md-4 col-sm-12"><span class="bold">Country of Destination: </span></div><div class="col-lg-9 col-md-8 col-sm-12"><span class="black-text" id="LblCountryDestination-${i}">${destination}</span></div></div>`;
        let documentsContainer = `<div class="row"><div class="step-three-field col-lg-3 col-md-4 col-sm-12"><span class="bold">Documents: </span></div><div class="col-lg-9 col-md-8 col-sm-12"><span class="black-text" id="LblTypeOfDocument-${i}"></span></div></div>`;
        let subTotalContainer = `<div class="row"><div class="step-three-field col-lg-3 col-md-4 col-sm-12"><span class="bold">Sub-Total:</span></div><div class="col-lg-9 col-md-8 col-sm-12 font-weight-bold"><span class="black-text" id="LblPayment-${i}"></span></div></div>`;

        documentsParent.append(codeContainer);
        documentsParent.append(ownerContainer);
        documentsParent.append(destinationContainer);
        documentsParent.append(documentsContainer);
        documentsParent.append(subTotalContainer);

        let docsContainer = $(`#LblTypeOfDocument-${i}`);
        let subTotalContainerElement = $(`#LblPayment-${i}`);
        let subFee = 0;

        for (var x = 0; x < window['documentObject' + i].length; x++) {
            let documentText = `<p class="mb-0">(${window['documentObject' + i][x].Quantity}) (${window['documentObject' + i][x].Transaction}) ${window['documentObject' + i][x].Name}</p>`;
            docsContainer.append(documentText);
            if (window['documentObject' + i][x].Transaction == 'Regular')
                subFee += prices.regular * window['documentObject' + i][x].Quantity;
            else
                subFee += prices.expedite * window['documentObject' + i][x].Quantity;


            console.log('subFee');
            console.log(subFee);

            if (x == window['documentObject' + i].length - 1) {
                totalFees += subFee;
                subTotalContainerElement.text(`PHP ${subFee.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')}`);

                console.log('totalFees');
                console.log(totalFees);
            }
        }

        documentsParent.append('<hr />');

        console.log('docOwner');
        console.log(docOwner);
        if (i == docOwner)
            $('#totalFeesAuth').text(`PHP ${totalFees.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')}`);

        let model = {
            ApostileData: JSON.stringify(window['documentObject' + i]),
            ApplicationCode: `${code}-${i}`,
            CountryDestination: destination,
            DateOfBirth: bday,
            Fees: subFee,
            FirstName: fname,
            MiddleName: mname,
            LastName: lname,
            ProcessingSite: $('#site').val(),
            ProcessingSiteAddress: $('#address').val(),
            Suffix: suffix,
            ScheduleDate: `${$('#appointmentDate').text()} ${$('input[name=option1]:checked').val()}`,
            NameOfRepresentative: `${$(`#AuthRecord_FirstName`).val()} ${$(`#AuthRecord_MiddleName`).val()} ${$(`#AuthRecord_LastName`).val()} ${$(`#AuthRecord_Suffix`).val()}`,
            RepresentativeContactNumber: $('#AuthRecord_ContactNumber').val()
        };

        records.push(model);
    }


    $('#LblTypeOfDocument').empty();

    let authorized = {
        ApplicationCode: code,
        ContactNumber: $('#AuthRecord_ContactNumber').val(),
        Fees: totalFees,
        FirstName: $(`#AuthRecord_FirstName`).val(),
        MiddleName: $(`#AuthRecord_MiddleName`).val(),
        LastName: $(`#AuthRecord_LastName`).val(),
        ProcessingSite: $('#site').val(),
        ProcessingSiteAddress: $('#address').val(),
        Suffix: $(`#AuthRecord_Suffix`).val()
    };

    record.ApplicationCode = code;
    record.ApplicantCount = docOwner;
    record.ScheduleDate = `${$('#appointmentDate').text()} ${$('input[name=option1]:checked').val()}`;
    record.Records = records;
    record.Record = authorized;
    record.Token = token;

    console.log(record);

    $("#step-one").hide();
    $("#step-one-authorized").hide();
    $("#step-three").show();
    loading.hide();

}

function HasExpedite(ifHasExpedite) {
    hasExpedite = ifHasExpedite;

    console.log('hasExpedite');
    console.log(hasExpedite);
}