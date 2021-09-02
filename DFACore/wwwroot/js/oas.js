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
let addDocumentOwnerBtn = $('#addDocumentOwnerBtn');
let docOwner = 0;
let documentObject = []
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
    authorizeDocument.show();
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

addDocumentOwnerBtn.on('click', function () {
    docOwner += 1;

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
                $(`#documentTypeSample${docOwner}`).children().remove();
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

function addPartialView() {
}

function getDocumentsType() {
    $.get("/Home/GetDocuments", function (data, status) {
        documents = data;
    });
};

