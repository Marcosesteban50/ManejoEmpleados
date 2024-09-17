using System.Security.Claims;

namespace ManejoClientes.Data
{
    public class ServicioUsuarios : IServicioUsuarios
    {

        private readonly HttpContext httpContext;
        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext; 
        }

        public string ObtenerUsarioId()
        {
            if(httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims.Where(x =>
                x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                
                if(idClaim == null)
                {
                    throw new ApplicationException("no esta autenticado");
                }

                return idClaim.Value.ToString();


            }
            else
            {
                throw new ApplicationException("no esta autenticado");
            }
        }
    }
}
