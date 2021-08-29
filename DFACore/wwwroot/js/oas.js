let siteAndScheduleSelection = $('#step-select-processing-site');
let applicationTypeSelection = $('#applicationTypeSelection');
let siteAndScheduleButton = $('#stepTwoNextBtn');

applicationTypeSelection.hide();

siteAndScheduleButton.on('click', function () {

    siteAndScheduleSelection.hide();
    applicationTypeSelection.show();
});