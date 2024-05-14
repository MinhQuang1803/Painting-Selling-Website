namespace SEP490_G28_SP24_WebSellingPaint.FunctionCode.EmailSender
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
