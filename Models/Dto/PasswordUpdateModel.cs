namespace Filmzie.Models.Dto
{
    public class PasswordUpdateModel
    {
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}