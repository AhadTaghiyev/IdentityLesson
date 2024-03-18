using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Identity.FilterAttributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Identity.Dtos;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Mail;
using System.Net;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class IdentityController : ControllerBase
    {
       readonly RoleManager<IdentityRole> _roleManager;
        readonly UserManager<IdentityUser> _userManager;
        readonly IWebHostEnvironment _env;

        public IdentityController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _env = env;
        }

        [HttpPost("")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateRole(string name)
        {
            IdentityRole identityRole = new IdentityRole();
            identityRole.Name = name;
            IdentityResult result =await _roleManager.CreateAsync(identityRole);

            if (!result.Succeeded)
            {
                return StatusCode(400,result.Errors);
            }
            return Ok();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterPostDto user)
        {
            IdentityUser identityUser = new IdentityUser
            {
                Email = user.Email,
                UserName = user.Email,
            };
            var result=  await  _userManager.CreateAsync(identityUser,user.Password);
            if (!result.Succeeded)
            {
                return StatusCode(400, result.Errors);
            }
            return Ok();
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return StatusCode(404, "email or password incorrect");
            }

            if(!await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return StatusCode(404, "email or password incorrect");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a5e281de-4ed8-4e4b-a48a-ecb5b5664c33"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim("Paswword", dto.Password));


            var result= new JwtSecurityToken("https://localhost:7048",
               "https://localhost:7048", claims,
               expires:DateTime.Now.AddMinutes(15),
               signingCredentials: credentials
                );
            var tokenHandeler = new JwtSecurityTokenHandler().WriteToken(result);
            return Ok(tokenHandeler);
        }

        [HttpPost("UpdateUser")]
        [Authorize]
        public async  Task<IActionResult> UpdateUser(UpdateUserDto dto)
        {
            var userName = User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            user.Email = dto.Email;

            await _userManager.UpdateAsync(user);
            if (!string.IsNullOrWhiteSpace(dto.OldPassword) && !string.IsNullOrWhiteSpace(dto.NewPassword))
            {
               var result= await _userManager.ChangePasswordAsync(user,dto.OldPassword,dto.NewPassword);

                if (!result.Succeeded)
                {
                    return StatusCode(400,result.Errors);
                }
            }
            return StatusCode(200);
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return StatusCode(404, "User nor found");
            }

           string token=  await _userManager.GeneratePasswordResetTokenAsync(user);
            return Ok(token);
        }


        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string email,string token,string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return StatusCode(404, "User nor found");
            }
            await _userManager.ResetPasswordAsync(user,token,password);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Test(string name,string surname)
        {
            return Ok(new {name=name,surname=surname});
        }

        [HttpPost("SendMail")]
        public async Task<IActionResult> SendMail(string mail,string subject,string message)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("pm4283719@gmail.com");
            mailMessage.To.Add(new MailAddress(mail));
            mailMessage.Subject = subject;
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = false;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential("pm4283719@gmail.com", "jwqy lsfi okck cvam");
            smtp.Send(mailMessage);

            return Ok();
        }

        [HttpPost("SendTemplate")]
        public async Task<IActionResult> SendTemplate(string mail)
        {
            string path = _env.WebRootPath;

            string ResultPath = Path.Combine(path,"mailtemplates","template.html");

            string Message = System.IO.File.ReadAllText(ResultPath);

            Message = Message.Replace("{{Atl}}", mail);

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("pm4283719@gmail.com");
            mailMessage.To.Add(new MailAddress(mail));
            mailMessage.Subject = "Test";
            mailMessage.Body = Message;
            mailMessage.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential("pm4283719@gmail.com", "jwqy lsfi okck cvam");
            smtp.Send(mailMessage);

            return Ok();
        }


    }
}

