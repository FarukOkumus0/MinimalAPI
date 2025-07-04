﻿using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repository.IRepository;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MagicVilla_CouponAPI.Endpoints
{
	public static class CouponEndpoints
	{
		public static void ConfigureCouponEndpoints(this WebApplication app)
		{

			app.MapGet("/api/coupon", GetAllCoupon)
				.WithName("GetCoupons").Produces<APIResponse>(200);/*.RequireAuthorization("AdminOnly");*/

			app.MapGet("/api/coupon{id:int}", GetCoupon).WithName("GetCoupon").Produces<APIResponse>(200)
				.AddEndpointFilter(async (context, next) =>
			{
				var id = context.GetArgument<int>(2);
				if (id == 0)
				{
					return Results.BadRequest("Id değeri 0 olamaz");
				}
				Console.WriteLine("Before 1st Filter");
				var result = await next(context);
				Console.WriteLine("After 1st Filter");
				return result;

			})
				.AddEndpointFilter(async (context, next) =>
				{
					
					Console.WriteLine("Before 2nd Filter");
					var result = await next(context);
					Console.WriteLine("After 2nd Filter");
					return result;

				});

			app.MapPost("api/coupon", CreateCoupon).WithName("CreateCoupon").Accepts<CouponCreateDto>("application/json").Produces<APIResponse>(201).Produces(400);

			app.MapPut("api/coupon", UpdateCoupon).WithName("UpdateCoupon").Accepts<CouponUpdateDto>("application/json").Produces<APIResponse>(200).Produces(400);

			app.MapDelete("api/coupon/{id:int}", DeleteCoupon);

		}


		private async static Task<IResult> GetCoupon(ICouponRepository _couponRepo, ILogger<Program> _logger, int id)
		{
			Console.WriteLine("Endpoint çalıştırıldı");
			APIResponse response = new();
			response.Result = await _couponRepo.GetAsync(id);
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.OK;
			return Results.Ok(response);
		}
		//[Authorize]
		private async static Task<IResult> CreateCoupon(ICouponRepository _couponRepo, IMapper _mapper, IValidator<CouponCreateDto> _validation, [FromBody] CouponCreateDto coupon_C_DTO)
		{

			APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

			var validationResult = await _validation.ValidateAsync(coupon_C_DTO);
			if (!validationResult.IsValid)
			{
				response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
				return Results.BadRequest(response);
			}
			if (_couponRepo.GetAsync(coupon_C_DTO.Name).GetAwaiter().GetResult != null)
			{
				response.ErrorMessages.Add("Kupon Adı Zaten Mevcut");
				return Results.BadRequest(response);
			}

			Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);

			//coupon.Id = CouponStore.couponList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
			await _couponRepo.CreateAsync(coupon);
			await _couponRepo.SaveAsync();
			CouponDto couponDto = _mapper.Map<CouponDto>(coupon);

			response.Result = couponDto;
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.Created;
			return Results.Ok(response);
		}
		//[Authorize]
		private async static Task<IResult> UpdateCoupon(ICouponRepository _couponRepo, IMapper _mapper, IValidator<CouponUpdateDto> _validation, [FromBody] CouponUpdateDto coupon_U_DTO)
		{
			APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

			var validationResult = await _validation.ValidateAsync(coupon_U_DTO);
			if (!validationResult.IsValid)
			{
				response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
				return Results.BadRequest(response);
			}

			await _couponRepo.UpdateAsync(_mapper.Map<Coupon>(coupon_U_DTO));
			await _couponRepo.SaveAsync();

			response.Result = _mapper.Map<CouponDto>(await _couponRepo.GetAsync(coupon_U_DTO.Id));
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.OK;
			return Results.Ok(response);
		}
		//[Authorize]
		private async static Task<IResult> DeleteCoupon(ICouponRepository _couponRepo, int id)
		{
			APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

			Coupon couponFromStore = await _couponRepo.GetAsync(id);
			if (couponFromStore != null)
			{
				await _couponRepo.RemoveAsync(couponFromStore);
				await _couponRepo.SaveAsync();
				response.IsSuccess = true;
				response.StatusCode = HttpStatusCode.NoContent;
				return Results.Ok(response);
			}
			else
			{
				response.ErrorMessages.Add("Invalid Id");
				return Results.BadRequest(response);
			}
		}

		private async static Task<IResult> GetAllCoupon(ICouponRepository _couponRepo, ILogger<Program> _logger)
		{
			APIResponse response = new();
			_logger.Log(LogLevel.Information, "Mevcut Kuponlar Listeleniyor");
			response.Result = await _couponRepo.GetAllAsync();
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.OK;
			return Results.Ok(response);
		}
	}
}
