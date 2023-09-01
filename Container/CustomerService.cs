using System;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Techie.Modal;
using Techie.Repos;
using Techie.Repos.Models;
using Techie.Service;

namespace Techie.Container
{
	public class CustomerService: ICustomerService  // we inherit from the interface
	{
        private readonly LearnDataContext _context;
        private readonly IMapper _mapper;
		public CustomerService(LearnDataContext context, IMapper mapper)
		{
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse> AddCustomer(CustomerModel customerModel)
        {
           try
           {
             // Map the DTO (CustomerModel) to the entity (Customer)
             // we need to convert the customermodel dto to customer entity src is CustomerModel dest is Customer
             // The reason we need to do this is because we need to save the entity to the database, and the CustomerModel dto is not an entity
             var customerEntity = _mapper.Map<Customer>(customerModel);
 
             _context.Customers.Add(customerEntity);
             await _context.SaveChangesAsync();
 
             // Map the saved entity back to a DTO (CustomerModel)
             // we need to convert the customer entity to customermodel dto src is Customer dest is CustomerModel
              _mapper.Map<CustomerModel>(customerEntity);
             return new ApiResponse() {
                 Result = $"Customer with id {customerEntity.Id} was saved successfully",
                 ResponseCode = StatusCodes.Status201Created
             };
           }
           catch (Exception ex)
           {
                return new ApiResponse() {
                     Result = ex.Message,
                     ResponseCode = StatusCodes.Status500InternalServerError
                };
           }
        }


        public async Task<List<CustomerModel>> GetAll()
        {
            // we need to convert the customermodel dto to customer entity src is Customer dest is CustomerModel
            // Retrieve all customers from the database
            var customerEntities = await _context.Customers.ToListAsync();
            if (customerEntities == null)
            {
                return new List<CustomerModel>(); // Return an empty list if no customers are found
            }

            // Map the entities to DTOs (CustomerModel)
            return _mapper.Map<List<CustomerModel>>(customerEntities);      

        }

         public async Task<CustomerModel> GetById(int id)
        {
            var response = new ApiResponse();
            var customerEntity = await _context.Customers.FindAsync(id);
            if (customerEntity == null){
                return null!;
            }
            // we need to convert the customer entity to customermodel dto src is Customer dest is CustomerModel
            var customerModel = _mapper.Map<CustomerModel>(customerEntity);
            return customerModel;

        }


        public async Task<ApiResponse> RemoveById(int id)
        {
            var customerEntity = await _context.Customers.FindAsync(id);
            if (customerEntity == null)
                return new ApiResponse() {
                    Result = $"Customer with id {id} was not found",
                    ResponseCode = StatusCodes.Status404NotFound
                };
            _context.Customers.Remove(customerEntity);

            await _context.SaveChangesAsync();
            return new ApiResponse() {
                Result = $"Customer with id {id} was deleted successfully",
                ResponseCode = StatusCodes.Status200OK
            };
            

        }

        public async Task<ApiResponse> Update(CustomerModel customer, int id)
        {
            var customerEntity = await _context.Customers.FindAsync(id);
            if (customerEntity == null)
                return new ApiResponse() {
                    Result = $"Customer with id {id} was not found",
                    ResponseCode = StatusCodes.Status404NotFound
                };
            // we need to convert the customermodel dto to customer entity src is CustomerModel dest is Customer
            _mapper.Map(customer, customerEntity);
            await _context.SaveChangesAsync();
            return new ApiResponse() {
                Result = $"Customer with id {id} was updated successfully",
                ResponseCode = StatusCodes.Status200OK
            };
        }
    }
}

