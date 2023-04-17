using System.ComponentModel.DataAnnotations;

namespace DechargeAPI.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Nom d'utilisateur requis")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mot de passe requis")]
        public string Password { get; set; }
    }
}
