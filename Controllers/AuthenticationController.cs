﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CityInfo.API.Controllers;


[ApiController]
[Route("api/v{version:apiVersion}/authentication")]
[ApiVersion(1)]
[ApiVersion(2)]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    private class CityInfoUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }

        public CityInfoUser(int userId, string userName, string firstName, string lastName, string city)
        {
            UserId = userId;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            City = city;
        }
    }

    public class AuthenticationRequestBody
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public AuthenticationController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    }
    [HttpPost("authenticate")]
    public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
    {
        //validate user
        var user = ValidateUserCredentials(authenticationRequestBody.UserName, authenticationRequestBody.Password);
        if (user == null)
        {
            return Unauthorized();
        }

        //create token
        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:SecretForKey"]));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claimsForToken = new List<Claim>();
        claimsForToken.Add(new Claim("sub", user.UserId.ToString()));
        claimsForToken.Add(new Claim("given_name", user.FirstName.ToString()));
        claimsForToken.Add(new Claim("family_name", user.LastName.ToString()));
        claimsForToken.Add(new Claim("city", user.City.ToString()));

        var jwtSecurityToken = new JwtSecurityToken(
            _configuration["Authentication:Issuer"],
             _configuration["Authentication:Audience"],
             claimsForToken,
             DateTime.UtcNow,
             DateTime.UtcNow.AddHours(1),
             signingCredentials
            );
        var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return Ok(tokenToReturn);
    }

    private CityInfoUser ValidateUserCredentials(string? username, string? password)
    {

        return new CityInfoUser(1,"pjsaha", "pj","saha", "Paris");


    }
}
