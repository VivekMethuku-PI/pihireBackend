using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IBlogsRepository : IBaseRepository
    {
        /// <summary>
        /// returning active blogs
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<BlogsModel>>> GetActiveBlogs();

        /// <summary>
        /// returning testimonials
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<BlogsModel>>> BlogsList();


        /// <summary>
        /// returning testimonial
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<BlogModel>> GetBlog(int Id);


        /// <summary>
        /// creating Blog
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateBlog(CreateBlogModel createBlogModel);

        /// <summary>
        /// updating Blog
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateBlog(UpdateBlogModel updateBlogModel);


        /// <summary>
        /// updating status
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateBlogStatus(UpdateStatusModel updateStatusModel);
    }
}
