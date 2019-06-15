using System;
using System.ComponentModel.DataAnnotations;

namespace Firepuma.MicroServices.Auth.Services
{
    public class TokenRefreshOptions
    {
        [Required]
        public TimeSpan? Interval { get; set; }
    }
}