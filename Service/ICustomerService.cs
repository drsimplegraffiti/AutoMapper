using Techie.Modal;
using Techie.Repos.Models;

namespace Techie.Service
{
	public interface ICustomerService
	{
		Task<List<CustomerModel>> GetAll();
		Task<ApiResponse> AddCustomer(CustomerModel customer);
		Task<CustomerModel> GetById(int id);
		Task<ApiResponse> RemoveById(int id);
		Task<ApiResponse> Update(CustomerModel customer, int id);

	}
}
