namespace SchoolManagement.Application.Common
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Teacher = "Teacher";
        public const string Parent = "Parent";
        public const string Accountant = "Accountant";

        public static readonly string[] All = { Admin, Teacher, Parent, Accountant };
    }
}