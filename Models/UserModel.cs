﻿using System.ComponentModel.DataAnnotations;

namespace DechargeAPI.Models
{
    public class UserModel
    {
        [Required(ErrorMessage ="Nom d'utilisateur requis")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Mot de passe requis")]
        public string MotDePasse { get; set; }

        public string CompteInterneId = "45";

        public string SocieteId = "10";
    }
}
