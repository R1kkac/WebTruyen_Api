using System.Diagnostics;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Chat.V1;

namespace TestWebApi_v1.Service.Phone
{
    public class SmsService
    {
        public Task SendAsync(string phoneNumber)
        {
            //Chưa thể làm được do tài khoản hết tiền đăng ký số điện thoại (phải mua số điện thoại sms https://console.twilio.com/us1/develop/phone-numbers/manage/search?isoCountry=US&types[]=Local&types[]=Tollfree&capabilities[]=Sms&capabilities[]=Mms&capabilities[]=Voice&capabilities[]=Fax&searchTerm=&searchFilter=left&searchType=number)
            //string accountSid = Environment.GetEnvironmentVariable("AC4da8738fda1f2e18478321a4b66cfd45")!;
            //string authToken = Environment.GetEnvironmentVariable("7d467680f018543a0454160778b9108e")!;
            string accountSid = "AC4da8738fda1f2e18478321a4b66cfd45";
            string authToken = "7d467680f018543a0454160778b9108e";

            TwilioClient.Init(accountSid, authToken);

            var messages = MessageResource.Create(
                body: "Join Earth's mightiest heroes. Like Kevin Bacon.",
                from: new Twilio.Types.PhoneNumber("+84384539162"),
                to: new Twilio.Types.PhoneNumber("0866137749")
            );
            return Task.FromResult(messages);
            //// Twilio Begin
            //var Twilio = new TwilioRestClient(
            //  PhoneNumberKey.SMSAccountIdentification,
            //  PhoneNumberKey.SMSAccountPassword);
            //var result = Twilio.SendSms(
            //  PhoneNumberKey.SMSAccountFrom,
            //  message.Destination, message.Body
            //);
            ////Status is one of Queued, Sending, Sent, Failed or null if the number is not valid
            //Trace.TraceInformation(result.Status);
            ////wilio doesn't currently have an async API, so return success.
            //return Task.FromResult(0);
            //// Twilio End
        }
    }
}
