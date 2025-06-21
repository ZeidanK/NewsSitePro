using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens; // namespace for SymmetricSecurityKey
using System.IdentityModel.Tokens.Jwt; //  namespace for JwtSecurityToken and JwtSecurityTokenHandler
using Microsoft.AspNetCore.Authorization; //  namespace for Claim


namespace NewsSite.BL
{
    public class User
    {
        
        private int id;
        private string name;
        private string email; 
        private string passwordHash;
        private bool isAdmin;
        private bool isLocked;
        private readonly object _config;
        public User(IConfiguration config)
        {
            _config = config;
        }
        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public string Email { get => email; set => email = value; }
        public string PasswordHash { get => passwordHash; set => passwordHash = value; }
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        public bool IsLocked { get => isLocked; set => isLocked = value; }



        // Constructors

        public User() { }

        public User(int id, string name, string email, string passwordHash, bool isAdmin, bool isLocked)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.passwordHash = passwordHash;
            this.isAdmin = isAdmin;
            this.isLocked = isLocked;
        }



        public bool Register(string name, string email, string password)
{
    DBservices dBservices = new DBservices();
    // Check if user already exists
    var existing = dBservices.GetUser(email, null, null);
    if (existing != null) return false;

    this.Name = name;
    this.Email = email;
    this.PasswordHash = HashPassword(password);
    this.IsAdmin = false;
    this.IsLocked = false;

    // Save user to DB
    return dBservices.CreateUser(this);
}

        public bool UpdateDetails(int id, string name, string password)
        {
            DBservices dBservices = new DBservices();
            var user = dBservices.GetUserById(id);
            if (user == null) return false;

            user.Name = name ?? user.Name;
            if (!string.IsNullOrEmpty(password))
                user.PasswordHash = HashPassword(password);

            return dBservices.UpdateUser(user);
        }

        public string LogIn(string password, string email)
        {
            DBservices dBservices = new DBservices();
            // Retrieve user by email
            var user = dBservices.GetUser(email, null, null);
            if (user == null)
                return null; // User not found

            // Check if user is locked
            if (user.IsLocked)
                return null; // User is locked

            // Hash the provided password and compare
            var hashedInput = HashPassword(password);
            if (user.PasswordHash != hashedInput)
                return null; // Password does not match

            // Set current user properties
            this.Id = user.Id;
            this.Name = user.Name;
            this.Email = user.Email;
            this.PasswordHash = user.PasswordHash;
            this.IsAdmin = user.IsAdmin;
            this.IsLocked = user.IsLocked;

            // Generate JWT
            return GenerateJwtToken();
        }

        // Static Methods

        public User ExtractUserFromJWT(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var idClaim = token.Claims.FirstOrDefault(c => c.Type == "id");
            var nameClaim = token.Claims.FirstOrDefault(c => c.Type == "name");
            var emailClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            var isAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "isAdmin");

            if (idClaim != null)
                this.Id = int.Parse(idClaim.Value);

            if (nameClaim != null)
                this.Name = nameClaim.Value;

            if (emailClaim != null)
                this.Email = emailClaim.Value;

            if (isAdminClaim != null)
                this.IsAdmin = bool.Parse(isAdminClaim.Value);

            // PasswordHash and IsLocked are not present in JWT, so they remain unchanged

            return this;
        }


        private string GenerateJwtToken()
        {
            var key = ((IConfiguration)_config)["Jwt:Key"];
            var issuer = ((IConfiguration)_config)["Jwt:Issuer"];
            var audience = ((IConfiguration)_config)["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, this.Email), // Standard subject claim
        new Claim("id", this.Id.ToString()), // Standard name identifier
        new Claim("name", this.Name),
        
        new Claim("isAdmin", this.IsAdmin.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    



        public static string HashPassword(string password)
        {
            using (var sha512 = SHA512.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha512.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }


    }
}
