# lyra-systempay-blazor
Embedded payment form Basic exemple with C# and blazor web app


[![Watch the video](https://www.peug.net/public/7TRr0nOU-q4.jpg)](https://youtu.be/7TRr0nOU-q4)


# Summary 
This solution is based on the payment solution of [Lyra](https://www.lyra.com/)
Documentation : [https://paiement.systempay.fr/doc/fr-FR/rest/V4.0/javascript/spa/](https://paiement.systempay.fr/doc/fr-FR/rest/V4.0/javascript/spa/) 

Create a new Blasor-Server project.

## 1) Key of your store (or demo key)
This source code uses test keys. You can find them here : (https://paiement.systempay.fr/doc/fr-FR/rest/V4.0/api/get_my_keys.html#je-nai-pas-de-compte-actif)

## 2) Add in _Host.cshtml
Open the /Pages/_Host.cshtml page and add this at the bottom of the body of the page:

```
<!-- systemPay section  -->
<script type="text/javascript"
    src="https://api.systempay.fr/static/js/krypton-client/V4.0/stable/kr-payment-form.min.js" 
    kr-public-key="73239078:testpublickey_Zr3fXIKKx0mLY9YNBQEan42ano2QsdrLuyb2W54QWmUJQ"
    kr-post-url-success="https://my.site.com">
</script>

<!-- theme and plugins. should be loaded in the HEAD section -->
<link rel="stylesheet" href="https://api.systempay.fr/static/js/krypton-client/V4.0/ext/classic-reset.css">
<script src="https://api.systempay.fr/static/js/krypton-client/V4.0/ext/classic.js"></script>

<script src="~/js/SystemPay.js"></script>
```

## 3) Add in appsettings.json
You must convert your **test password** or **prod** password to base64. So I converted this **73239078:testpassword_SbEbeOueaMDyg8Rtei1bSaiB5lms9V0ZDjzldGXGAnIwH** :  
```
 "SystemPay": {
    "API": "https://api.systempay.fr/api-payment/V4/Charge/CreatePayment",
    "Authorization": "NzMyMzkwNzg6dGVzdHBhc3N3b3JkX1NiRWJlT3VlYU1EeWc4UnRlaTFiU2FpQjVsbXM5VjBaRGp6bGRHWEdBbkl3SA=="
  }
```

## 4) Add a model of configuration in your projet:
/configurations/SystemPay.cs :
```
namespace Blazor_SystemPay.Configurations
{
    public class SystemPay
    {
        public string API { get; set; }
        public string Authorization { get; set; }
    }
}
```

## 5) Add a Service and its Interface
/Services/SystemPayService.cs
```
using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Blazor_SystemPay.Configurations;

namespace Blazor_SystemPay.Services
{
    public class SystemPayService : ISystemPayService
    {
        
        private HttpClient _client;
        private IOptions<SystemPay> _systemPayConfig;

        public SystemPayService(
                            HttpClient client,
                            IOptions<SystemPay> systemPayConfig)
        {
            _client = client;
            _systemPayConfig = systemPayConfig;
        }

        public async Task<string> GetFormToken(string JSON_Order)
        {
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", _systemPayConfig.Value.Authorization);

            var data = new StringContent(JSON_Order, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(_systemPayConfig.Value.API, data);

            var res = await response.Content.ReadAsStringAsync();

            var json = System.Text.Json.JsonDocument.Parse(res);
            return json.RootElement.GetProperty("answer").GetProperty("formToken").ToString();
        }
    }
}
```

and /Services/ISystemPayService.cs
```
using System;
using System.Threading.Tasks;

namespace Blazor_SystemPay.Services
{
    public interface ISystemPayService
    {
        Task<string> GetFormToken(string JSON_Order);
    }
}
```

## 6) Add in Startup.cs :
In ConfigurationServices, add :
```
services.AddSingleton<HttpClient>();
services.AddScoped<ISystemPayService, SystemPayService>();
services.Configure<SystemPay>(Configuration.GetSection("SystemPay"));
```

## 7) Add this javascript file
Add **SystemPay.js** in www/root/js/
```
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

        // Show success message
        document.getElementById("paymentSuccessful").style.display = "block";

    } else {
        // Show error message to the user
        alert("Payment failed !");
    }
}
```

## 8) In your razor payment page

```
<!--Hidden payment form -->
<div id="paymentForm" class="kr-embedded" style="display:none">

    <!-- payment form fields -->
    <div class="kr-pan"></div>
    <div class="kr-expiry"></div>
    <div class="kr-security-code"></div>

    <!-- payment form submit button -->
    <button class="kr-payment-button"></button>

    <!-- error zone -->
    <div class="kr-form-error"></div>
</div>
<div id="kr-answer"></div>
<div id="customerror"></div>
```

## 9) and this code

```
protected override async Task OnAfterRenderAsync(bool firstRender)
  {
      if (firstRender)
      {
          string jsonRequest = @"{
                      ""orderId"": 23720,
                      ""amount"": 20000,
                      ""currency"": ""EUR"",
                      ""customer"": {
                          ""email"": ""sample@example.com"",
                          ""billingDetails"":
                          {
                              ""firstName"": ""François"",
                              ""lastName"": ""TestLyra"",
                              ""phoneNumber"" : ""11223344556677""
                          }
                      }
                  }";

          string formToken = await systemPayService.GetFormToken(jsonRequest);
          await jsRuntime.InvokeVoidAsync("displayPaymentForm", formToken);
      }
  }
```
