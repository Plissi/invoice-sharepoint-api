using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DechargeAPI.Models
{
    public class UserModel
    {
        [Required(ErrorMessage ="Nom d'utilisateur requis")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Mot de passe requis")]
        public string MotDePasse { get; set; }

        [DefaultValue("45")]
        public string CompteInterneId { get; set; }

        [DefaultValue("10")]
        public string SocieteId { get; set; }
    }
}
