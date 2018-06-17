$(document).ready(function () {
    updateOnlineTimer();
    updateSpeedTimer();
    console.log('Overdrive.js loaded');
});


let overdriveInterval = 10;
let overdriveInterval2 = 5;
let numberOfCars = 7;
const oneSecond = 1000;

function convertSpeed(s) {
    return Math.round(s / 10) + "," + Math.round(s % 10);
}

function updateOnlineTimer() {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState === XMLHttpRequest.DONE) {   // XMLHttpRequest.DONE == 4
            if (xmlhttp.status === 200) {
                let cars = JSON.parse(xmlhttp.responseText);
                updateOnlineOverdrive(cars);
            }
            else if (xmlhttp.status === 400) {
                alert('There was an error 400');
            }
            else {
                //alert('something else other than 200 was returned: ' + xmlhttp.status);
            }
        }
    };
    if (localStorage.getItem("apiAddress") !== null) {
        xmlhttp.open("GET", localStorage.getItem("apiAddress")  + "/api/read/car", true);
        xmlhttp.send();
    }
}

function updateSpeedTimer() {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState === XMLHttpRequest.DONE) {   // XMLHttpRequest.DONE == 4
            if (xmlhttp.status === 200) {
                let cars = JSON.parse(xmlhttp.responseText);
                updateSpeedOverdrive(cars);
            }
            else if (xmlhttp.status === 400) {
                alert('There was an error 400');
            }
            else {
                //alert('something else other than 200 was returned: ' + xmlhttp.status);
            }
        }
    };
    if (localStorage.getItem("apiAddress") !== null) {
        xmlhttp.open("GET", localStorage.getItem("apiAddress")  + "/api/read/car", true);
        xmlhttp.send();
    }
}
function updateOnline(car) {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.open("PUT", localStorage.getItem("apiAddress") + '/api/write/car/online/' + car.carId, true);
    xmlhttp.setRequestHeader('Content-type', 'application/json; charset=utf-8');
    xmlhttp.send(JSON.stringify(car));
}
function updateSpeed(car) {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.open("PUT", localStorage.getItem("apiAddress") + '/api/write/car/speed/' + car.carId, true);
    xmlhttp.setRequestHeader('Content-type', 'application/json; charset=utf-8');
    xmlhttp.send(JSON.stringify(car));
}
function updateOnlineOverdrive(cars) {
    if (localStorage.getItem("apiAddress") === null) {
        setTimeout(updateOnlineTimer, oneSecond);
        console.log("No apiAddress found");
        return;
    }
    if (cars.length === 0) {
        setTimeout(updateOnlineTimer, oneSecond);
        console.log("No vehicles found!");
        return;
    }
    numberOfCars = cars.length;
    const selectedItem = Math.floor(Math.random() * cars.length);
    let selectedCar = cars[selectedItem];
    if (selectedCar.locked === true) {
        console.log(selectedCar.regNr + " is Locked for uppdating of Online/Offline!");
        overdriveInterval = Math.round(overdriveInterval / numberOfCars);
        setTimeout(updateOnlineTimer, overdriveInterval);
        return;
    }
    selectedCar.online = !selectedCar.online;
    updateOnline(selectedCar)
    var onlineStr = (selectedCar.online) ? 'online' : 'offline';
    document.getElementById("infoText").innerHTML = 'Car ' + selectedCar.regNr + ' is ' + onlineStr;
    let interval = Math.round(overdriveInterval / numberOfCars);
    setTimeout(updateOnlineTimer, interval);
}

function updateSpeedOverdrive(cars) {
    if (localStorage.getItem("apiAddress") === null) {
        setTimeout(updateSpeedTimer, oneSecond);
        console.log("No apiAddress found");
        return;
    }
    if (cars.length === 0) {
        setTimeout(updateSpeedTimer, oneSecond);
        console.log("No vehicles found!");
        return;
    }
    numberOfCars = cars.length;
    const selectedItem = Math.floor(Math.random() * cars.length);
    let selectedCar = cars[selectedItem];
    if (selectedCar.locked === true) {
        console.log(selectedCar.regNr + " is Locked for uppdating of Online/Offline!");
        let interval = Math.round(overdriveInterval2 / numberOfCars);
        setTimeout(updateSpeedTimer, interval);
        return;
    }
    const delta = selectedCar.speed / 10;
    selectedCar.speed = selectedCar.speed + Math.round(delta / 2 - Math.floor(Math.random() * delta));
    updateSpeed(selectedCar);
    document.getElementById("infoText").innerHTML = 'Car ' + selectedCar.regNr + ' has speed ' + selectedCar.speed/10 + ' km/h';
    overdriveInterval2 = Math.round(overdriveInterval2 / numberOfCars);
    setTimeout(updateSpeedTimer, overdriveInterval2);
}

