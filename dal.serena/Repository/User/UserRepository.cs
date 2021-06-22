using biz.serena.Entities;
using biz.serena.Repository;
using dal.serena.DBContext;
using CryptoHelper;
using System;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using biz.serena.Models.Email;
using biz.serena.Servicies;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace dal.serena.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private IConfiguration _config;
        private IEmailService _emailservice;

        public UserRepository(Db_SerenaContext context, IConfiguration config, IEmailService emailService) : base(context)
        {
            _config = config;
            _emailservice = emailService;
        }

        public string HashPassword(string password)
        {
            return Crypto.HashPassword(password);
        }

        public string BuildToken(User user)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        public bool VerifyPassword(string hash, string password)
        {
            return Crypto.VerifyHashedPassword(hash, password);
        }

        public override User Add(User user)
        {
            user.Password = HashPassword(user.Password);
            return base.Add(user);
        }

        public override User Update(User user, object id)
        {
            user.UpdatedDate = DateTime.Now;
            return base.Update(user, id);
        }

        public string SendMail(string emailTo, string body, string subject)
        {
            EmailModel email = new EmailModel();
            email.To = emailTo;
            email.Subject = subject;
            email.Body = body;
            email.IsBodyHtml = true;

            return _emailservice.SendEmail(email);

        }

        public string VerifyEmail(string email)
        {
            var result = "";

            if (_context.Users.SingleOrDefault(x => x.Email.ToLower().Trim() == email.ToLower().Trim()) != null)
            {
                result = "Exist";
            }
            else
            {
                result = "No Exist";
            }

            return result;
        }
    }
}
