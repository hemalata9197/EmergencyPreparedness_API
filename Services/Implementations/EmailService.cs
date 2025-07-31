using EmergencyManagement.Models.DTOs;
using EmergencyManagement.Models.Entities;
using EmergencyManagement.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using EmergencyManagement.Data;
using EmergencyManagement.Models.DTOs.Common;
using EmergencyManagement.Models.Entities.Email;


namespace EmergencyManagement.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly EHSDbContext _context;
        private readonly EmailTemplateService _templateService;
        private readonly IMastersService _MastersService;
        private readonly ISubmitReportService _submitReportService;
        private readonly ITaskService _TaskService;

        public EmailService(IOptions<EmailSettings> emailOptions, EHSDbContext context, EmailTemplateService templateService, IMastersService MastersService, ISubmitReportService submitReportService, ITaskService taskService)
        {
            _emailSettings = emailOptions.Value;
            _context = context;
            _templateService = templateService;
            _MastersService = MastersService;
            _submitReportService = submitReportService;
            _TaskService = taskService;
        }

        public async Task SendMailFireDrillAfterSubmissionAsync(int fireDrillId, int UnitId)
        {

            var fireDrill = await _submitReportService.GetFireDrillsformailAsync(fireDrillId);
            if (fireDrill == null)
                return;

            var taskDetails = await _TaskService.GetAllTaskForMailAsync(fireDrillId);
            if (taskDetails == null)
                return;


            //var CCEmailIds = "abcd@gmail.com, xyz@example.com, test.user@domain.com";

            string htmlHeader = _templateService.BuildHeader("User");
            string htmlFooter = _templateService.BuildFooter();

            string taskTable = @"
                <tr><td>
                <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                <tr style='background-color:#d9edf7;'><th>Sr.No.</th><th>Recommendation</th><th>Responsible Person</th><th>Target Date</th></tr>";

            int index = 1;
            foreach (var task in taskDetails)
            {
                taskTable += $@"
                 <tr>
                    <td>{index}</td>
                    <td>{task.TaskDetails}</td>
                    <td>{task.ResponsiblePerson}</td>
                    <td>{(task.TargetDate.HasValue ? task.TargetDate.Value.ToString("dd.MMM.yyyy") : "-")}</td>
                </tr>";
                index++;
            }

            taskTable += "</table></td></tr>";

            string statusMessage = fireDrill.IsReview
                    ? " <tr><td><p>  This is to inform you that a Fire Drill has been Reviewed.</p></td></tr>" +
                   "<tr><td><p> Please find the details below for Released.</p></td></tr>"
                    : " <tr><td><p>  This is to inform you that a Fire Drill has been submitted in the system.</p></td></tr>" +
                    "<tr><td><p> Please find the details below for Review.</p></td></tr>";

            string htmlBody = $@"
                  
                   {statusMessage}

                   <tr><td>
                   <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                       <tr style='background-color:#d9edf7;'>
                        <th>Sr.No.</th>
                        <th>Title</th>
                        <th>Details</th></tr>
                       <tr><td>1.</td><td>Ref. No.</td><td>{fireDrill.RefNo}</td></tr>
                       <tr><td>2.</td><td>FireDrill Date</td><td>{fireDrill.FireDrillDate:dd.MMM.yyyy}</td></tr>
                       <tr><td>3.</td><td>Time</td><td>{fireDrill.Time}</td></tr>
                       <tr><td>4.</td><td>Area</td><td>{fireDrill.AreaName}</td></tr> 
                       <tr><td>5.</td><td>Section</td><td>{fireDrill.SectionName}</td></tr>
                       <tr><td>6.</td><td>Scenario</td><td>{fireDrill.ScenarioName}</td></tr>
                      {(fireDrill.IsReview == true ? $"<tr><td>7.</td><td>Review Remark</td><td>{fireDrill.ReviewRemark}</td></tr>" : "")}
                   </table>
                   </td></tr>
                {taskTable}";

            string fullHtml = "<html><body><table width='100%'>" + htmlHeader + htmlBody + htmlFooter + "</table></body></html>";
            // Send mail

            var subject = fireDrill.IsReview == true
                    ? $"Fire Drill Review - Ref No: {fireDrill.RefNo ?? "(Auto)"}"
                    : $"Fire Drill Submitted - Ref No: {fireDrill.RefNo ?? "(Auto)"}";
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail),
                Subject = subject,
                Body = fullHtml,
                IsBodyHtml = true
            };
            List<string> ToEmailIds = new();
            List<string> CCEmailIds = new();
            if (fireDrill.IsReview == true)
            {
                int configElementId = 1;

                var ValuIds = await _MastersService.CheckHODEHSAsync(configElementId, UnitId);

                var valueIds = ValuIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => int.Parse(id.Trim()))
                        .ToList();

                ToEmailIds = await _context.Employees
                    .Where(e => valueIds.Contains(e.Id) && !string.IsNullOrEmpty(e.Email))
                    .Select(e => e.Email)
                    .ToListAsync();

                if (fireDrill.ReviewBy > 0)
                {
                    var ReviewByEmail = await _context.Employees
                        .Where(e => e.Id == fireDrill.ReviewBy && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(ReviewByEmail))
                    {
                        CCEmailIds.Add(ReviewByEmail);
                    }
                }


            }
            else
            {
                if (fireDrill.AreaHOD > 0)
                {
                    var hodEmail = await _context.Employees
                        .Where(e => e.Id == fireDrill.AreaHOD && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(hodEmail))
                    {
                        ToEmailIds.Add(hodEmail);
                    }
                }
                if (fireDrill.SubmittedBy > 0)
                {
                    var SubmittedByEmail = await _context.Employees
                        .Where(e => e.Id == fireDrill.SubmittedBy && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(SubmittedByEmail))
                    {
                        CCEmailIds.Add(SubmittedByEmail);
                    }
                }
            }

            var distinctToEmails = ToEmailIds.Distinct().ToList();
            var distinctCcEmails = CCEmailIds.Distinct().ToList();

            // ✅ Remove overlap: if email exists in TO, remove from CC
            distinctCcEmails = distinctCcEmails
                .Where(cc => !distinctToEmails.Contains(cc, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // ✅ Add to MailMessage
            foreach (var email in distinctToEmails)
            {
                mail.To.Add(email);
            }

            foreach (var ccEmail in distinctCcEmails)
            {
                mail.CC.Add(ccEmail);
            }
            if (fireDrill.AttachmentPaths != null && fireDrill.AttachmentPaths.Any())
            {
                foreach (var filePath in fireDrill.AttachmentPaths)
                {
                    if (File.Exists(filePath))
                    {
                        mail.Attachments.Add(new Attachment(filePath));
                    }
                }
            }

            //mail.CC.Add(CCEmail);

            using var smtp = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            // Save to DB before sending
            int messageId = await SaveMailLogAsync(subject, fullHtml, UnitId, "Fire Drill", distinctToEmails, distinctCcEmails);

            try
            {
                await smtp.SendMailAsync(mail);
                await UpdateMailLogStatusAsync(messageId, "Sent");
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                await UpdateMailLogStatusAsync(messageId, "Failed", ex.Message);
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public async Task SendMailFireDrillDeletedAsync(int fireDrillId, int EmpId)
        {

            var fireDrill = await _submitReportService.GetFireDrillsformailAsync(fireDrillId);
            if (fireDrill == null)
                return;        

            string htmlHeader = _templateService.BuildHeader("User");
            string htmlFooter = _templateService.BuildFooter();         


            string htmlBody = $@"
                  
                  <tr><td><p>  This is to inform you that a Fire Drill has been Deleted.</p></td></tr>

                   <tr><td>
                   <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                       <tr style='background-color:#d9edf7;'>
                        <th>Sr.No.</th>
                        <th>Title</th>
                        <th>Details</th></tr>
                       <tr><td>1.</td><td>Ref. No.</td><td>{fireDrill.RefNo}</td></tr>
                       <tr><td>2.</td><td>FireDrill Date</td><td>{fireDrill.FireDrillDate:dd.MMM.yyyy}</td></tr>
                       <tr><td>3.</td><td>Time</td><td>{fireDrill.Time}</td></tr>
                       <tr><td>4.</td><td>Area</td><td>{fireDrill.AreaName}</td></tr> 
                       <tr><td>5.</td><td>Section</td><td>{fireDrill.SectionName}</td></tr>
                       <tr><td>6.</td><td>Scenario</td><td>{fireDrill.ScenarioName}</td></tr>
                      {(fireDrill.IsReview == true ? $"<tr><td>7.</td><td>Review Remark</td><td>{fireDrill.ReviewRemark}</td></tr>" : "")}
                   </table>
                   </td></tr>
               ";

            string fullHtml = "<html><body><table width='100%'>" + htmlHeader + htmlBody + htmlFooter + "</table></body></html>";

            var subject = $"Fire Drill Deleted - Ref No: {fireDrill.RefNo ?? "(Auto)"}";
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail),
                Subject = subject,
                Body = fullHtml,
                IsBodyHtml = true
            };
            List<string> ToEmailIds = new();
            List<string> CCEmailIds = new();           
               
                    var DeletedByEmail = await _context.Employees
                        .Where(e => e.Id == EmpId && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(DeletedByEmail))
                    {
                        CCEmailIds.Add(DeletedByEmail);
                    }
               

              if (fireDrill.AreaHOD > 0)
                {
                    var hodEmail = await _context.Employees
                        .Where(e => e.Id == fireDrill.AreaHOD && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(hodEmail))
                    {
                        ToEmailIds.Add(hodEmail);
                    }
                }
                if (fireDrill.SubmittedBy > 0)
                {
                    var SubmittedByEmail = await _context.Employees
                        .Where(e => e.Id == fireDrill.SubmittedBy && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(SubmittedByEmail))
                    {
                        CCEmailIds.Add(SubmittedByEmail);
                    }
                }


            var distinctToEmails = ToEmailIds.Distinct().ToList();
            var distinctCcEmails = CCEmailIds.Distinct().ToList();

            // Remove overlap: if email exists in TO, remove from CC
            distinctCcEmails = distinctCcEmails
                .Where(cc => !distinctToEmails.Contains(cc, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Add to MailMessage
            foreach (var email in distinctToEmails)
            {
                mail.To.Add(email);
            }

            foreach (var ccEmail in distinctCcEmails)
            {
                mail.CC.Add(ccEmail);
            }


            using var smtp = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            // Save to DB before sending
            int messageId = await SaveMailLogAsync(subject, fullHtml, fireDrill.UnitId, "Fire Drill", distinctToEmails, distinctCcEmails);

            try
            {
                await smtp.SendMailAsync(mail);
                await UpdateMailLogStatusAsync(messageId, "Sent");
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                await UpdateMailLogStatusAsync(messageId, "Failed", ex.Message);
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public async Task SendMailFireDrillAfterReleasedAsync(int fireDrillId, int taskId, int unitId)
        {
            var fireDrill = await _submitReportService.GetFireDrillsformailAsync(fireDrillId);
            if (fireDrill == null)
                return;

            var task = await _TaskService.GetAllTaskByTaskIdForMailAsync(taskId);
            if (task == null)
                return;

            //var ccEmailIds = "abcd@gmail.com, xyz@example.com, test.user@domain.com";

            string htmlHeader = _templateService.BuildHeader("User");
            string htmlFooter = _templateService.BuildFooter();

            string taskTable = $@"
             <tr><td>
                <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                <tr style='background-color:#d9edf7;'>
                <th>Sr.No.</th><th>Recommendation</th><th>Responsible Person</th><th>Target Date</th>
                </tr>
                <tr>
                <td>1</td>
                <td>{task.TaskDetails}</td>
                <td>{task.ResponsiblePerson}</td>
                <td>{(task.TargetDate.HasValue ? task.TargetDate.Value.ToString("dd.MMM.yyyy") : "-")}</td>
            </tr>
            </table></td></tr>";

            string htmlBody = $@"
            <tr><td><p>This is to inform you that a task related to the Fire Drill has been assigned to you.</p></td></tr>
            <tr><td><p>You are requested to review the details below and complete the assigned task within the specified timeframe.</p></td></tr>

            <tr><td>
            <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                <tr style='background-color:#d9edf7;'>
                <th>Sr.No.</th><th>Title</th><th>Details</th>
                </tr>
                <tr><td>1.</td><td>Ref. No.</td><td>{fireDrill.RefNo}</td></tr>
                <tr><td>2.</td><td>FireDrill Date</td><td>{fireDrill.FireDrillDate:dd.MMM.yyyy}</td></tr>
                <tr><td>3.</td><td>Time</td><td>{fireDrill.Time}</td></tr>
                <tr><td>4.</td><td>Area</td><td>{fireDrill.AreaName}</td></tr>
                <tr><td>5.</td><td>Section</td><td>{fireDrill.SectionName}</td></tr>
                <tr><td>6.</td><td>Scenario</td><td>{fireDrill.ScenarioName}</td></tr>
                <tr><td>7.</td><td>Review Remark</td><td>{fireDrill.ReviewRemark}</td></tr>
            </table>
            </td></tr>
        {taskTable}";

            string fullHtml = $"<html><body><table width='100%'>{htmlHeader}{htmlBody}{htmlFooter}</table></body></html>";
            var subject = $"Fire Drill Released - Ref No: {fireDrill.RefNo ?? "(Auto)"}";
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail),
                Subject = $"Fire Drill Released - Ref No: {fireDrill.RefNo ?? "(Auto)"}",
                Body = fullHtml,
                IsBodyHtml = true
            };

            List<string> ToEmailIds = new();
            List<string> CCEmailIds = new();

            if (task.ResponsiblePersonId > 0)
            {
                var responsibleEmail = await _context.Employees
                    .Where(e => e.Id == task.ResponsiblePersonId && !string.IsNullOrEmpty(e.Email))
                    .Select(e => e.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(responsibleEmail))
                {
                    ToEmailIds.Add(responsibleEmail);
                }
            }
            if (fireDrill.ReleasedBy > 0)
            {
                var ReleasedByEmail = await _context.Employees
                    .Where(e => e.Id == fireDrill.ReleasedBy && !string.IsNullOrEmpty(e.Email))
                    .Select(e => e.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(ReleasedByEmail))
                {
                    CCEmailIds.Add(ReleasedByEmail);
                }
            }
            var distinctToEmails = ToEmailIds.Distinct().ToList();
            var distinctCcEmails = CCEmailIds.Distinct().ToList();

            // ✅ Remove overlap: if email exists in TO, remove from CC
            distinctCcEmails = distinctCcEmails
                .Where(cc => !distinctToEmails.Contains(cc, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // ✅ Add to MailMessage
            foreach (var email in distinctToEmails)
            {
                mail.To.Add(email);
            }

            foreach (var ccEmail in distinctCcEmails)
            {
                mail.CC.Add(ccEmail);
            }
            if (fireDrill.AttachmentPaths != null && fireDrill.AttachmentPaths.Any())
            {
                foreach (var filePath in fireDrill.AttachmentPaths)
                {
                    if (File.Exists(filePath))
                    {
                        mail.Attachments.Add(new Attachment(filePath));
                    }
                }
            }

            using var smtp = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            // Save to DB before sending
            int messageId = await SaveMailLogAsync(subject, fullHtml, unitId, "Fire Drill", distinctToEmails, distinctCcEmails);

            try
            {
                await smtp.SendMailAsync(mail);
                await UpdateMailLogStatusAsync(messageId, "Sent");
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                await UpdateMailLogStatusAsync(messageId, "Failed", ex.Message);
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public async Task SendMailOnFireDrillTaskStatusUpdationAsync(int fireDrillId, int taskId)
        {
            var fireDrill = await _submitReportService.GetFireDrillsformailAsync(fireDrillId);
            if (fireDrill == null)
                return;

            var task = await _TaskService.GetAllTaskDeatilsAfterStatusUpdationByTaskIdForMailAsync(taskId);
            if (task == null)
                return;

            string htmlHeader = _templateService.BuildHeader("User");
            string htmlFooter = _templateService.BuildFooter();

            // Conditional Extended Target Date header and value
            string extendedTh = task.TaskStatusId == 3 ? "<th>Extended Target Date</th>" : "";
            string extendedTd = task.TaskStatusId == 3
                ? $"<td>{(task.ExtendedTargetDate.HasValue ? task.ExtendedTargetDate.Value.ToString("dd.MMM.yyyy") : "-")}</td>"
                : "";

            string taskTable = $@"
            <tr><td>
            <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                <tr style='background-color:#d9edf7;'>
                    <th>Sr.No.</th>
                    <th>Recommendation</th>
                    <th>Responsible Person</th>
                    <th>Target Date</th>
                    <th>Task Status</th>
                    <th>Remarks</th>
                    {extendedTh}
                </tr>
                <tr>
                    <td>1</td>
                    <td>{task.TaskDetails}</td>
                    <td>{task.ResponsiblePerson}</td>
                    <td>{(task.TargetDate.HasValue ? task.TargetDate.Value.ToString("dd.MMM.yyyy") : "-")}</td>
                    <td>{task.TaskStatus}</td>
                    <td>{task.Remarks}</td>
                    {extendedTd}
                </tr>
            </table>
            </td></tr>";

            string statusMessage = task.TaskStatusId == 3
        ? "<tr><td><p>The target date for the assigned task has been extended. Please review the updated deadline.</p></td></tr>"
        : "<tr><td><p>The assigned task has been marked as completed. Kindly review and provide your feedback.</p></td></tr>";

            string htmlBody = $@"
                {statusMessage}

            <tr><td>
                <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                    <tr style='background-color:#d9edf7;'>
                        <th>Sr.No.</th><th>Title</th><th>Details</th>
                    </tr>
                    <tr><td>1.</td><td>Ref. No.</td><td>{fireDrill.RefNo}</td></tr>
                    <tr><td>2.</td><td>FireDrill Date</td><td>{fireDrill.FireDrillDate:dd.MMM.yyyy}</td></tr>
                    <tr><td>3.</td><td>Time</td><td>{fireDrill.Time}</td></tr>
                    <tr><td>4.</td><td>Area</td><td>{fireDrill.AreaName}</td></tr>
                    <tr><td>5.</td><td>Section</td><td>{fireDrill.SectionName}</td></tr>
                    <tr><td>6.</td><td>Scenario</td><td>{fireDrill.ScenarioName}</td></tr>
                    <tr><td>7.</td><td>Review Remark</td><td>{fireDrill.ReviewRemark}</td></tr>
                </table>
            </td></tr>
                    {taskTable}";

            string fullHtml = $"<html><body><table width='100%'>{htmlHeader}{htmlBody}{htmlFooter}</table></body></html>";
            var subject = task.TaskStatusId == 3
                 ? $"Fire Drill- Target Date Exceded - Ref No: {fireDrill.RefNo ?? "(Auto)"}"
                 : $"Fire Drill- Task Completed - Ref No: {fireDrill.RefNo ?? "(Auto)"}";
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail),
                Subject = subject,
                Body = fullHtml,
                IsBodyHtml = true
            };

            List<string> ToEmailIds = new();
            List<string> CCEmailIds = new();
            if (task.TaskStatusId == 3)
            {
                //Inprogress
                if (task.ResponsiblePersonId > 0)
                {
                    var responsibleEmail = await _context.Employees
                        .Where(e => e.Id == task.ResponsiblePersonId && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(responsibleEmail))
                    {
                        ToEmailIds.Add(responsibleEmail);
                    }
                }
                if (task.UserAreaHead > 0)
                {
                    var UserAreaHeadEmail = await _context.Employees
                        .Where(e => e.Id == task.UserAreaHead && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(UserAreaHeadEmail))
                    {
                        CCEmailIds.Add(UserAreaHeadEmail);
                    }
                }
            }
            else
            {
                //Complete

                if (task.UserAreaHead > 0)
                {
                    var UserAreaHeadEmail = await _context.Employees
                        .Where(e => e.Id == task.UserAreaHead && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(UserAreaHeadEmail))
                    {
                        ToEmailIds.Add(UserAreaHeadEmail);
                    }
                }
                if (task.ResponsiblePersonId > 0)
                {
                    var responsibleEmail = await _context.Employees
                        .Where(e => e.Id == task.ResponsiblePersonId && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(responsibleEmail))
                    {
                        CCEmailIds.Add(responsibleEmail);
                    }
                }

            }
            // CC: ReleasedBy
            if (fireDrill.ReleasedBy > 0)
            {
                var releasedByEmail = await _context.Employees
                    .Where(e => e.Id == fireDrill.ReleasedBy && !string.IsNullOrEmpty(e.Email))
                    .Select(e => e.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(releasedByEmail))
                {
                    CCEmailIds.Add(releasedByEmail);
                }
            }

            var distinctToEmails = ToEmailIds.Distinct().ToList();
            var distinctCcEmails = CCEmailIds.Distinct().ToList();

            // Remove overlap: if email exists in TO, remove from CC
            distinctCcEmails = distinctCcEmails
                .Where(cc => !distinctToEmails.Contains(cc, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Add to MailMessage
            foreach (var email in distinctToEmails)
            {
                mail.To.Add(email);
            }

            foreach (var ccEmail in distinctCcEmails)
            {
                mail.CC.Add(ccEmail);
            }


            using var smtp = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            // Save to DB before sending
            int messageId = await SaveMailLogAsync(subject, fullHtml, fireDrill.UnitId, "Fire Drill", distinctToEmails, distinctCcEmails);

            try
            {
                await smtp.SendMailAsync(mail);
                await UpdateMailLogStatusAsync(messageId, "Sent");
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                await UpdateMailLogStatusAsync(messageId, "Failed", ex.Message);
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public async Task SendMailOnFireDrillTaskApprovalAsync(int fireDrillId, int taskId)
        {
            var fireDrill = await _submitReportService.GetFireDrillsformailAsync(fireDrillId);
            if (fireDrill == null)
                return;

            var task = await _TaskService.GetAllTaskDeatilsAfterStatusUpdationByTaskIdForMailAsync(taskId);
            if (task == null)
                return;

            string htmlHeader = _templateService.BuildHeader("User");
            string htmlFooter = _templateService.BuildFooter();           

            string taskTable = $@"
            <tr><td>
            <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                <tr style='background-color:#d9edf7;'>
                    <th>Sr.No.</th>
                    <th>Recommendation</th>
                    <th>Responsible Person</th>
                    <th>Target Date</th>
                    <th>Approval Status</th>
                    <th>Approval Remarks</th>                   
                </tr>
                <tr>
                    <td>1</td>
                    <td>{task.TaskDetails}</td>
                    <td>{task.ResponsiblePerson}</td>
                    <td>{(task.TargetDate.HasValue ? task.TargetDate.Value.ToString("dd.MMM.yyyy") : "-")}</td>
                    <td>{task.ApprovalStatus}</td>
                    <td>{task.ClosedRemark}</td>
                </tr>
            </table>
            </td></tr>";

            string approvalMessage = task.ApprovalStatusId == 2
                     ? "<tr><td><p>The task has been rejected. Please review the remarks provided and take necessary action.</p></td></tr>"
                     : "<tr><td><p>The task has been approved.</p></td></tr>";

            string htmlBody = $@"
                  {approvalMessage}
            <tr><td>
                <table border='1' cellpadding='5' cellspacing='0' width='100%'>
                    <tr style='background-color:#d9edf7;'>
                        <th>Sr.No.</th><th>Title</th><th>Details</th>
                    </tr>
                    <tr><td>1.</td><td>Ref. No.</td><td>{fireDrill.RefNo}</td></tr>
                    <tr><td>2.</td><td>FireDrill Date</td><td>{fireDrill.FireDrillDate:dd.MMM.yyyy}</td></tr>
                    <tr><td>3.</td><td>Time</td><td>{fireDrill.Time}</td></tr>
                    <tr><td>4.</td><td>Area</td><td>{fireDrill.AreaName}</td></tr>
                    <tr><td>5.</td><td>Section</td><td>{fireDrill.SectionName}</td></tr>
                    <tr><td>6.</td><td>Scenario</td><td>{fireDrill.ScenarioName}</td></tr>
                    <tr><td>7.</td><td>Review Remark</td><td>{fireDrill.ReviewRemark}</td></tr>
                </table>
            </td></tr>
                    {taskTable}";

            string fullHtml = $"<html><body><table width='100%'>{htmlHeader}{htmlBody}{htmlFooter}</table></body></html>";
            var subject = task.ApprovalStatusId == 2
                 ? $"Fire Drill- Task Approval-Rejected - Ref No: {fireDrill.RefNo ?? "(Auto)"}"
                 : $"Fire Drill- Task Approval-Approved - Ref No: {fireDrill.RefNo ?? "(Auto)"}";
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail),
                Subject = subject,
                Body = fullHtml,
                IsBodyHtml = true
            };

            List<string> ToEmailIds = new();
            List<string> CCEmailIds = new();
            if (task.ApprovalStatusId == 2)
            {
                //Inprogress
                if (task.ResponsiblePersonId > 0)
                {
                    var responsibleEmail = await _context.Employees
                        .Where(e => e.Id == task.ResponsiblePersonId && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(responsibleEmail))
                    {
                        ToEmailIds.Add(responsibleEmail);
                    }
                }
                if (task.UserAreaHead > 0)
                {
                    var UserAreaHeadEmail = await _context.Employees
                        .Where(e => e.Id == task.UserAreaHead && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(UserAreaHeadEmail))
                    {
                        CCEmailIds.Add(UserAreaHeadEmail);
                    }
                }
                // CC: ReleasedBy
                if (fireDrill.ReleasedBy > 0)
                {
                    var releasedByEmail = await _context.Employees
                        .Where(e => e.Id == fireDrill.ReleasedBy && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(releasedByEmail))
                    {
                        CCEmailIds.Add(releasedByEmail);
                    }
                }
            }
            else
            {
                //Complete
                if (fireDrill.ReleasedBy > 0)
                {
                    var releasedByEmail = await _context.Employees
                        .Where(e => e.Id == fireDrill.ReleasedBy && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(releasedByEmail))
                    {
                        ToEmailIds.Add(releasedByEmail);
                    }
                }

                if (task.UserAreaHead > 0)
                {
                    var UserAreaHeadEmail = await _context.Employees
                        .Where(e => e.Id == task.UserAreaHead && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(UserAreaHeadEmail))
                    {
                        CCEmailIds.Add(UserAreaHeadEmail);
                    }
                }
                if (task.ResponsiblePersonId > 0)
                {
                    var responsibleEmail = await _context.Employees
                        .Where(e => e.Id == task.ResponsiblePersonId && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(responsibleEmail))
                    {
                        CCEmailIds.Add(responsibleEmail);
                    }
                }
                if (fireDrill.ReviewBy > 0)
                {
                    var ReviewByEmail = await _context.Employees
                        .Where(e => e.Id == fireDrill.ReviewBy && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(ReviewByEmail))
                    {
                        CCEmailIds.Add(ReviewByEmail);
                    }
                }
                if (fireDrill.SubmittedBy > 0)
                {
                    var SubmittedByEmail = await _context.Employees
                        .Where(e => e.Id == fireDrill.SubmittedBy && !string.IsNullOrEmpty(e.Email))
                        .Select(e => e.Email)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(SubmittedByEmail))
                    {
                        CCEmailIds.Add(SubmittedByEmail);
                    }
                }

            }

            var distinctToEmails = ToEmailIds.Distinct().ToList();
            var distinctCcEmails = CCEmailIds.Distinct().ToList();

            // Remove overlap: if email exists in TO, remove from CC
            distinctCcEmails = distinctCcEmails
                .Where(cc => !distinctToEmails.Contains(cc, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Add to MailMessage
            foreach (var email in distinctToEmails)
            {
                mail.To.Add(email);
            }

            foreach (var ccEmail in distinctCcEmails)
            {
                mail.CC.Add(ccEmail);
            }


            using var smtp = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            // Save to DB before sending
            int messageId = await SaveMailLogAsync(subject, fullHtml, fireDrill.UnitId, "Fire Drill", distinctToEmails, distinctCcEmails);

            try
            {
                await smtp.SendMailAsync(mail);
                await UpdateMailLogStatusAsync(messageId, "Sent");
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                await UpdateMailLogStatusAsync(messageId, "Failed", ex.Message);
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public async Task<bool> SendMailForgotPasswordAsync(ForgotPasswordDto request)
        {

            var user = await (from e in _context.Employees
                              join u in _context.Users on e.EmployeeCode equals u.Username
                              where u.Username == request.Username && e.Email == request.Email
                              select u)
                  .FirstOrDefaultAsync();

            if (user == null)
                return false; // User not found or email mismatch
            string resetLink = $"http://localhost:4200/reset-password?username={request.Username}";

            string htmlHeader = _templateService.BuildHeader("User");
            string htmlFooter = _templateService.BuildFooter();

            string htmlBody = $@"
        <tr><td><p>We received a request to reset your password.</p></td></tr>
        <tr><td><p>Click the link below to reset your password:</p></td></tr>
        <tr><td><a href='{resetLink}' style='color:blue'>{resetLink}</a></td></tr>
        <tr><td><p>If you did not request a password reset, you can ignore this email.</p></td></tr>";

            string fullHtml = "<html><body><table width='100%'>" + htmlHeader + htmlBody + htmlFooter + "</table></body></html>";
            var subject = "Login Credentials";
            //  4. Create and send email
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail),
                Subject = subject,
                Body = fullHtml,
                IsBodyHtml = true
            };
            List<string> ToEmailIds = new();
            List<string> CCEmailIds = new();
            ToEmailIds.Add(request.Email);

            var distinctToEmails = ToEmailIds.Distinct().ToList();
            var distinctCcEmails = CCEmailIds.Distinct().ToList();
          
            foreach (var email in distinctToEmails)
            {
                mail.To.Add(request.Email);
            }        


            using var smtp = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            // Save to DB before sending
            int messageId = await SaveMailLogAsync(subject, fullHtml, 0, "Fire Drill", distinctToEmails, distinctCcEmails);

            try
            {
                await smtp.SendMailAsync(mail);
                await UpdateMailLogStatusAsync(messageId, "Sent");               
                Console.WriteLine("Email sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                await UpdateMailLogStatusAsync(messageId, "Failed", ex.Message);
                Console.WriteLine("Error sending forgot password email: " + ex.Message);
                return false;
            }           
        }

        private async Task<int> SaveMailLogAsync(string subject, string body, int unitId, string fromMod, List<string> toEmails, List<string> ccEmails)
        {
            var mailLog = new MailMessages
            {
                FromMailId = _emailSettings.FromEmail,
                Subject = subject,
                Body = body,
                SendStatus = "Pending",
                UnitId = unitId,
                FromMod = fromMod,
                MailMessagesToUsers = toEmails.Select(e => new MailMessagesToUser
                {
                    MailId = e,
                    MailIdStatus = "Pending",
                    ConfCat = "to" // Mark as TO
                })
        .Concat(ccEmails.Select(e => new MailMessagesToUser
        {
            MailId = e,
            MailIdStatus = "Pending",
            ConfCat = "cc"  // Mark as CC
        }))
        .ToList()
            };

            _context.MailMessages.Add(mailLog);
            await _context.SaveChangesAsync();
            return mailLog.MessageId;
        }

        private async Task UpdateMailLogStatusAsync(int messageId, string status, string exceptionMessage = null)
        {
            var mailLog = await _context.MailMessages
                .Include(m => m.MailMessagesToUsers)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);

            if (mailLog != null)
            {
                mailLog.SendStatus = status;
                mailLog.ExceptionMessage = exceptionMessage;

                foreach (var userMail in mailLog.MailMessagesToUsers)
                {
                    userMail.MailIdStatus = status;
                    userMail.ExceptionMessage = exceptionMessage;
                }

                await _context.SaveChangesAsync();
            }
        }


    }
}