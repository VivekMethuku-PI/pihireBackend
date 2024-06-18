using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface ITestimonialRepository : IBaseRepository
    {

        /// <summary>
        /// returning active testimonials
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<TestimonialsModel>>> GetActiveTestimonials();

        /// <summary>
        /// returning testimonials
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<TestimonialsModel>>> TestimonialsList();


        /// <summary>
        /// returning testimonial
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<TestimonialModel>> GetTestimonial(int Id);


        /// <summary>
        /// creating testimonial
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<string>> CreateTestimonial(CreateTestimonialModel createTestimonialsModel);



        /// <summary>
        /// updating testimonial
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateTestimonial(UpdateTestimonialModel updateTestimonialsModel);


        /// <summary>
        /// updating status
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> UpdateTestimonialStatus(UpdateStatusModel updateStatusModel);


    }
}
