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

