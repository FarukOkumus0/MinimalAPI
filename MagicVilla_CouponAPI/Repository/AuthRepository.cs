﻿using AutoMapper;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_CouponAPI.Repository
{
	public class AuthRepository : IAuthRepository
	{
		private readonly ApplicationDbContext _db;
		private readonly IConfiguration _configuration;
		private readonly IMapper _mapper;
		private string secretKey;

		public AuthRepository(ApplicationDbContext db, IMapper mapper, IConfiguration configuration)
		{
			_db = db;
			_mapper = mapper;
			_configuration = configuration;
			secretKey = _configuration.GetValue<string>("ApiSettings:Secret");
		}

		public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
		{
			var user = _db.LocalUsers.SingleOrDefault(x=> x.UserName == loginRequestDto.UserName && x.Password == loginRequestDto.Password);

			if (user == null)
			{
				return null;
			}
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(secretKey);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[] {
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(ClaimTypes.Role, user.Role),
				}),
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			LoginResponseDto loginResponseDto = new()
			{
				User = _mapper.Map<UserDto>(user),
				Token = new JwtSecurityTokenHandler().WriteToken(token)
			};
			return loginResponseDto;
		}

		public bool IsUniqueUser(string username)
		{
			var user = _db.LocalUsers.FirstOrDefault(x => x.UserName == username);
			if (user == null)

				return true;

			return false;
		}

		public async Task<UserDto> Register(RegistrationRequestDto requestDto)
		{
			LocalUser userObj = new()
			{
				UserName = requestDto.UserName,
				Password = requestDto.Password,
				Name = requestDto.Name,
				Role = "customer"
			};
			_db.LocalUsers.Add(userObj);
			_db.SaveChanges();
			userObj.Password = "";
			return _mapper.Map<UserDto>(userObj);
		}
	}
}
