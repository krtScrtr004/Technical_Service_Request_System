$(".alpha").keypress(function (event) {
    var inputValue = event.which;

    // If digits or not a space then don't let keypress work.
    if ((inputValue > 47 && inputValue < 58) && (inputValue != 32)) {
        event.preventDefault();
    }
});