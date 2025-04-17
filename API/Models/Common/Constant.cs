public class Constant
{
    public const string FAIL_MESSAGE = "Fail";

    // Success
    public const int SUCCESS_STATUS_CODE = 200;
    public const string SUCCESS_MESSAGE = "Success";

    // Client errors (4xx)
    public const int BAD_REQUEST_STATUS_CODE = 400;
    public const string BAD_REQUEST_MESSAGE = "Bad request";

    public const int UNAUTHORIZED_STATUS_CODE = 401;
    public const string UNAUTHORIZED_MESSAGE = "Unauthorized";

    public const int NOT_FOUND_STATUS_CODE = 404;
    public const string NOT_FOUND_MESSAGE = "Not found";

    // Server errors (5xx)
    public const int INTERNAL_SERVER_ERROR_STATUS_CODE = 500;
    public const string INTERNAL_SERVER_ERROR_MESSAGE = "Internal server error";

    public const int USER_ROLE = 2;
    public const int ADMIN_ROLE = 1;
    public const string USER_ROLE_STRING = "USER";
    public const string ADMIN_ROLE_STRING = "ADMIN";

    // Cache prefix
    public const string USER_CACHE_PREFIX = "users";

    // Status
    public const string PENDING = "pending";
    public const string SUCCESS = "success";
    public const string FAILED = "failed";
    public const string NEW = "new";
}
