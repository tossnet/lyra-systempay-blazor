$(document).ready(function () {
    KR.onError(function (event) {
        var code = event.errorCode;
        var message = event.detailedErrorMessage;
        var myMessage = code + ": " + message;

        document.getElementById("customerror").innerText = myMessage;
    });
});

function displayPaymentForm(formToken) {
    // Show the payment form
    document.getElementById('paymentForm').style.display = 'block';

    // Set form token
    KR.setFormToken(formToken);

    // Add listener for submit event
    KR.onSubmit(onPaid);
}


function onPaid(event) {
    if (event.clientAnswer.orderStatus === "PAID") {
        // Remove the payment form
        KR.removeForms();

        document.getElementById('paymentForm').style.display = 'none';

        var result = DotNet.invokeMethodAsync("Blazor-SystemPay", "PaymentValidated", event.clientAnswer);

        // Show success message
        document.getElementById("paymentSuccessful").style.display = "block";


    } else {
        // Show error message to the user
        alert("Payment failed !");
    }
}