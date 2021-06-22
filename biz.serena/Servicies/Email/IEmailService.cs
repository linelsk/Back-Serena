using biz.serena.Models.Email;

namespace biz.serena.Servicies
{
    public interface IEmailService
    {
        string SendEmail(EmailModel email);
        string SendEmailAttach(EmailModelAttach email);
    }
}
