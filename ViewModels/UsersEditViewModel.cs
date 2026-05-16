using SecureGate.Web.Models;

namespace SecureGate.Web.ViewModels
{
    public class UsersEditViewModel : UsersCreateViewModel
    {
        public int Id { get; set; }
        public string? StudentId { get; set; }
        public string? PhotoPath { get; set; }
        public StudentStatus Status { get; set; }
    }
}
