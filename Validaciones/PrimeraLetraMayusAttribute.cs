using System.ComponentModel.DataAnnotations;

namespace ManejoClientes.Validaciones
{
    public class PrimeraLetraMayusAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
           if(value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }


           var primeraLetra = value.ToString()![0].ToString();

            if(primeraLetra != primeraLetra.ToUpper())
            {
                return new ValidationResult("Primera letra debe ser mayuscula");
            }

            return ValidationResult.Success;
        }
    }
}
