﻿using ChainingAssertion;
using DotNetCoreWebApiJwtSample.Configs;
using DotNetCoreWebApiJwtSample.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace DotNetCoreWebApiJwtSample.Test.Services
{
    public class JwtServiceTest
    {
        private readonly JwtService _service;
        private readonly JwtConfigurableOptions _options;

        public JwtServiceTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            var appSettings = new AppSettings();
            configuration.Bind(appSettings);

            _options = appSettings.JwtSetting;
            _service = new JwtService(_options);
        }

        [Fact]
        public void GenerateEncodedToken()
        {
            // Arrange
            var roles = new List<string> { "User", "Admin" };
            var userName = "Taro Yamada";

            // Act
            var result = _service.GenerateEncodedToken(userName, roles);
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.JwtIssuer,
                ValidAudience = _options.JwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_options.JwtKey)),
                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero

            };
            handler.ValidateToken(result, validationParameters, out var token);
            var jwtToken = (System.IdentityModel.Tokens.Jwt.JwtSecurityToken)token;

            // Assert
            jwtToken.Issuer.Is(_options.JwtIssuer);
            jwtToken.Audiences.First().Is(_options.JwtAudience);
            jwtToken.Subject.Is(userName);

            var jsonRole = jwtToken.Payload.First(_ => _.Key == ClaimTypes.Role).Value;
            foreach (var role in (JArray)jsonRole)
            {
                roles.Contains(role.ToString()).IsTrue();
            }
        }
    }
}
