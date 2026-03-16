namespace PeopleDirectoryApplication.Models;

public enum EmailNotificationJobStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    DeadLetter = 3,
}
