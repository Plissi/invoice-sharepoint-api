using System.ComponentModel.DataAnnotations;

namespace DechargeAPI.Models
{
    public class UserModel
    {
        [Required(ErrorMessage ="Nom d'utilisateur requis")]
        public string Username { get; set; }

        /*[Required(ErrorMessage = "Email requis")]
        public string Email { get; set; }*/

        [Required(ErrorMessage = "Mot de passe requis")]
        public string Password { get; set; }
    }
}
