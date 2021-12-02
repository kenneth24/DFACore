let philippinesButton = $('#documentLocation #philippines-button');
let abroadButton = $('#documentLocation #abroad-button');

philippinesButton.on('click', function () {
    window.location = `${urlBase}Home/Index?id=0`//'/Home/Index?id=0'
});

abroadButton.on('click', function () {
    window.location = `${urlBase}Home/Index?id=1`
});
