

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

siteAndScheduleButton.on('click', function () {
    siteAndScheduleSelection.hide();
    applicationTypeSelection.show();
});

ownerButton.on('click', function () {
    applicantType = 0;
    applicationTypeSelection.hide();
    ownerDocument.show();
});

authorizedButton.on('click', function () {
    applicantType = 1;
    applicationTypeSelection.hide();
    console.log('pass');
    authorizeDocument.show();
    addDocumentOwnerBtn.click();

});

backToStepTwoButton.on('click', function () {
    applicationTypeSelection.hide();
    stepTwo.show();
    siteAndScheduleSelection.show();
});

stepFourBackButton.on('click', function () {
    ownerDocument.hide();
    applicationTypeSelection.show();
    $('#step-select-processing-site').hide();
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
});

docTypeSelect.on('change', function () {
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
    }
});

selectDocumentsBtn.on('click', function () {
    let $this = docTypeSelect;
    selected = $this.find(":selected"),
        id = selected.attr('id'),
        regularId = regularQuantityElement.attr('id'),
        expediteId = expediteQuantityElement.attr('id'),
        regularQuantity = parseInt(regularQuantityElement.val()),
        expediteQuantity = parseInt(expediteQuantityElement.val());


    if (regularQuantity > 10 || expediteQuantity > 10) {
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
    else { return; }

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

});

backToStepThreeBtn.on('click', function () {
    documentObject = [];

    $("#documentsTable tbody > tr").empty();
    //documentsTableBody.empty();
    var header = $('.docsHeader');
    documentsTable.append('<tr class="docsHeader"><th>Document</th><th>Quantity</th><th>Transaction</th></tr>');
    $('#documentsCount').text(0);
    ownerDocument.hide();
    applicationTypeSelection.show();
});

backToStepThreeAuthBtn.on('click', function () {
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
});

addDocumentOwnerBtn.on('click', function () {
    console.log('clicked');
    docOwner += 1;
    window['documentObject' + docOwner] = [];

    if (docOwner > 9) {
        var errorModal = $('#errorModal');
        var errorMessageElement = $('#errorMessage');

        errorMessageElement.text('You have exceeded the maximum no. of document owners!');
        errorModal.modal('show');
        return;
    }

    $.ajax({
        url: '/Home/PartialApplicant?i=' + docOwner + '&id=' + applicantType,
        cache: false,
        success: function (html) {
            if (docOwner == 0) {
                $("#documentOwners").append(html);
            } else {
                $("#documentOwners").append(html).insertAfter('documentOwner-' + docOwner - 1);
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

                    $(`#documentQuantity${docOwnerBase} .regular`).attr('id', `${regularQuantity.id}${docOwnerBase}`);
                    $(`#documentQuantity${docOwnerBase} .expedite`).attr('id', `${expediteQuantity.id}${docOwnerBase}`);

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

                if (regularQuantity > 10 || expediteQuantity > 10) {
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
                else { return; }

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
        },
        async: false
    });
});

goToStepFiveButton.on('click', function () {
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
        return;
    }

    var lnamePartialClassReq = $('.lNamePartial').length;
    var lnamePartialClass = $('.lNamePartial').filter(function () {
        return this.value != '';
    });
    if ((lnamePartialClass.length >= 0 && (lnamePartialClass.length !== lnamePartialClassReq)) || lnamePartialClass == '[]') {
        $('.lNamePartialValidation').text('This field is required.');
        return;
    }

    var dateOfbirthPartialClassReq = $('.dateOfbirthPartial').length;
    var dateOfbirthClass = $('.dateOfbirthPartial').filter(function () {
        return this.value != '';
    });
    if ((dateOfbirthClass.length >= 0 && (dateOfbirthClass.length !== dateOfbirthPartialClassReq)) || dateOfbirthClass == '[]') {
        $('.dateOfbirthPartialValidation').text('This field is required.');
        return;
    }


    var countrydestinationPartialClassReq = $('.countrydestinationPartial').length;
    var countrydestinationClass = $('.countrydestinationPartial').filter(function () {
        return this.value != '';
    });
    if ((countrydestinationClass.length >= 0 && (countrydestinationClass.length !== countrydestinationPartialClassReq)) || countrydestinationClass == '[]') {
        $('.countrydestinationPartialValidation').text('This field is required.');
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
});

goToStepFiveButtonAuth.on('click', function () {
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
        return;
    }

    var lnamePartialClassReq = $('.lNamePartial').length;
    var lnamePartialClass = $('.lNamePartial').filter(function () {
        return this.value != '';
    });
    if ((lnamePartialClass.length >= 0 && (lnamePartialClass.length !== lnamePartialClassReq)) || lnamePartialClass == '[]') {
        $('.lNamePartialValidation').text('This field is required.');
        return;
    }

    var dateOfbirthPartialClassReq = $('.dateOfbirthPartial').length;
    var dateOfbirthClass = $('.dateOfbirthPartial').filter(function () {
        return this.value != '';
    });
    if ((dateOfbirthClass.length >= 0 && (dateOfbirthClass.length !== dateOfbirthPartialClassReq)) || dateOfbirthClass == '[]') {
        $('.dateOfbirthPartialValidation').text('This field is required.');
        return;
    }


    var countrydestinationPartialClassReq = $('.countrydestinationPartial').length;
    var countrydestinationClass = $('.countrydestinationPartial').filter(function () {
        return this.value != '';
    });
    if ((countrydestinationClass.length >= 0 && (countrydestinationClass.length !== countrydestinationPartialClassReq)) || countrydestinationClass == '[]') {
        $('.countrydestinationPartialValidation').text('This field is required.');
        return;
    }

    var reqlength = $('.apostiledataPartial').length;
    var value = $('.apostiledataPartial').filter(function () {
        return this.value != '';
    });
    if ((value.length >= 0 && (value.length !== reqlength)) || value == '[]') {
        alert('Please fill out all required fields.');
        $('.apostiledataPartialValidation').text('Please insert document quantity.');
        return;
    }

    let totalFees = 0;
    let documentsParent = $('#documentsContainer');
    documentsParent.empty();
    authNameLbl.text($('#AuthRecord_FirstName').val().toUpperCase() + ' ' + $('#AuthRecord_MiddleName').val().toUpperCase() + ' ' + $('#AuthRecord_LastName').val().toUpperCase() + ' ' + $('#Record_Suffix').val().toUpperCase());
    authContactNumber.text($('#AuthRecord_ContactNumber').val());

    for (var i = 1; i <= docOwner; i++) {
        let fname = $(`#Records_${i}__FirstName`).val();
        let mname = $(`#Records_${i}__MiddleName`).val();
        let lname = $(`#Records_${i}__LastName`).val();
        let bday = $(`#Records_${i}__DateOfBirth`).val();
        let destination = $(`#Records_${i}__CountryDestination`).val();

        let codeContainer = `<div class="font-weight-bold mb-2"><span id="LblAppointmentCode-${i}">${code}-${i}</div>`;
        let ownerContainer = `<div class="row"><div class="step-three-field col-lg-3 col-md-4 col-sm-12"><span class="bold">Document Owner: </span></div><div class="col-lg-9 col-md-8 col-sm-12"><span class="black-text text-uppercase" id="documentOwner-${i}">${fname} ${mname} ${lname}</span></div></div>`;
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

            if (x == window['documentObject' + i].length - 1 ) {
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
    }

    


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

});

function addPartialView() {
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

function SetCode(codeParam) {
    code = codeParam;
}

