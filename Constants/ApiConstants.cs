﻿namespace ExpressBase.Mobile.Constants
{
    /// <summary>
    /// Rest api related 
    /// Contains constants for api names
    /// </summary>
    public class ApiConstants
    {
        public const string PROTOCOL = "https://";

        public const string VALIDATE_SOL = "api/validate_solution";

        public const string GET_ACTIONS = "api/get_actions";
        public const string GET_ACTION_INFO = "api/get_action_info";

        public const string GET_SOLUTION_DATA = "api/get_solution_data";
        public const string GET_SOLUTION_DATAv2 = "api/get_solution_datav2";
        public const string GET_VIS_DATA = "api/get_data";
        public const string GET_VIS_DATA_V2 = "api/v2/data/visualization";
        public const string GET_SQL_EXPR_RESULT = "api/get_sql_expr_result";
        public const string GET_PROFILE_DATA = "api/get_profile";
        public const string GET_DATA_FLAT = "api/get_data_flat";
        public const string GET_DATA_PS = "api/get_data_ps";
        public const string GET_PDF = "api/get_pdf";
        public const string PULL_LATEST_AUTOID = "api/pull_latest_autoid";

        public const string PUSH_DATA = "api/push_data";

        public const string AUTHETICATE = "api/auth";
        public const string AUTHETICATE_SSO = "api/auth/sso";
        public const string SEND_AUTH_OTP = "api/send_authentication_otp";
        public const string VERIFY_OTP = "api/verify_otp";
        public const string RESEND_OTP = "api/resend_otp";

        public const int TIMEOUT_STD = 10000;
        public const int TIMEOUT_IMPORT = 30000;
    }
}
