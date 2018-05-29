using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Utilities
{
    public class Mail
    {
        MailConfig mcf;

        public Mail(String SMTPServer, Int32 ServerPort, String mailAccount, String mailAccountPass, Boolean SSL)
        {
            mcf = new MailConfig();
            mcf.ServerSMTP = SMTPServer;
            mcf.portSMTP = ServerPort;
            mcf.Account = mailAccount;
            mcf.AccountPass = mailAccountPass;
            mcf.useSSL = SSL;
        }

        public String sendCustomMail(String subject, String body, String destinationMail)
        {
            String collector = String.Empty;
            try
            {
                MailMessage message = new MailMessage();

                message.To.Add(new MailAddress(destinationMail));
                message.From = new MailAddress(mcf.Account);
                message.Subject = subject;
                message.Body = body;

                SmtpClient server = new SmtpClient(mcf.ServerSMTP, mcf.portSMTP);
                server.Credentials = new NetworkCredential(mcf.Account, mcf.AccountPass);
                server.EnableSsl = mcf.useSSL;

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
                server.Send(message);
            }
            catch (System.Net.Mail.SmtpException x) { collector = String.Format("sendCustomMail > SmtpException [{0}] \n\n ExceptionDescription -> {1}", x.Message, x.StackTrace); }
            catch (Exception e) { collector = String.Format("sendCustomMail > SystemException [{0}] \n\n ExceptionDescription -> {1}", e.Message, e.StackTrace); }

            return collector;
        }
    }
}
