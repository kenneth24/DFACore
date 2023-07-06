

$(document).ready(function () {
    $('#loading').hide();
    let checkmark = $('.checkmark'),
        siteButton = $('#site'),
        documentStatusRadio = $('input[type=radio][name=DocumentStatus]'),
        documentTypeRadio = $('input[type=radio][name=DocumentType]'),
        lblProcessingSiteAddress = $('#lblProcessingSiteAddress'),
        lblProcessingSiteOfficeHours = $('#lblProcessingSiteOfficeHours'),
        lblProcessingSiteContactNumber = $('#lblProcessingSiteContactNumber'),
        lblProcessingSiteEmail = $('#lblProcessingSiteEmail'),
        mapAddressButton = $('#mapAddress')
    addressInput = $('#address'),
        hasExpiditeInput = $('#HasExpidite'),
        branches = [];

    getBranches();



    documentStatusRadio.change(function () {
        let $this = $(this),
            value = $this.val(),
            philippinesOptions = $('option.ph');

        //siteButton.val('1');

        if (value != 'Abroad') {
            philippinesOptions.hide();
        }
        else {
            philippinesOptions.show();
        }
    });


    siteButton.on('change', function () {

        var selectedValue = $(this).val();
        localStorage.setItem('selectedSite', selectedValue);

        let $this = $(this),
            selectedSite = $this.val(),
            branchDetail = branches.filter(x => x.id == selectedSite)[0];


        //console.log(branchDetail);
        hasExpiditeInput.val(branchDetail.hasExpidite ? 1 : 0);
        //console.log(hasExpiditeInput.val());

        mapAddressButton.attr('href', branchDetail.mapAddress);
        lblProcessingSiteAddress.text(branchDetail.branchAddress);
        lblProcessingSiteOfficeHours.text(branchDetail.officeHours);
        lblProcessingSiteContactNumber.text(branchDetail.contactNumber);
        lblProcessingSiteEmail.text(branchDetail.email);
        addressInput.val(branchDetail.branchAddress);
    });

    function getBranches() {
        $.get(`${urlBase}Home/GetBranches`, function (data, status) {

            if (status === "success") {
                // The request was successful
                branches = data;

                var storedValue = localStorage.getItem('selectedSite');
                if (storedValue) {
                    $('#site').val(storedValue);
                    let selectedSite = storedValue,
                        branchDetail = branches.filter(x => x.id == selectedSite)[0];
                    hasExpiditeInput.val(branchDetail.hasExpidite ? 1 : 0);

                    mapAddressButton.attr('href', branchDetail.mapAddress);
                    lblProcessingSiteAddress.text(branchDetail.branchAddress);
                    lblProcessingSiteOfficeHours.text(branchDetail.officeHours);
                    lblProcessingSiteContactNumber.text(branchDetail.contactNumber);
                    lblProcessingSiteEmail.text(branchDetail.email);
                    addressInput.val(branchDetail.branchAddress);
                }
                //check if ph is selected on back btn
                let $this = $('input[type=radio][name=DocumentStatus]:checked'),
                    value = $this.val(),
                    philippinesOptions = $('option.ph');

                //siteButton.val('1');

                if (value != 'Abroad') {
                    philippinesOptions.hide();
                }
                else {
                    philippinesOptions.show();
                }
            } else {
                // The request encountered an error
                console.log("Error: " + status);
            }

        });
    };

});

