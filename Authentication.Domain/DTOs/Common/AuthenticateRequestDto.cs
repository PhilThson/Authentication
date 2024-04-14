using System.ComponentModel.DataAnnotations;

namespace Authentication.Domain.DTOs.Common
{
    public class AuthenticateRequestDto : IParsable<AuthenticateRequestDto>
	{
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        public static AuthenticateRequestDto Parse(string s, IFormatProvider? provider)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException(nameof(s), "No query string provided");
            
            var props = s.Split("&");
            if (props.Length != 2)
                throw new ArgumentException("Invalid arguments count");

            var emailString =
                props.FirstOrDefault(p => p.Equals("email", StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentNullException("Email", "Email is required");
            
            var email = emailString.Split("=").LastOrDefault();
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException("Email", "Email is required");
            
            var passwordString =
                props.FirstOrDefault(p => p.Equals("password", StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentNullException("Password", "Password is required");
            
            var password = passwordString.Split("=").LastOrDefault();
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("Password","Password is required");
                
            return new AuthenticateRequestDto()
            {
                Email = email,
                Password = password
            };
        }

        public static bool TryParse(string? s, IFormatProvider? provider, out AuthenticateRequestDto result)
        {
            result = new AuthenticateRequestDto();
            try
            {
                result = Parse(s, provider);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}

