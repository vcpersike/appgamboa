using System.ComponentModel.DataAnnotations;

namespace AppGamboa.Shared.Models
{
    public class ContactFormModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O assunto é obrigatório")]
        [StringLength(100, ErrorMessage = "O assunto deve ter no máximo 100 caracteres")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "A mensagem é obrigatória")]
        [StringLength(1000, ErrorMessage = "A mensagem deve ter no máximo 1000 caracteres")]
        public string Message { get; set; }
    }
}