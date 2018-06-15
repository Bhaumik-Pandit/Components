namespace NugetEmailDemo.DTO
{
    public class EmailSendRequestDTO
    {
        public string FromAddress { get; set; }
        public string FromDisplayName { get; set; }
        public string ToAddresses { get; set; }
        public string CCAddresses { get; set; }
        public string BCCAddresses { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtmlFormat { get; set; }
        public string AttachmentFile { get; set; }
    }
}
