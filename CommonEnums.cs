namespace NugetEmailDemo.Utility
{
    public class CommonEnums
    {
        public enum ConfigKeys
        {
            EmailServiceProvider, // This comprises of the third party emailing service provider. i.e. currently we are using sendgrid emailing service.
            SendgridApiKey, // Sendgrid api key which serves as credential to send email using sendgrid server.
            AmazonAccessKeyID, // Amazon access key id
            AmazonSecretKey // Amazon secret key
        }
    }
}
