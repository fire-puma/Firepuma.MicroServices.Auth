using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Firepuma.MicroServices.Auth.OpenIdConnect
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class OpenIdConnectOptions
    {
        [Required]
        public string Authority { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ClientSecret { get; set; }
    }
}