

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
        addressInput = $('#address'),
        hasExpiditeInput = $('#HasExpidite'),
        branches = [];

    getBranches();

    documentStatusRadio.change(function () {
        let $this = $(this),
            value = $this.val(),
            philippinesOptions = $('option.ph');

        siteButton.val('1');

        if (value == 'Abroad') {
            philippinesOptions.hide();
        }
        else {
            philippinesOptions.show();
        }
    });


    siteButton.on('change', function () {
        let $this = $(this),
            selectedSite = $this.val(),
            branchDetail = branches.filter(x => x.id == selectedSite)[0];


        console.log(branchDetail);
        hasExpiditeInput.val(branchDetail.hasExpidite ? 1 : 0);
        console.log(hasExpiditeInput.val());
        lblProcessingSiteAddress.text(branchDetail.branchAddress);
        lblProcessingSiteOfficeHours.text(branchDetail.officeHours);
        lblProcessingSiteContactNumber.text(branchDetail.contactNumber);
        lblProcessingSiteEmail.text(branchDetail.email);
        addressInput.val(branchDetail.branchAddress);
    });

    function getBranches() {
        $.get(`${urlBase}Home/GetBranches`, function (data, status) {
            branches = data;
        });
    };

});

