

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
        addressInput = $('#address')
        branches = [];

    getBranches();
    init();

    documentStatusRadio.change(function () {
        let $this = $(this),
            value = $this.val(),
            philippinesOptions = $('option.ph');

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


        lblProcessingSiteAddress.text(branchDetail.branchAddress);
        lblProcessingSiteOfficeHours.text(branchDetail.officeHours);
        lblProcessingSiteContactNumber.text(branchDetail.contactNumber);
        lblProcessingSiteEmail.text(branchDetail.email);
        addressInput.val(branchDetail.branchAddress);
    });

    function init() {
        documentStatusRadio.val('Abroad');
        documentTypeRadio.val('Document Owner');
    }

    function getBranches() {
        $.get(`${urlBase}Home/GetBranches`, function (data, status) {
            branches = data;
        });
    };

});

