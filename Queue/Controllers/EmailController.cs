using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Queue.Controllers
{
    [Authorize(Roles = "SAdmin,Admin,Manager,Employer,Employee")]
    public class EmailController : Controller
    {
        // GET: Email
        public ActionResult Index()
        {
            return View();
        }

        #region Register
        public async Task<bool> SendInvitation(List<string> email, string login, string pass)
        {
            try
            {
                await SendMail(email, SetEmailBody(email, login, pass), ConfigurationManager.AppSettings["EmailSubjec"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        private AlternateView SetEmailBody(List<string> email, string login, string pass)
        {
            try
            {
                //se arma el correo que se envia para el ambio de clave
                string ruta = ConfigurationManager.AppSettings["WelcomeTemplate"];

                string plantilla = Path.Combine(HttpRuntime.AppDomainAppPath, ruta);

                var html = System.IO.File.ReadAllText(plantilla);

                html = html.Replace("{{user}}", email[0]);
                html = html.Replace("{{login}}", login);
                html = html.Replace("{{pass}}", pass);

                AlternateView av = AlternateView.CreateAlternateViewFromString(html, null, "text/html");

                return av;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Forgot
        public async void SendForgot(List<string> email, string callback)
        {
            await SendMail(email, SetForgotEmailBody(email, callback), ConfigurationManager.AppSettings["EmailSubjectForgot"]);
        }
        private AlternateView SetForgotEmailBody(List<string> email, string callback)
        {
            try
            {
                //se arma el correo que se envia para el ambio de clave
                string ruta = ConfigurationManager.AppSettings["ForgotTemplate"];

                string plantilla = Path.Combine(HttpRuntime.AppDomainAppPath, ruta);

                var html = System.IO.File.ReadAllText(plantilla);

                html = html.Replace("{{user}}", email[0]);
                html = html.Replace("{{callback}}", callback);

                AlternateView av = AlternateView.CreateAlternateViewFromString(html, null, "text/html");

                return av;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Alerts
        public async void SendAlert(List<string> email, int type, List<string> users)
        {
            await SendMail(email, SeetAlertEmail(email, type, users), "Alertas MonitorTracker");
        }

        private AlternateView SeetAlertEmail(List<string> email, int type, List<string> users)
        {
            try
            {
                //se arma el correo que se envia para el ambio de clave
                string ruta = ConfigurationManager.AppSettings["AlertsTemplate"];

                string plantilla = Path.Combine(HttpRuntime.AppDomainAppPath, ruta);

                var html = System.IO.File.ReadAllText(plantilla);
                string type_ = string.Empty;
                switch (type)
                {
                    case 2:
                        type_ = "APLIACIONES IMPRODUCTIVAS CON MAS DE 30 MIN DE USO";
                        break;
                    case 3:
                        type_ = "MAS DE 30 MIN DE INACTIVIDAD ACUMULADA";
                        break;
                    case 4:
                        type_ = "NO REPORTA DESDE HACE MAS DE 30 MIN";
                        break;

                    default:
                        break;
                }
                html = html.Replace("{{type}}", type_);

                string userlist = string.Empty;

                foreach (var u in users)
                {
                    userlist = userlist + "<tr><td>" + u + "</td></tr>";
                }

                html = html.Replace("{{userlist}}", userlist);

                AlternateView av = AlternateView.CreateAlternateViewFromString(html, null, "text/html");

                return av;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        private async Task<bool> SendMail(List<string> toAddress, AlternateView emailbody, string Subject)
        {
            try
            {
                var userCredentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPUserName"].ToString(), ConfigurationManager.AppSettings["SMTPPassword"]);
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                SmtpClient smtp = new SmtpClient
                {
                    Host = Convert.ToString(ConfigurationManager.AppSettings["SMTPHost"]),
                    Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]),
                    EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SMTPEnableSsl"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPTimeout"])
                };
                smtp.Credentials = userCredentials;
                //smtp.UseDefaultCredentials = true;
                smtp.EnableSsl = true;

                MailMessage message = new MailMessage();
                message.From = new MailAddress(ConfigurationManager.AppSettings["SenderEmailAddress"], ConfigurationManager.AppSettings["SenderDisplayName"]);
                message.Subject = Subject;
                message.IsBodyHtml = true;
                message.AlternateViews.Add(emailbody);
                message.IsBodyHtml = true;

                //message.To.Add(email);
                foreach (var m in toAddress)
                {
                    message.To.Add(m);
                };

                smtp.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}