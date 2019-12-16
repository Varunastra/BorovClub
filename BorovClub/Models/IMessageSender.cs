namespace BorovClub.Models
{
    public interface IMessageSender
    {
        public ApplicationUser Sender { get; set; }
    }
}
