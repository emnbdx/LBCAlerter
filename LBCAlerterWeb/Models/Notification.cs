
namespace LBCAlerterWeb.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Notification")]
    public class Notification
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [DisplayName("Titre")]
        public string Title { get; set; }
        [DisplayName("Message")]
        public string Message { get; set; }
        public bool Important { get; set; }
        public bool Viewed { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}