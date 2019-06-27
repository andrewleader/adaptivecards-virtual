function addPassenger() {
    var passengers = getData().passengers;
    passengers.push({});

    setData({
        passengers: passengers
    });
}