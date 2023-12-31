﻿using System;
using AutoMapper;
using Techie.Modal;
using Techie.Repos.Models;

namespace Techie.Helper
{
	public class AutoMapperHandler : Profile
	{
		public AutoMapperHandler()
		{
			// src destination, we are converting the Cutsomer model to dto
			CreateMap<Customer, CustomerModel>()
				.ForMember(item => item.StatusName, opt => opt.MapFrom(item => item.IsActive ? "Active" : "InActive"));
			// src destination, we are converting the dto to Cutsomer model
			CreateMap<CustomerModel, Customer>();

			// src destination, we are converting the User model to dto and vice versa
			// handle the CreatedAt and UpdatedAt fields
			CreateMap<User, UserModel>().ReverseMap();

			CreateMap<UserModel, User>();
		}
	}
}

