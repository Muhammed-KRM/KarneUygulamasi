namespace KeremProject1backend.Core.Constants;

public static class ErrorCodes
{
    // System & Global Errors (100XXX)
    public const string Unauthorized = "100000"; // No session / Token invalid
    public const string AccessDenied = "100403"; // Forbidden / Role mismatch
    public const string InternalError = "100500";
    public const string ValidationFailed = "100400";

    // Feature 001: Auth & User Operations
    public const string AuthUsernameTaken = "001001";
    public const string AuthEmailTaken = "001002";
    public const string AuthInvalidCredentials = "001003";
    public const string AuthUserSuspended = "001004";
    public const string AuthUserDeleted = "001005";
    public const string AuthInstitutionAlreadyApplied = "001006";
    public const string AuthLicenseNumberTaken = "001007";
    public const string AuthUserNotFound = "001008"; // Add this line if missing

    // Feature 002: Admin Operations
    public const string AdminInstitutionNotFound = "002001";
    public const string AdminInstitutionNotPending = "002002";
}
