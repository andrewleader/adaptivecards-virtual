var appId = 'dab46e29dd888460ba8d1938bb41b062';
var zip = '98052';

//setData({
//    testing: true
//});

var forecast = JSON.parse(getBlocking('https://api.openweathermap.org/data/2.5/weather?zip=' + zip + '&units=imperial&appid=' + appId));

setData({
    forecast: forecast
});