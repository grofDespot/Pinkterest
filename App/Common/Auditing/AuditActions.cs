namespace Pinkterest.Application.Common.Auditing;

public static class AuditActions
{
    public const string Register = "user.register";
    public const string Login = "user.login";
    public const string LoginFailed = "user.login.failed";
    public const string Logout = "user.logout";
    public const string PhotoUpload = "photo.upload";
    public const string PhotoEdit = "photo.edit";
    public const string PhotoDownload = "photo.download";
    public const string PhotoView = "photo.view";
    public const string PhotoSearch = "photo.search";
    public const string PackageChangeRequested = "package.change.requested";
    public const string PackageChangeApplied = "package.change.applied";
}
