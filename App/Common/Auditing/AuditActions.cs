namespace Pinkterest.Application.Common.Auditing;

public static class AuditActions
{
    public const string Register = "user.register";
    public const string Login = "user.login";
    public const string LoginFailed = "user.login.failed";
    public const string Logout = "user.logout";
    public const string TokenIssued = "token.issued";
    public const string TokenRefreshed = "token.refreshed";
    public const string TokenRevoked = "token.revoked";
    public const string TokenReuseDetected = "token.reuse.detected";
    public const string PhotoUpload = "photo.upload";
    public const string PhotoEdit = "photo.edit";
    public const string PhotoDownload = "photo.download";
    public const string PhotoView = "photo.view";
    public const string PhotoSearch = "photo.search";
    public const string PresetSaved = "preset.saved";
    public const string PresetDeleted = "preset.deleted";
    public const string PackageChangeRequested = "package.change.requested";
    public const string PackageChangeApplied = "package.change.applied";
}
