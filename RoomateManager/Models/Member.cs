using System;

namespace RoomateManager.Models 
{
    public enum MemberRole { Manager, Member }
    public enum PaymentStatus { Paid, Unpaid, NotApplicable }

    public class Member
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string AvatarInitial => (Name?.Length > 0) ? Name[0].ToString().ToUpper() : "?";
        public MemberRole Role { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}