using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;

namespace fnConsole
{
    public class SendMail : FileNotify2.IScript
    {
        public void SendTheMail(string subject)
        {
            SmtpClient smtp = new SmtpClient("smtp.numericable.fr"); // SMTP Server address
            MailMessage message = new MailMessage(new MailAddress("from@xtware.com"), new MailAddress("to@xtware.com"));
            message.Subject = subject;
            message.Body = "This mail is empty";
            smtp.Send(message);
        }

        public override void DeletedFile(FileNotify2.DirectoryPicture.Win32FindData file)
        {
            SendTheMail(string.Format("The file {0}\\{1} has been deleted", m_directory, file.cFileName));
        }

        public override void NewFile(FileNotify2.DirectoryPicture.Win32FindData file)
        {
            SendTheMail(string.Format("The file {0}\\{1} appeared", m_directory, file.cFileName));
        }

        public override void ChangedFile(FileNotify2.DirectoryPicture.Win32FindData before, FileNotify2.DirectoryPicture.Win32FindData now)
        {
            SendTheMail(string.Format("The file {0}\\{1} changed", m_directory, before.cFileName));
        }

        public override void IdenticalFile(FileNotify2.DirectoryPicture.Win32FindData file)
        {
            // Move the file
            System.IO.File.Move(m_directory + "\\" + file.cFileName, @"c:\temp\" + file.cFileName);
        }
    }
}
