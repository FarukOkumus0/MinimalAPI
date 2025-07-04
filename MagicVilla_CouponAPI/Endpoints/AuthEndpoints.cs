﻿using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repository.IRepository;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_CouponAPI.Endpoints
{
	public static class AuthEndpoints
	{
		public static void ConfigureAuthEndpoints(this WebApplication app)
		{
			app.MapPost("api/login", Login).WithName("Login").Accepts<LoginRequestDto>("application/json").Produces<APIResponse>(200).Produces(400);
			app.MapPost("api/register", Register).WithName("Register").Accepts<RegistrationRequestDto>("application/json").Produces<APIResponse>(200).Produces(400);

		}
		private async static Task<IResult> Login(IAuthRepository _authRepo,[FromBody] LoginRequestDto model)
		{

			APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
			var loginResponse = await _authRepo.Login(model);
			if (loginResponse == null)
			{
				response.ErrorMessages.Add("Hatalı kullanıcı adı veya şifre");
				return Results.BadRequest(response);
			}
			response.Result = loginResponse;
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.OK;
			return Results.Ok(response);
		}

		private async static Task<IResult> Register(IAuthRepository _authRepo, [FromBody] RegistrationRequestDto model)
		{

			APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

			bool ifUserNameisUnique = _authRepo.IsUniqueUser(model.UserName);
			if (!ifUserNameisUnique)
			{
				response.ErrorMessages.Add("Bu kullanıcı adı zaten var");
				return Results.BadRequest(response);
			}

			var registerResponse = await _authRepo.Register(model);
			if (registerResponse == null || string.IsNullOrEmpty(registerResponse.UserName))
			{
				return Results.BadRequest(response);
			}
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.OK;
			return Results.Ok(response);
		}

	}
}
