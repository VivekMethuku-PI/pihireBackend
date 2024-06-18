using System;
using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModel;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System.Linq;
using PiHire.DAL.Models;
using Microsoft.EntityFrameworkCore;
using PiHire.BAL.ViewModels;
using static PiHire.BAL.Common.Types.AppConstants;
using System.Net;
using PiHire.DAL.Entities;
using System.Diagnostics.Metrics;

namespace PiHire.BAL.Repositories
{
    public class CompanyRepository : BaseRepository, ICompanyRepository
    {
        readonly Logger logger;
        public CompanyRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<CompanyRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }

        public async Task<GetResponseViewModel<List<CompanyViewModel>>> GetCompanies()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CompanyViewModel>>();
            var companies = new List<CompanyViewModel>();
            int UserId = Usr.Id;
            try
            {
                

                companies = await (from pu in dbContext.TblParamProcessUnitMasters
                                   select new CompanyViewModel
                                   {
                                       id = pu.Id,
                                       pu_name = pu.PuName,
                                       short_name = pu.ShortName,
                                       date_of_establisment = pu.DateOfEstablisment,
                                       city_name = dbContext.PhCities.Where(x => x.Id == pu.City).Select(x => x.Name).FirstOrDefault(),
                                       state = pu.State,
                                       city = pu.City,
                                       country = pu.Country,
                                       country_name = dbContext.PhCountries.Where(x => x.Id == pu.Country).Select(x => x.Name).FirstOrDefault(),
                                       time_zone = pu.TimeZone,
                                       iso_code = pu.IsoCode,
                                       mobile_number = pu.MobileNumber,
                                       website = pu.Website,
                                       logo = pu.Logo,
                                       Latitude = pu.Latitude,
                                       Longitude = pu.Longitude,
                                       created_date = pu.CreatedDate,
                                       created_by = pu.CreatedBy,
                                       pan_no = pu.PanNo,
                                       gst_no = pu.GstNo,
                                       service_tax_no = pu.ServiceTaxNo,
                                       tin_no = pu.TinNo,
                                       tan_no = pu.TanNo,
                                       PayslipEmail = pu.PayslipEmail,
                                       VAT_Number = pu.VatNumber
                                   }).ToListAsync();

                respModel.SetResult(companies);

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<CompanyViewModel>> GetCompany(int id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CompanyViewModel>();
            var company = new CompanyViewModel();
            try
            {
                company = await (from pu in dbContext.TblParamProcessUnitMasters
                                 where pu.Id == id
                                 select new CompanyViewModel
                                 {
                                     id = pu.Id,
                                     pu_name = pu.PuName,
                                     short_name = pu.ShortName,
                                     date_of_establisment = pu.DateOfEstablisment,
                                     city_name = dbContext.PhCities.Where(x => x.Id == pu.City).Select(x => x.Name).FirstOrDefault(),
                                     state = pu.State,
                                     city = pu.City,
                                     country = pu.Country,
                                     country_name = dbContext.PhCountries.Where(x => x.Id == pu.Country).Select(x => x.Name).FirstOrDefault(),
                                     time_zone = pu.TimeZone,
                                     iso_code = pu.IsoCode,
                                     mobile_number = pu.MobileNumber,
                                     website = pu.Website,
                                     logo = pu.Logo,
                                     Latitude = pu.Latitude,
                                     Longitude = pu.Longitude,
                                     created_date = pu.CreatedDate,
                                     created_by = pu.CreatedBy,
                                     pan_no = pu.PanNo,
                                     gst_no = pu.GstNo,
                                     service_tax_no = pu.ServiceTaxNo,
                                     tin_no = pu.TinNo,
                                     tan_no = pu.TanNo,
                                     PayslipEmail = pu.PayslipEmail,
                                     VAT_Number = pu.VatNumber
                                 }).FirstOrDefaultAsync();

                respModel.SetResult(company);
            }
            catch (Exception ex)
            {
                throw;
            }
            return respModel;
        }
        
        public async Task<GetResponseViewModel<List<Sp_UserType_LocationsModel>>> GetUserTypeLocationsAsync(UserType? userType)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<Sp_UserType_LocationsModel>>();
            try
            {
                //   logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: userType->" + userType, respModel.Meta.RequestID);
                {
                    var response = await dbContext.GetUserTypeLocations((short)userType);
                    respModel.SetResult(response);
                    respModel.Status = true;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "userType->" + userType, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }
        
        public async Task<GetResponseViewModel<List<PuLocationViewModel>>> GetCompanyLocations(int cId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<PuLocationViewModel>>();
            try
            {
                //   logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: companyId->" + cId, respModel.Meta.RequestID);
                if (cId == 0)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Invalid CompanyId", true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "InvalidData:" + respModel.Meta?.Error?.ErrorMessage ?? string.Empty, respModel.Meta.RequestID);
                }
                else
                {
                    var response = await dbContext.GetCompanyLocations(cId);
                    respModel.SetResult(response);
                    respModel.Status = true;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "companyId->" + cId + ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        
        public async Task<GetResponseViewModel<List<PuLocationsModel>>> GetCompanyLocations(int[] cId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<PuLocationsModel>>();
            try
            {
                //   logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: companyId->" + cId, respModel.Meta.RequestID);
                if (cId.Length == 0)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Invalid CompanyId", true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "InvalidData:" + respModel.Meta?.Error?.ErrorMessage ?? string.Empty, respModel.Meta.RequestID);
                }
                else
                {
                    var response = await dbContext.GetCompanyLocations(cId);
                    respModel.SetResult(response);
                    respModel.Status = true;

                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "companyId->" + cId + ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        
        public async Task<GetResponseViewModel<PuLocationViewModel>> GetCompanyLocation(int cId, int locId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<PuLocationViewModel>();
            var puLocationViewModel = new PuLocationViewModel();
            try
            {
                // logger.Log(LogLevel.Debug, LoggingEvents.GetItem, "Start of method: companyId->" + cId, respModel.Meta.RequestID);

                if (cId == 0)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Invalid CompanyId", true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "InvalidData:" + respModel.Meta?.Error?.ErrorMessage ?? string.Empty, respModel.Meta.RequestID);
                }
                else
                {
                    var response = await dbContext.GetCompanyLocations(locId);
                    if (response.Count > 0)
                    {
                        puLocationViewModel = response[0];
                    }
                    respModel.SetResult(puLocationViewModel);
                    respModel.Status = true;

                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "companyId->" + cId + ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<CreateResponseViewModel<int>> CreateUpdateCompany(CreateUpdateProcessUnitViewModel processUnit)
        {
            var respModel = new CreateResponseViewModel<int>();
            try
            {
                var cityDtls = await dbContext.PhCities.Where(x => x.Name == processUnit.city_name).FirstOrDefaultAsync();
                var countryDtls = await dbContext.PhCountries.Where(x => x.Name == processUnit.country_name).FirstOrDefaultAsync();
                TblParamProcessUnitMaster master = null;
                master = await dbContext.TblParamProcessUnitMasters.FirstOrDefaultAsync(x => x.Id == processUnit.id);

                if (master != null)
                {
                    master.City = cityDtls != null ? cityDtls.Id : 0;
                    master.Country = countryDtls != null ? countryDtls.Id : 0;


                    master.CreatedBy = Usr.Id;
                    master.CreatedDate = CurrentTime;
                    master.DateOfEstablisment = processUnit.date_of_establishment;
                    master.IsoCode = processUnit.iso_code;
                    master.Latitude = processUnit.Latitude;
                    master.Logo = processUnit.LogoURL;
                    master.Longitude = processUnit.Longitude;
                    master.PuName = processUnit.pu_name;
                    master.ShortName = processUnit.short_name;
                    master.State = processUnit.state;
                    master.TimeZone = processUnit.time_zone;
                    master.MobileNumber = processUnit.mobile_number;
                    master.Website = processUnit.website;
                    master.VatNumber = processUnit.VAT_Number;

                    master.PanNo = processUnit.pan_no;
                    master.GstNo = processUnit.gst_no;
                    master.ServiceTaxNo = processUnit.service_tax_no;
                    master.TinNo = processUnit.tin_no;
                    master.TanNo = processUnit.tan_no;

                    await dbContext.SaveChangesAsync();

                    respModel.SetResult(master.Id);
                }
                else
                {


                    master = new TblParamProcessUnitMaster();

                    master.Id = processUnit.id;

                    master.City = cityDtls != null ? cityDtls.Id : 0;
                    master.Country = countryDtls != null ? countryDtls.Id : 0;

                    master.CreatedBy = Usr.Id;
                    master.CreatedDate = CurrentTime;
                    master.DateOfEstablisment = processUnit.date_of_establishment;
                    master.IsoCode = processUnit.iso_code;
                    master.Latitude = processUnit.Latitude;
                    master.Logo = processUnit.LogoURL;
                    master.Longitude = processUnit.Longitude;
                    master.MobileNumber = processUnit.mobile_number;
                    master.PuName = processUnit.pu_name;
                    master.ShortName = processUnit.short_name;
                    master.State = processUnit.state;
                    master.TimeZone = processUnit.time_zone;
                    master.Website = processUnit.website;
                    master.CreatedDate = CurrentTime;
                    master.CreatedBy = Usr.Id;
                    master.VatNumber = processUnit.VAT_Number;

                    master.PanNo = processUnit.pan_no;
                    master.GstNo = processUnit.gst_no;
                    master.ServiceTaxNo = processUnit.service_tax_no;
                    master.TinNo = processUnit.tin_no;
                    master.TanNo = processUnit.tan_no;

                    dbContext.TblParamProcessUnitMasters.Add(master);

                    await dbContext.SaveChangesAsync();

                    respModel.SetResult(master.Id);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return respModel;
        }

        public async Task<CreateResponseViewModel<int>> CreateUpdateCompanyBusinessUnit(CreateUpdateProcessUnitBusinessUnitViewModel model)
        {
            var respModel = new CreateResponseViewModel<int>();
            try
            {

                TblParamPuBusinessUnit buDtls = null;

                buDtls = await dbContext.TblParamPuBusinessUnits.FirstOrDefaultAsync(x => x.Id == model.id);
                if (buDtls != null)
                {
                    buDtls.BusUnitCode = model.bus_unit_code;
                    buDtls.BusUnitFullName = model.bus_unit_full_name;
                    buDtls.CreatedDate = CurrentTime;
                    buDtls.Description = model.description;
                    buDtls.PuId = model.pu_id;

                    await dbContext.SaveChangesAsync();

                    respModel.SetResult(buDtls.Id);
                }

                if (buDtls == null)
                {
                    buDtls = await dbContext.TblParamPuBusinessUnits.FirstOrDefaultAsync(x => x.BusUnitCode == model.bus_unit_code);
                    if (buDtls != null)
                    {
                        buDtls.BusUnitCode = model.bus_unit_code;
                        buDtls.BusUnitFullName = model.bus_unit_full_name;
                        buDtls.CreatedDate = CurrentTime;
                        buDtls.Description = model.description;
                        buDtls.PuId = model.pu_id;

                        await dbContext.SaveChangesAsync();

                        respModel.SetResult(buDtls.Id);
                    }
                    else
                    {
                        buDtls = new TblParamPuBusinessUnit();

                        buDtls.Id = model.id;
                        buDtls.BusUnitCode = model.bus_unit_code;
                        buDtls.BusUnitFullName = model.bus_unit_full_name;
                        buDtls.CreatedDate = CurrentTime;
                        buDtls.Description = model.description;
                        buDtls.PuId = model.pu_id;

                        dbContext.TblParamPuBusinessUnits.Add(buDtls);

                        await dbContext.SaveChangesAsync();

                        respModel.SetResult(buDtls.Id);
                    }

                }


            }
            catch (Exception)
            {
                throw;
            }
            return respModel;
        }

        public async Task<CreateResponseViewModel<int>> CreateUpdateCompanyLocation(CreateUpdateProcessUnitLocationViewModel model)
        {
            var respModel = new CreateResponseViewModel<int>();
            try
            {

                var cityDtls = await dbContext.PhCities.Where(x => x.Name == model.city).FirstOrDefaultAsync();
                var countryDtls = await dbContext.PhCountries.Where(x => x.Name == model.country).FirstOrDefaultAsync();

                var pulocation = await dbContext.TblParamPuOfficeLocations.FirstOrDefaultAsync(x => x.Id == model.id);
                if (pulocation != null)
                {
                    pulocation.Address1 = model.address1;
                    pulocation.Address2 = model.address2;
                    pulocation.Address3 = model.address3;
                    pulocation.City = cityDtls != null ? cityDtls.Id : 0;
                    pulocation.ContactPerson = model.contact_person;
                    pulocation.Country = countryDtls != null ? countryDtls.Id : 0;
                    pulocation.CreatedBy = Usr.Id;
                    pulocation.CreatedDate = CurrentTime;
                    pulocation.EmailAddress = model.email_address;
                    pulocation.FaxNo = model.fax_no;
                    pulocation.LandMark = model.land_mark;
                    pulocation.LandNumber = model.land_number;
                    pulocation.Latitude = model.Latitude;
                    pulocation.LocationMap = model.location_map;
                    pulocation.LocationName = model.location_name;
                    pulocation.Longitude = model.Longitude;
                    pulocation.MobileNumber = model.mobile_number;
                    pulocation.Pin = model.pin;
                    pulocation.PuId = model.pu_id;
                    pulocation.State = model.State;
                    pulocation.Website = model.website;
                    pulocation.LocationMapName = model.location_map_name;
                    pulocation.LocationMapType = model.location_map_type;
                    pulocation.IsMainLocation = model.isMainLocation == null ? false : model.isMainLocation;

                    await dbContext.SaveChangesAsync();

                    respModel.SetResult(pulocation.Id);
                }

                else
                {
                    pulocation = new TblParamPuOfficeLocation();

                    pulocation.Id = model.id;
                    pulocation.Address1 = model.address1;
                    pulocation.Address2 = model.address2;
                    pulocation.Address3 = model.address3;
                    pulocation.City = cityDtls != null ? cityDtls.Id : 0;
                    pulocation.ContactPerson = model.contact_person;
                    pulocation.Country = countryDtls != null ? countryDtls.Id : 0;
                    pulocation.CreatedBy = Usr.Id;
                    pulocation.CreatedDate = CurrentTime;
                    pulocation.EmailAddress = model.email_address;
                    pulocation.FaxNo = model.fax_no;
                    pulocation.LandMark = model.land_mark;
                    pulocation.LandNumber = model.land_number;
                    pulocation.Latitude = model.Latitude;
                    pulocation.LocationMap = model.location_map;
                    pulocation.LocationName = model.location_name;
                    pulocation.Longitude = model.Longitude;
                    pulocation.MobileNumber = model.mobile_number;
                    pulocation.Pin = model.pin;
                    pulocation.PuId = model.pu_id;
                    pulocation.State = model.State;
                    pulocation.LocationMapName = model.location_map_name;
                    pulocation.LocationMapType = model.location_map_type;
                    pulocation.Website = model.website;
                    pulocation.IsMainLocation = model.isMainLocation == null ? false : model.isMainLocation;

                    dbContext.TblParamPuOfficeLocations.Add(pulocation);
                    await dbContext.SaveChangesAsync();

                    respModel.SetResult(pulocation.Id);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return respModel;
        }

    }
}
